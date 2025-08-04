using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using TenantPluginArchitecture.Plugins.Contracts;

namespace TenantPluginArchitecture.Core;

/// <summary>
/// Interface for loading tenant-specific plugins
/// </summary>
public interface IPluginLoader
{
    /// <summary>
    /// Load a plugin of type T for the specified tenant
    /// </summary>
    Task<T?> LoadPluginAsync<T>(string tenantId) where T : class, ITenantPlugin;
    
    /// <summary>
    /// Load all plugins of type T for the specified tenant
    /// </summary>
    Task<IEnumerable<T>> LoadAllPluginsAsync<T>(string tenantId) where T : class, ITenantPlugin;
    
    /// <summary>
    /// Check if a plugin is available for the tenant
    /// </summary>
    Task<bool> IsPluginAvailableAsync<T>(string tenantId) where T : class, ITenantPlugin;
    
    /// <summary>
    /// Reload plugins for a specific tenant (useful for plugin updates)
    /// </summary>
    Task ReloadTenantPluginsAsync(string tenantId);
}

/// <summary>
/// Plugin metadata for discovery and loading
/// </summary>
public class PluginMetadata
{
    public string TenantId { get; set; } = string.Empty;
    public string PluginType { get; set; } = string.Empty;
    public string AssemblyPath { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public DateTime LastModified { get; set; }
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Plugin configuration for tenant-specific settings
/// </summary>
public class PluginConfiguration
{
    public string PluginsDirectory { get; set; } = "Plugins";
    public bool EnableHotReload { get; set; } = true;
    public TimeSpan CacheTimeout { get; set; } = TimeSpan.FromMinutes(30);
    public Dictionary<string, Dictionary<string, object>> TenantConfigurations { get; set; } = new();
}

/// <summary>
/// Custom assembly load context for plugin isolation
/// </summary>
public class PluginAssemblyLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginAssemblyLoadContext(string pluginPath) : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath != null ? LoadUnmanagedDllFromPath(libraryPath) : IntPtr.Zero;
    }
}

/// <summary>
/// Plugin loader implementation with caching and tenant isolation
/// </summary>
public class PluginLoader : IPluginLoader, IDisposable
{
    private readonly ILogger<PluginLoader> _logger;
    private readonly PluginConfiguration _configuration;
    private readonly Dictionary<string, Dictionary<Type, List<object>>> _pluginCache;
    private readonly Dictionary<string, PluginAssemblyLoadContext> _loadContexts;
    private readonly Dictionary<string, DateTime> _lastLoadTimes;
    private readonly SemaphoreSlim _loadSemaphore;
    private bool _disposed;

    public PluginLoader(ILogger<PluginLoader> logger, PluginConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _pluginCache = new Dictionary<string, Dictionary<Type, List<object>>>();
        _loadContexts = new Dictionary<string, PluginAssemblyLoadContext>();
        _lastLoadTimes = new Dictionary<string, DateTime>();
        _loadSemaphore = new SemaphoreSlim(1, 1);
    }

    public async Task<T?> LoadPluginAsync<T>(string tenantId) where T : class, ITenantPlugin
    {
        var plugins = await LoadAllPluginsAsync<T>(tenantId);
        return plugins.FirstOrDefault();
    }

    public async Task<IEnumerable<T>> LoadAllPluginsAsync<T>(string tenantId) where T : class, ITenantPlugin
    {
        await _loadSemaphore.WaitAsync();
        try
        {
            var pluginType = typeof(T);
            
            // Check cache first
            if (_pluginCache.TryGetValue(tenantId, out var tenantCache) &&
                tenantCache.TryGetValue(pluginType, out var cachedPlugins))
            {
                var shouldReload = _configuration.EnableHotReload && 
                                   ShouldReloadPlugins(tenantId);
                
                if (!shouldReload)
                {
                    _logger.LogDebug("Returning cached plugins for tenant {TenantId}, type {PluginType}", 
                                   tenantId, pluginType.Name);
                    return cachedPlugins.Cast<T>();
                }
            }

            // Load plugins from disk
            var loadedPlugins = await LoadPluginsFromDiskAsync<T>(tenantId);
            
            // Update cache
            if (!_pluginCache.ContainsKey(tenantId))
                _pluginCache[tenantId] = new Dictionary<Type, List<object>>();
            
            _pluginCache[tenantId][pluginType] = loadedPlugins.Cast<object>().ToList();
            _lastLoadTimes[tenantId] = DateTime.UtcNow;

            _logger.LogInformation("Loaded {Count} plugins for tenant {TenantId}, type {PluginType}", 
                                 loadedPlugins.Count(), tenantId, pluginType.Name);

            return loadedPlugins;
        }
        finally
        {
            _loadSemaphore.Release();
        }
    }

    public async Task<bool> IsPluginAvailableAsync<T>(string tenantId) where T : class, ITenantPlugin
    {
        var plugins = await LoadAllPluginsAsync<T>(tenantId);
        return plugins.Any();
    }

    public async Task ReloadTenantPluginsAsync(string tenantId)
    {
        await _loadSemaphore.WaitAsync();
        try
        {
            _logger.LogInformation("Reloading plugins for tenant {TenantId}", tenantId);
            
            // Unload existing context
            if (_loadContexts.TryGetValue(tenantId, out var existingContext))
            {
                existingContext.Unload();
                _loadContexts.Remove(tenantId);
            }
            
            // Clear cache
            _pluginCache.Remove(tenantId);
            _lastLoadTimes.Remove(tenantId);
            
            _logger.LogInformation("Successfully reloaded plugins for tenant {TenantId}", tenantId);
        }
        finally
        {
            _loadSemaphore.Release();
        }
    }

    private async Task<IEnumerable<T>> LoadPluginsFromDiskAsync<T>(string tenantId) where T : class, ITenantPlugin
    {
        var plugins = new List<T>();
        var tenantPluginPath = Path.Combine(_configuration.PluginsDirectory, tenantId);
        
        if (!Directory.Exists(tenantPluginPath))
        {
            _logger.LogWarning("Plugin directory not found for tenant {TenantId}: {Path}", 
                             tenantId, tenantPluginPath);
            return plugins;
        }

        var assemblyFiles = Directory.GetFiles(tenantPluginPath, "*.dll", SearchOption.TopDirectoryOnly);
        
        foreach (var assemblyFile in assemblyFiles)
        {
            try
            {
                // Ensure we have an absolute path
                var absolutePath = Path.IsPathRooted(assemblyFile) ? assemblyFile : Path.GetFullPath(assemblyFile);
                var loadedPlugins = await LoadPluginsFromAssemblyAsync<T>(tenantId, absolutePath);
                plugins.AddRange(loadedPlugins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load plugins from assembly {AssemblyFile} for tenant {TenantId}", 
                               assemblyFile, tenantId);
            }
        }

        return plugins;
    }

    private async Task<IEnumerable<T>> LoadPluginsFromAssemblyAsync<T>(string tenantId, string assemblyPath) 
        where T : class, ITenantPlugin
    {
        var plugins = new List<T>();
        
        // Create or get load context
        if (!_loadContexts.TryGetValue(tenantId, out var loadContext))
        {
            loadContext = new PluginAssemblyLoadContext(assemblyPath);
            _loadContexts[tenantId] = loadContext;
        }

        var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
        var pluginTypes = assembly.GetTypes()
            .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var pluginType in pluginTypes)
        {
            try
            {
                if (Activator.CreateInstance(pluginType) is T plugin)
                {
                    // Verify tenant ID matches
                    if (plugin.TenantId == tenantId)
                    {
                        // Initialize plugin with configuration
                        var tenantConfig = GetTenantConfiguration(tenantId);
                        await plugin.InitializeAsync(tenantConfig);
                        
                        plugins.Add(plugin);
                        
                        _logger.LogDebug("Loaded plugin {PluginName} v{Version} for tenant {TenantId}", 
                                       plugin.Name, plugin.Version, tenantId);
                    }
                    else
                    {
                        _logger.LogWarning("Plugin {PluginType} tenant ID mismatch. Expected: {ExpectedTenant}, Actual: {ActualTenant}", 
                                         pluginType.Name, tenantId, plugin.TenantId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create instance of plugin type {PluginType} for tenant {TenantId}", 
                               pluginType.Name, tenantId);
            }
        }

        return plugins;
    }

    private bool ShouldReloadPlugins(string tenantId)
    {
        if (!_lastLoadTimes.TryGetValue(tenantId, out var lastLoadTime))
            return true;
            
        return DateTime.UtcNow - lastLoadTime > _configuration.CacheTimeout;
    }

    private Dictionary<string, object> GetTenantConfiguration(string tenantId)
    {
        _configuration.TenantConfigurations.TryGetValue(tenantId, out var config);
        return config ?? new Dictionary<string, object>();
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _loadSemaphore?.Dispose();
        
        foreach (var context in _loadContexts.Values)
        {
            context?.Unload();
        }
        
        _loadContexts.Clear();
        _pluginCache.Clear();
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
