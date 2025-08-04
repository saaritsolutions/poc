using System.Reflection;
using TenantPluginArchitecture.Plugins.Contracts;

// Test if we can load the assembly manually
var assemblyPath = "../TenantPluginArchitecture.Demo/plugins/TenantA/TenantPluginArchitecture.Plugins.TenantA.dll";

try
{
    var assembly = Assembly.LoadFrom(assemblyPath);
    Console.WriteLine($"Assembly loaded: {assembly.FullName}");
    
    var types = assembly.GetTypes();
    Console.WriteLine($"Found {types.Length} types in assembly:");
    
    foreach (var type in types)
    {
        Console.WriteLine($"  - {type.Name} (implements: {string.Join(", ", type.GetInterfaces().Select(i => i.Name))})");
        
        if (typeof(ICustomValidator).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
        {
            Console.WriteLine($"    ✅ This is a valid ICustomValidator implementation");
            
            try
            {
                var instance = Activator.CreateInstance(type) as ICustomValidator;
                if (instance != null)
                {
                    Console.WriteLine($"    ✅ Instance created successfully. TenantId: {instance.TenantId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ❌ Failed to create instance: {ex.Message}");
            }
        }
        
        if (typeof(ICustomFormProcessor).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
        {
            Console.WriteLine($"    ✅ This is a valid ICustomFormProcessor implementation");
            
            try
            {
                var instance = Activator.CreateInstance(type) as ICustomFormProcessor;
                if (instance != null)
                {
                    Console.WriteLine($"    ✅ Instance created successfully. TenantId: {instance.TenantId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ❌ Failed to create instance: {ex.Message}");
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to load assembly: {ex.Message}");
}
