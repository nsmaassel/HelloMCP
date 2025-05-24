using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AzureFunctionsMcp.Services;
using Microsoft.Azure.Functions.Worker;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((Action<WorkerOptions>)(options => {
        // Configure worker options
    }))
    .ConfigureServices(services => {
        services.AddSingleton<SessionService>();
        services.Configure<JsonSerializerOptions>(options => {
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        });
    })
    .Build();

host.Run();