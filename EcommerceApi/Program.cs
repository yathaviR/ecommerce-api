using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using EcommerceApi.Data;

var builder = WebApplication.CreateBuilder(args);

//=======================================================
// Add Services to DI container
// ======================================================

// Add Controllers
builder.Services.AddControllers();

//Add Swagger/OpenAPI documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "E-commerce REST API",
        Version = "v1",
        Description = "A comprehensive e-commerce REST API with products, shopping cart, orders, and payments",
        Contact = new OpenApiContact
        {
            Name = "Your Name",
            Email = "your.email@example.com"
        }
    });

    // Enable XML documentation comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=EcommerceDb;Integrated_Security=true;";
    
    builder.Services.AddDbContext<ECommerceDbContext>(options =>options.UseSqlServer(connectionString));

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

// Add Logginf
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

//===================================================================
//Build application
// ==================================================================

var app=builder.Build();

// =================================================================
// Configure HTTP request pipeline
// =================================================================

if(app.Environment.IsDevelopment())
{
    // Enable swagger UI in development
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "E-commerce aPI v1");
        options.RoutePrefix=string.Empty;
    });
}

// Global exception handling middleware (optional but recommended)
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                message = "An unexpected error occurred",
                error = error.Error.Message
            });
        }
    });
});

// Redirest HTTP to HTTPS
app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAll");

// Enable authentication/authorization (if added later)
app.UseAuthentication();
app.UseAuthorization();

// MapController routes
app.MapControllers();

// ================================================================
// Run application
// ================================================================

app.Run();
