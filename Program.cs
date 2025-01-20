using Durable.Services;
using Durable.Services.Interfaces;
using Durable.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

//FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

//builder.ConfigureFunctionsWebApplication();

//// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
//// builder.Services
////     .AddApplicationInsightsTelemetryWorkerService()
////     .ConfigureFunctionsApplicationInsights();


//builder.Build().Run();
IHost hostBuilder = new HostBuilder()
    .ConfigureFunctionsWebApplication(workerApp => { })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<IEnvImpactReportService, EnvImpactReportService>();
        services.AddSingleton<IMongoDbRepository, MongoDbRepository>();
        services.AddSingleton<IServiceProviderValidatorFactory, Durable.Utilities.ServiceProviderValidatorFactory>();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    })
    .Build();

await hostBuilder.RunAsync();
