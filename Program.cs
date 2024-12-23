using Durable.Functions.Validators;
using Durable.Services;
using Durable.Services.Interfaces;
using Durable.Utilities;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

//FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

//builder.ConfigureFunctionsWebApplication();

//// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
//// builder.Services
////     .AddApplicationInsightsTelemetryWorkerService()
////     .ConfigureFunctionsApplicationInsights();


//builder.Build().Run();
IHost hostBuilder = new HostBuilder()
    .ConfigureFunctionsWebApplication(workerApp =>
{ })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<IEnvImpactReportService, EnvImpactReportService>();
        services.AddSingleton<IServiceProviderValidatorFactory, ServiceProviderValidatorFactory>();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    })
    .Build();

await hostBuilder.RunAsync();
