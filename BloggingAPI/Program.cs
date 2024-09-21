using BloggingAPI.Domain.Repositories;
using BloggingAPI.Persistence;
using BloggingAPI.Persistence.Extensions;
using BloggingAPI.Persistence.Repositories;
using BloggingAPI.Services.Constants;
using BloggingAPI.Services.Implementation;
using BloggingAPI.Services.Interface;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;
using static BloggingAPI.Persistence.Extensions.ServiceExtensions;
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
try
{
    Log.Information("starting server.");
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);
    builder.Host.UseSerilog((context, loggerConfiguration) =>
    {
        loggerConfiguration.WriteTo.Console();
        loggerConfiguration.ReadFrom.Configuration(context.Configuration);
    });
    // Add services to the container.
    builder.Services.ConfigureCors();
    builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("EmailConfiguration"));
    
    builder.Services.ConfigureSqlContext(builder.Configuration);
    builder.Services.AddScoped<IRepositoryManager, RepositoryManager>();
    builder.Services.ConfigureIdentity();
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
    builder.Services.AddScoped<IBloggingService, BloggingService>();
    builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();
    builder.Services.AddHttpClient();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
    builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
    builder.Services.ConfigureHangFire(builder.Configuration);
    builder.Services.AddHangfireServer();
    builder.Services.ConfigureAuthentication(builder.Configuration);
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
    builder.Services.ConfigureUrlHelper();
    builder.Services.ConfigureRedisCache(builder.Configuration);
    builder.Services.AddControllers(config =>
    {
        config.RespectBrowserAcceptHeader = true;
        config.ReturnHttpNotAcceptable = true;
        config.InputFormatters.Insert(0, MyJPIF.GetJsonPatchInputFormatter());
    });
        //.AddXmlDataContractSerializerFormatters();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

    builder.Services.ConfigureSwaggerGen();
    builder.Services.AddEndpointsApiExplorer();
    var app = builder.Build();
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        try
        {
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            if (dbContext.Database.IsSqlServer())
            {
                dbContext.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseSerilogRequestLogging();
    app.UseHangfireDashboard();
    app.UseHttpsRedirection();
    app.UseCors("CorsPolicy");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseHangfireDashboard();
    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "server terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
