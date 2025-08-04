using TenantPluginArchitecture.Plugins.Contracts;

namespace TenantPluginArchitecture.Plugins.TenantA;

/// <summary>
/// Custom validator implementation for Tenant A
/// Implements business-specific validation rules
/// </summary>
public class TenantAValidator : ICustomValidator
{
    public string TenantId => "TenantA";
    public string Name => "Tenant A Custom Validator";
    public string Version => "1.0.0";

    private Dictionary<string, object> _configuration = new();

    public async Task InitializeAsync(IDictionary<string, object> configuration)
    {
        _configuration = new Dictionary<string, object>(configuration);
        
        // Simulate async initialization (e.g., loading config from database)
        await Task.Delay(100);
        
        Console.WriteLine($"Initialized {Name} for tenant {TenantId}");
    }

    public async Task<ValidationResult> ValidateAsync(object data, ValidationContext context)
    {
        var result = new ValidationResult { IsValid = true };
        
        if (data is not Dictionary<string, object> formData)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                Field = "form",
                Message = "Invalid form data format",
                Code = "INVALID_FORMAT"
            });
            return result;
        }

        // Tenant A specific validation rules
        await ValidateRequiredFields(formData, result);
        await ValidateBusinessRules(formData, result, context);
        await ValidateDataIntegrity(formData, result);

        return result;
    }

    public async Task<IEnumerable<ValidationRule>> GetValidationRulesAsync(string formType)
    {
        await Task.CompletedTask;
        
        return formType switch
        {
            "customer-registration" => GetCustomerRegistrationRules(),
            "order-form" => GetOrderFormRules(),
            "support-ticket" => GetSupportTicketRules(),
            _ => new List<ValidationRule>()
        };
    }

    private async Task ValidateRequiredFields(Dictionary<string, object> formData, ValidationResult result)
    {
        await Task.CompletedTask;
        
        var requiredFields = new[] { "email", "firstName", "lastName" };
        
        foreach (var field in requiredFields)
        {
            if (!formData.ContainsKey(field) || 
                formData[field] == null || 
                string.IsNullOrWhiteSpace(formData[field].ToString()))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    Field = field,
                    Message = $"{field} is required for Tenant A",
                    Code = "REQUIRED_FIELD"
                });
            }
        }
    }

    private async Task ValidateBusinessRules(Dictionary<string, object> formData, ValidationResult result, ValidationContext context)
    {
        await Task.CompletedTask;
        
        // Tenant A specific business rule: Email domain must be from approved list
        if (formData.TryGetValue("email", out var emailObj) && emailObj is string email)
        {
            var approvedDomains = new[] { "@company-a.com", "@partner-a.com", "@tenant-a.org" };
            
            if (!approvedDomains.Any(domain => email.EndsWith(domain, StringComparison.OrdinalIgnoreCase)))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    Field = "email",
                    Message = "Email must be from an approved domain for Tenant A",
                    Code = "INVALID_EMAIL_DOMAIN",
                    Value = email
                });
            }
        }

        // Age validation for Tenant A
        if (formData.TryGetValue("age", out var ageObj) && int.TryParse(ageObj.ToString(), out var age))
        {
            if (age < 18)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    Field = "age",
                    Message = "Tenant A requires users to be at least 18 years old",
                    Code = "AGE_RESTRICTION",
                    Value = age
                });
            }
        }
    }

    private async Task ValidateDataIntegrity(Dictionary<string, object> formData, ValidationResult result)
    {
        await Task.CompletedTask;
        
        // Tenant A specific data integrity checks
        if (formData.TryGetValue("phone", out var phoneObj) && phoneObj is string phone)
        {
            // Tenant A requires specific phone format
            if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\+1\d{10}$"))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    Field = "phone",
                    Message = "Tenant A requires phone numbers in format +1XXXXXXXXXX",
                    Code = "INVALID_PHONE_FORMAT",
                    Value = phone
                });
            }
        }
    }

    private static List<ValidationRule> GetCustomerRegistrationRules()
    {
        return new List<ValidationRule>
        {
            new ValidationRule
            {
                Field = "email",
                RuleType = "email",
                ErrorMessage = "Valid email address is required",
                Priority = 1,
                IsActive = true
            },
            new ValidationRule
            {
                Field = "firstName",
                RuleType = "required",
                ErrorMessage = "First name is required",
                Priority = 1,
                IsActive = true
            },
            new ValidationRule
            {
                Field = "lastName",
                RuleType = "required",
                ErrorMessage = "Last name is required",
                Priority = 1,
                IsActive = true
            },
            new ValidationRule
            {
                Field = "age",
                RuleType = "minimum",
                RuleValue = 18,
                ErrorMessage = "Must be at least 18 years old",
                Priority = 2,
                IsActive = true
            }
        };
    }

    private static List<ValidationRule> GetOrderFormRules()
    {
        return new List<ValidationRule>
        {
            new ValidationRule
            {
                Field = "productId",
                RuleType = "required",
                ErrorMessage = "Product selection is required",
                Priority = 1,
                IsActive = true
            },
            new ValidationRule
            {
                Field = "quantity",
                RuleType = "minimum",
                RuleValue = 1,
                ErrorMessage = "Quantity must be at least 1",
                Priority = 1,
                IsActive = true
            },
            new ValidationRule
            {
                Field = "deliveryAddress",
                RuleType = "required",
                ErrorMessage = "Delivery address is required",
                Priority = 2,
                IsActive = true
            }
        };
    }

    private static List<ValidationRule> GetSupportTicketRules()
    {
        return new List<ValidationRule>
        {
            new ValidationRule
            {
                Field = "subject",
                RuleType = "required",
                ErrorMessage = "Subject is required",
                Priority = 1,
                IsActive = true
            },
            new ValidationRule
            {
                Field = "description",
                RuleType = "minLength",
                RuleValue = 10,
                ErrorMessage = "Description must be at least 10 characters",
                Priority = 1,
                IsActive = true
            },
            new ValidationRule
            {
                Field = "priority",
                RuleType = "required",
                ErrorMessage = "Priority level is required",
                Priority = 1,
                IsActive = true
            }
        };
    }
}

/// <summary>
/// Custom form processor implementation for Tenant A
/// </summary>
public class TenantAFormProcessor : ICustomFormProcessor
{
    public string TenantId => "TenantA";
    public string Name => "Tenant A Form Processor";
    public string Version => "1.0.0";

    private Dictionary<string, object> _configuration = new();

    public async Task InitializeAsync(IDictionary<string, object> configuration)
    {
        _configuration = new Dictionary<string, object>(configuration);
        await Task.Delay(50);
        Console.WriteLine($"Initialized {Name} for tenant {TenantId}");
    }

    public async Task<FormProcessingResult> ProcessFormAsync(string formType, object formData, ValidationContext context)
    {
        await Task.CompletedTask;
        
        var result = new FormProcessingResult
        {
            IsSuccessful = true,
            FormId = context.FormId
        };

        try
        {
            // Tenant A specific processing logic
            var processedData = formType switch
            {
                "customer-registration" => await ProcessCustomerRegistration(formData, context),
                "order-form" => await ProcessOrderForm(formData, context),
                "support-ticket" => await ProcessSupportTicket(formData, context),
                _ => formData
            };

            result.ProcessedData = processedData;
            result.Messages.Add($"Form {formType} processed successfully for Tenant A");
            
            // Add tenant-specific metadata
            result.Metadata["processedBy"] = "TenantAFormProcessor";
            result.Metadata["processingTime"] = DateTime.UtcNow;
            result.Metadata["tenantSpecificId"] = Guid.NewGuid().ToString();
        }
        catch (Exception ex)
        {
            result.IsSuccessful = false;
            result.Errors.Add(new ValidationError
            {
                Message = $"Processing error: {ex.Message}",
                Code = "PROCESSING_ERROR"
            });
        }

        return result;
    }

    public async Task<object> TransformFormDataAsync(string formType, object rawData)
    {
        await Task.CompletedTask;
        
        if (rawData is not Dictionary<string, object> data)
            return rawData;

        // Tenant A specific transformations
        var transformed = new Dictionary<string, object>(data);

        // Add tenant-specific fields
        transformed["tenantId"] = TenantId;
        transformed["processedAt"] = DateTime.UtcNow;
        
        // Normalize data for Tenant A
        if (data.TryGetValue("phone", out var phone))
        {
            // Ensure phone numbers are in Tenant A format
            transformed["phone"] = NormalizePhoneNumber(phone?.ToString() ?? "");
        }

        if (data.TryGetValue("email", out var email))
        {
            // Normalize email to lowercase for Tenant A
            transformed["email"] = email?.ToString()?.ToLowerInvariant() ?? "";
        }

        return transformed;
    }

    public async Task<FormConfiguration> GetFormConfigurationAsync(string formType)
    {
        await Task.CompletedTask;
        
        return formType switch
        {
            "customer-registration" => GetCustomerRegistrationConfig(),
            "order-form" => GetOrderFormConfig(),
            "support-ticket" => GetSupportTicketConfig(),
            _ => new FormConfiguration
            {
                FormType = formType,
                DisplayName = formType,
                IsActive = true
            }
        };
    }

    private async Task<object> ProcessCustomerRegistration(object formData, ValidationContext context)
    {
        await Task.CompletedTask;
        
        if (formData is Dictionary<string, object> data)
        {
            // Tenant A specific customer processing
            data["customerId"] = $"CUST-A-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
            data["registrationSource"] = "TenantA";
            data["accountStatus"] = "Active";
        }
        
        return formData;
    }

    private async Task<object> ProcessOrderForm(object formData, ValidationContext context)
    {
        await Task.CompletedTask;
        
        if (formData is Dictionary<string, object> data)
        {
            // Tenant A specific order processing
            data["orderId"] = $"ORD-A-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";
            data["orderStatus"] = "Pending";
            data["tenantProcessingFee"] = 2.99; // Tenant A specific fee
        }
        
        return formData;
    }

    private async Task<object> ProcessSupportTicket(object formData, ValidationContext context)
    {
        await Task.CompletedTask;
        
        if (formData is Dictionary<string, object> data)
        {
            // Tenant A specific support ticket processing
            data["ticketId"] = $"TICK-A-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
            data["assignedTeam"] = "TenantA-Support";
            data["slaHours"] = 24; // Tenant A SLA
        }
        
        return formData;
    }

    private static string NormalizePhoneNumber(string phone)
    {
        // Remove all non-digit characters
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        
        // Add +1 prefix if not present and has 10 digits
        if (digits.Length == 10)
        {
            return $"+1{digits}";
        }
        
        return phone; // Return original if cannot normalize
    }

    private static FormConfiguration GetCustomerRegistrationConfig()
    {
        return new FormConfiguration
        {
            FormType = "customer-registration",
            DisplayName = "Customer Registration (Tenant A)",
            Description = "Register a new customer for Tenant A",
            IsActive = true,
            Fields = new List<FormField>
            {
                new FormField { Name = "firstName", DisplayName = "First Name", FieldType = "text", IsRequired = true },
                new FormField { Name = "lastName", DisplayName = "Last Name", FieldType = "text", IsRequired = true },
                new FormField { Name = "email", DisplayName = "Email Address", FieldType = "email", IsRequired = true },
                new FormField { Name = "phone", DisplayName = "Phone Number", FieldType = "tel", IsRequired = false },
                new FormField { Name = "age", DisplayName = "Age", FieldType = "number", IsRequired = true }
            }
        };
    }

    private static FormConfiguration GetOrderFormConfig()
    {
        return new FormConfiguration
        {
            FormType = "order-form",
            DisplayName = "Order Form (Tenant A)",
            Description = "Place an order with Tenant A",
            IsActive = true,
            Fields = new List<FormField>
            {
                new FormField { Name = "productId", DisplayName = "Product", FieldType = "select", IsRequired = true },
                new FormField { Name = "quantity", DisplayName = "Quantity", FieldType = "number", IsRequired = true },
                new FormField { Name = "deliveryAddress", DisplayName = "Delivery Address", FieldType = "textarea", IsRequired = true },
                new FormField { Name = "specialInstructions", DisplayName = "Special Instructions", FieldType = "textarea", IsRequired = false }
            }
        };
    }

    private static FormConfiguration GetSupportTicketConfig()
    {
        return new FormConfiguration
        {
            FormType = "support-ticket",
            DisplayName = "Support Ticket (Tenant A)",
            Description = "Submit a support request for Tenant A",
            IsActive = true,
            Fields = new List<FormField>
            {
                new FormField { Name = "subject", DisplayName = "Subject", FieldType = "text", IsRequired = true },
                new FormField { Name = "description", DisplayName = "Description", FieldType = "textarea", IsRequired = true },
                new FormField { Name = "priority", DisplayName = "Priority", FieldType = "select", IsRequired = true },
                new FormField { Name = "category", DisplayName = "Category", FieldType = "select", IsRequired = false }
            }
        };
    }
}

/// <summary>
/// Tenant A Workflow Handler Implementation
/// Handles custom workflow processing logic specific to Tenant A business processes
/// </summary>
public class TenantAWorkflowHandler : ICustomWorkflowHandler
{
    public string TenantId => "TenantA";
    public string Name => "Tenant A Workflow Handler";
    public string Version => "1.0.0";

    private Dictionary<string, object> _configuration = new();

    public async Task InitializeAsync(IDictionary<string, object> configuration)
    {
        _configuration = new Dictionary<string, object>(configuration);
        
        // Simulate async initialization
        await Task.Delay(50);
        
        Console.WriteLine($"Initialized {Name} for tenant {TenantId}");
    }

    public async Task<WorkflowResult> ExecuteStepAsync(string stepName, IDictionary<string, object> inputData, WorkflowContext context)
    {
        var result = new WorkflowResult { IsSuccessful = true };

        try
        {
            // Custom step execution based on step name
            switch (stepName.ToLower())
            {
                case "validatecustomer":
                    result = await ValidateCustomerStep(inputData, context);
                    break;
                case "calculatepricing":
                    result = await CalculatePricingStep(inputData, context);
                    break;
                case "processorder":
                    result = await ProcessOrderStep(inputData, context);
                    break;
                case "sendnotification":
                    result = await SendNotificationStep(inputData, context);
                    break;
                default:
                    // Default processing
                    result.Messages.Add($"Executed step '{stepName}' with Tenant A default handler");
                    result.OutputData = new Dictionary<string, object>(inputData)
                    {
                        ["stepProcessedBy"] = "TenantA-WorkflowHandler",
                        ["stepProcessedAt"] = DateTime.UtcNow
                    };
                    break;
            }
        }
        catch (Exception ex)
        {
            result.IsSuccessful = false;
            result.Errors.Add(new WorkflowError
            {
                Code = "EXECUTION_ERROR",
                Message = $"Error executing step '{stepName}': {ex.Message}",
                Step = stepName
            });
        }

        return result;
    }

    public async Task<IEnumerable<WorkflowStep>> GetAvailableStepsAsync(string workflowType)
    {
        await Task.Delay(10);
        
        return workflowType.ToLower() switch
        {
            "orderprocessing" => new List<WorkflowStep>
            {
                new() { Name = "validatecustomer", DisplayName = "Validate Customer", Order = 1 },
                new() { Name = "calculatepricing", DisplayName = "Calculate Pricing", Order = 2 },
                new() { Name = "processorder", DisplayName = "Process Order", Order = 3 },
                new() { Name = "sendnotification", DisplayName = "Send Notification", Order = 4 }
            },
            "customeronboarding" => new List<WorkflowStep>
            {
                new() { Name = "validateidentity", DisplayName = "Validate Identity", Order = 1 },
                new() { Name = "setupaccount", DisplayName = "Setup Account", Order = 2 },
                new() { Name = "sendwelcome", DisplayName = "Send Welcome", Order = 3 }
            },
            _ => new List<WorkflowStep>()
        };
    }

    public async Task<bool> CanExecuteStepAsync(string stepName, IDictionary<string, object> inputData, WorkflowContext context)
    {
        await Task.Delay(5);
        
        // Tenant A specific step execution rules
        return stepName.ToLower() switch
        {
            "validatecustomer" => inputData.ContainsKey("customerId") || inputData.ContainsKey("email"),
            "calculatepricing" => inputData.ContainsKey("orderValue") || inputData.ContainsKey("productId"),
            "processorder" => inputData.ContainsKey("orderValue") && inputData.ContainsKey("customerId"),
            _ => true // Default: allow execution
        };
    }

    private async Task<WorkflowResult> ValidateCustomerStep(IDictionary<string, object> inputData, WorkflowContext context)
    {
        await Task.Delay(20);
        
        var result = new WorkflowResult { IsSuccessful = true };
        var outputData = new Dictionary<string, object>(inputData);
        
        // Tenant A customer validation logic
        if (inputData.ContainsKey("customerType") && inputData["customerType"]?.ToString() == "premium")
        {
            outputData["validationStatus"] = "approved";
            outputData["customerTier"] = "TenantA-Premium";
            outputData["creditLimit"] = 50000;
            result.NextStep = "calculatepricing";
            result.Messages.Add("Premium customer validated successfully");
        }
        else
        {
            outputData["validationStatus"] = "standard";
            outputData["customerTier"] = "TenantA-Standard";
            outputData["creditLimit"] = 10000;
            result.NextStep = "calculatepricing";
            result.Messages.Add("Standard customer validated");
        }
        
        result.OutputData = outputData;
        return result;
    }

    private async Task<WorkflowResult> CalculatePricingStep(IDictionary<string, object> inputData, WorkflowContext context)
    {
        await Task.Delay(15);
        
        var result = new WorkflowResult { IsSuccessful = true };
        var outputData = new Dictionary<string, object>(inputData);
        
        // Tenant A pricing calculation
        if (inputData.ContainsKey("orderValue") && decimal.TryParse(inputData["orderValue"]?.ToString(), out var orderValue))
        {
            var customerTier = inputData.GetValueOrDefault("customerTier", "TenantA-Standard")?.ToString();
            var discountRate = customerTier == "TenantA-Premium" ? 0.20m : 0.10m;
            var discount = orderValue * discountRate;
            var finalAmount = orderValue - discount;
            
            outputData["discount"] = discount;
            outputData["finalAmount"] = finalAmount;
            outputData["pricingCalculatedBy"] = "TenantA-PricingEngine";
            
            result.NextStep = "processorder";
            result.Messages.Add($"Pricing calculated with {discountRate:P0} Tenant A discount");
        }
        else
        {
            result.IsSuccessful = false;
            result.Errors.Add(new WorkflowError
            {
                Code = "MISSING_ORDER_VALUE",
                Message = "Order value is required for pricing calculation",
                Step = "calculatepricing"
            });
        }
        
        result.OutputData = outputData;
        return result;
    }

    private async Task<WorkflowResult> ProcessOrderStep(IDictionary<string, object> inputData, WorkflowContext context)
    {
        await Task.Delay(30);
        
        var result = new WorkflowResult { IsSuccessful = true };
        var outputData = new Dictionary<string, object>(inputData);
        
        // Tenant A order processing
        var orderId = $"TNA-ORDER-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";
        outputData["orderId"] = orderId;
        outputData["orderStatus"] = "confirmed";
        outputData["processedBy"] = "TenantA-OrderProcessor";
        outputData["estimatedDelivery"] = DateTime.UtcNow.AddDays(2); // Tenant A: 2-day delivery
        
        result.NextStep = "sendnotification";
        result.Messages.Add($"Order {orderId} processed successfully with Tenant A expedited handling");
        result.OutputData = outputData;
        
        return result;
    }

    private async Task<WorkflowResult> SendNotificationStep(IDictionary<string, object> inputData, WorkflowContext context)
    {
        await Task.Delay(10);
        
        var result = new WorkflowResult { IsSuccessful = true };
        var outputData = new Dictionary<string, object>(inputData);
        
        // Tenant A notification sending
        var orderId = inputData.GetValueOrDefault("orderId", "UNKNOWN")?.ToString();
        var customerTier = inputData.GetValueOrDefault("customerTier", "Standard")?.ToString();
        
        outputData["notificationSent"] = true;
        outputData["notificationMethod"] = customerTier == "TenantA-Premium" ? "SMS+Email" : "Email";
        outputData["notificationProvider"] = "TenantA-NotificationService";
        
        result.Messages.Add($"Notification sent for order {orderId} via {outputData["notificationMethod"]}");
        result.OutputData = outputData;
        
        return result;
    }
}
