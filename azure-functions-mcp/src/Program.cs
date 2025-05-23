using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AzureFunctionsMcp.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker => {
        worker.UseDefaultJsonSerializer(options => {
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        });
    })
    .ConfigureServices(services => {
        services.AddSingleton<SessionService>();
    })
    .Build();

host.Run();