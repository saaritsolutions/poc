using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Data;
using WorkflowEngine.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Entity Framework
builder.Services.AddDbContext<WorkflowDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add service implementations
builder.Services.AddScoped<IWorkflowService, WorkflowEngine.Data.Services.WorkflowService>();
builder.Services.AddScoped<IWorkflowInstanceService, WorkflowEngine.Data.Services.WorkflowInstanceService>();
builder.Services.AddScoped<IWorkflowEngine, WorkflowEngine.Data.Services.WorkflowEngine>();
builder.Services.AddScoped<IWorkflowDefinitionParser, WorkflowEngine.Data.Services.WorkflowDefinitionParser>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Workflow Engine API", 
        Version = "v1",
        Description = "A comprehensive workflow engine for core banking solutions"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngularApp");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
