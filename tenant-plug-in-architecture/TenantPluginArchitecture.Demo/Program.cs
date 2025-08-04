using TenantPluginArchitecture.Core;
using TenantPluginArchitecture.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace TenantPluginArchitecture.Demo;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Tenant Plugin Architecture Demo ===\n");

        // Setup plugin directory
        var pluginDirectory = Path.Combine(Directory.GetCurrentDirectory(), "plugins");
        Directory.CreateDirectory(pluginDirectory);

        // Copy TenantA plugin to plugins directory
        await CopyTenantAPlugin(pluginDirectory);

        // Create logger (simple console logger for demo)
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole().SetMinimumLevel(LogLevel.Information);
        });
        
        var pluginLoaderLogger = loggerFactory.CreateLogger<PluginLoader>();
        var formEngineLogger = loggerFactory.CreateLogger<FormEngine>();
        var workflowEngineLogger = loggerFactory.CreateLogger<WorkflowEngine>();

        // Initialize plugin loader and engines
        var pluginConfiguration = new PluginConfiguration
        {
            PluginsDirectory = pluginDirectory,
            EnableHotReload = true,
            CacheTimeout = TimeSpan.FromMinutes(30)
        };
        
        var pluginLoader = new PluginLoader(pluginLoaderLogger, pluginConfiguration);
        var formEngine = new FormEngine(pluginLoader, formEngineLogger);
        var workflowEngine = new WorkflowEngine(pluginLoader, workflowEngineLogger);

        Console.WriteLine("🔌 Plugin system initialized");
        Console.WriteLine($"📁 Plugin directory: {pluginDirectory}\n");

        // Demo 1: Check plugin availability for different tenants
        await DemoPluginAvailability(pluginLoader);

        // Demo 2: Form validation and processing
        await DemoFormProcessing(formEngine);

        // Demo 3: Workflow execution
        await DemoWorkflowExecution(workflowEngine);

        Console.WriteLine("\n✅ Demo completed successfully!");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static Task CopyTenantAPlugin(string pluginDirectory)
    {
        try
        {
            var tenantADirectory = Path.Combine(pluginDirectory, "TenantA");
            Directory.CreateDirectory(tenantADirectory);

            // In a real scenario, this would be the compiled DLL
            // For demo purposes, we'll create a placeholder file
            var pluginFilePath = Path.Combine(tenantADirectory, "TenantPluginArchitecture.Plugins.TenantA.dll");
            
            if (!File.Exists(pluginFilePath))
            {
                Console.WriteLine($"📋 Note: In production, TenantA plugin DLL would be at: {pluginFilePath}");
                Console.WriteLine("📋 For this demo, the plugin is loaded from the compiled assembly.\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Warning: Could not set up plugin directory: {ex.Message}");
        }
        
        return Task.CompletedTask;
    }

    private static async Task DemoPluginAvailability(IPluginLoader pluginLoader)
    {
        Console.WriteLine("🔍 === Plugin Availability Demo ===");

        var tenants = new[] { "TenantA", "TenantB", "TenantC" };

        foreach (var tenant in tenants)
        {
            Console.WriteLine($"\n🏢 Checking plugins for {tenant}:");
            
            var hasValidator = await pluginLoader.IsPluginAvailableAsync<ICustomValidator>(tenant);
            var hasFormProcessor = await pluginLoader.IsPluginAvailableAsync<ICustomFormProcessor>(tenant);
            var hasWorkflowHandler = await pluginLoader.IsPluginAvailableAsync<ICustomWorkflowHandler>(tenant);

            Console.WriteLine($"  ✓ Validator: {(hasValidator ? "Available" : "Not Available")}");
            Console.WriteLine($"  ✓ Form Processor: {(hasFormProcessor ? "Available" : "Not Available")}");
            Console.WriteLine($"  ✓ Workflow Handler: {(hasWorkflowHandler ? "Available" : "Not Available")}");
        }

        Console.WriteLine();
    }

    private static async Task DemoFormProcessing(IFormEngine formEngine)
    {
        Console.WriteLine("📝 === Form Processing Demo ===\n");

        // Demo form data for TenantA
        var tenantAFormData = new TenantPluginArchitecture.Core.FormData
        {
            FormId = Guid.NewGuid().ToString(),
            FormType = "UserRegistration",
            TenantId = "TenantA",
            UserId = "demo-user",
            Fields = new Dictionary<string, object>
            {
                ["email"] = "user@company.com",
                ["age"] = 25,
                ["name"] = "John Doe",
                ["phone"] = "1234567890"
            },
            SubmittedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        Console.WriteLine("📋 Form Data for TenantA:");
        Console.WriteLine($"  📧 Email: {tenantAFormData.Fields["email"]}");
        Console.WriteLine($"  👤 Name: {tenantAFormData.Fields["name"]}");
        Console.WriteLine($"  🎂 Age: {tenantAFormData.Fields["age"]}");
        Console.WriteLine($"  📞 Phone: {tenantAFormData.Fields["phone"]}");

        // Validate form
        Console.WriteLine("\n🔍 Validating form...");
        var validationResult = await formEngine.ValidateFormAsync(tenantAFormData);
        
        Console.WriteLine($"✅ Validation Result: {(validationResult.IsValid ? "Valid" : "Invalid")}");
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                Console.WriteLine($"  ❌ {error.Field}: {error.Message}");
            }
        }

        // Submit form if valid
        if (validationResult.IsValid)
        {
            Console.WriteLine("\n📤 Submitting form...");
            var submissionResult = await formEngine.SubmitFormAsync(tenantAFormData);
            
            Console.WriteLine($"📋 Submission Result: {(submissionResult.IsSuccessful ? "Success" : "Failed")}");
            if (submissionResult.IsSuccessful && submissionResult.ProcessedData != null)
            {
                Console.WriteLine($"✨ Processed Data: {submissionResult.ProcessedData}");
            }
        }

        // Demo with invalid data
        Console.WriteLine("\n❌ Testing with invalid data (TenantA):");
        var invalidFormData = new TenantPluginArchitecture.Core.FormData
        {
            FormId = Guid.NewGuid().ToString(),
            FormType = "UserRegistration",
            TenantId = "TenantA",
            UserId = "demo-user",
            Fields = new Dictionary<string, object>
            {
                ["email"] = "invalid@gmail.com", // TenantA doesn't allow gmail
                ["age"] = 16, // Too young for TenantA
                ["name"] = "Jane Doe",
                ["phone"] = "1234567890"
            }
        };

        var invalidValidation = await formEngine.ValidateFormAsync(invalidFormData);
        Console.WriteLine($"🔍 Validation Result: {(invalidValidation.IsValid ? "Valid" : "Invalid")}");
        foreach (var error in invalidValidation.Errors)
        {
            Console.WriteLine($"  ❌ {error.Field}: {error.Message}");
        }

        // Demo for tenant without plugins
        Console.WriteLine("\n🏢 Testing with TenantB (no custom plugins):");
        var tenantBFormData = new TenantPluginArchitecture.Core.FormData
        {
            FormId = tenantAFormData.FormId,
            FormType = tenantAFormData.FormType,
            TenantId = "TenantB",
            UserId = tenantAFormData.UserId,
            Fields = new Dictionary<string, object>(tenantAFormData.Fields),
            SubmittedAt = tenantAFormData.SubmittedAt,
            Status = tenantAFormData.Status
        };
        
        var tenantBValidation = await formEngine.ValidateFormAsync(tenantBFormData);
        Console.WriteLine($"✅ TenantB Validation (default): {(tenantBValidation.IsValid ? "Valid" : "Invalid")}");

        Console.WriteLine();
    }

    private static async Task DemoWorkflowExecution(IWorkflowEngine workflowEngine)
    {
        Console.WriteLine("⚙️ === Workflow Execution Demo ===\n");

        var inputData = new Dictionary<string, object>
        {
            ["customerType"] = "premium",
            ["orderValue"] = 1500.00,
            ["region"] = "North America"
        };

        Console.WriteLine("🚀 Starting workflow for TenantA...");
        Console.WriteLine($"📊 Input Data:");
        foreach (var kvp in inputData)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        var workflowResult = await workflowEngine.StartWorkflowAsync(
            "TenantA", 
            "OrderProcessing", 
            "demo-user", 
            inputData);

        Console.WriteLine($"\n⚙️  Workflow Result: {(workflowResult.IsSuccessful ? "Success" : "Failed")}");
        Console.WriteLine($" Next Step: {workflowResult.NextStep ?? "Completed"}");
        
        if (workflowResult.OutputData.Any())
        {
            Console.WriteLine($"📄 Output Data:");
            foreach (var kvp in workflowResult.OutputData)
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
            }
        }

        if (workflowResult.Errors.Any())
        {
            Console.WriteLine("❌ Errors:");
            foreach (var error in workflowResult.Errors)
            {
                Console.WriteLine($"  • {error}");
            }
        }

        // Demo workflow execution for tenant without custom handler
        Console.WriteLine("\n🏢 Testing workflow with TenantB (no custom handler):");
        var tenantBWorkflow = await workflowEngine.StartWorkflowAsync(
            "TenantB", 
            "OrderProcessing", 
            "demo-user", 
            inputData);

        Console.WriteLine($"⚙️  TenantB Workflow (default): {(tenantBWorkflow.IsSuccessful ? "Success" : "Failed")}");
        Console.WriteLine($"📍 Next Step: {tenantBWorkflow.NextStep ?? "Completed"}");

        Console.WriteLine();
    }
}
