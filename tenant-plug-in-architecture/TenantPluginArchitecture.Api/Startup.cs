using TenantPluginArchitecture.Core;
using TenantPluginArchitecture.Plugins.Contracts;

namespace TenantPluginArchitecture.Api;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Add controllers
        services.AddControllers();

        // Add API documentation
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { 
                Title = "Tenant Plugin Architecture API", 
                Version = "v1",
                Description = "Multi-tenant application with dynamic plugin loading"
            });
        });

        // Configure logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        // Register plugin system as singletons for performance
        services.AddSingleton<IPluginLoader, PluginLoader>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<PluginLoader>>();
            var pluginDirectory = Configuration["PluginSettings:PluginDirectory"] ?? "plugins";
            var configuration = new PluginConfiguration
            {
                PluginsDirectory = pluginDirectory,
                EnableHotReload = true,
                CacheTimeout = TimeSpan.FromMinutes(30)
            };
            return new PluginLoader(logger, configuration);
        });

        // Register form and workflow engines
        services.AddSingleton<IFormEngine, FormEngine>();
        services.AddSingleton<IWorkflowEngine, WorkflowEngine>();

        // Add CORS if needed
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tenant Plugin Architecture API v1");
                c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
            });
        }

        app.UseRouting();
        app.UseCors();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            
            // Add health check endpoint
            endpoints.MapGet("/health", () => Results.Ok(new { 
                Status = "Healthy", 
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0"
            }));
        });
    }
}
