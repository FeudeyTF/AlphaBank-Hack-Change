using AlphaOfferService.AlphaStructure.Clients;
using AlphaOfferService.AlphaStructure.Entities;
using AlphaOfferService.AlphaStructure.Services;
using AlphaOfferService.Core;
using AlphaOfferService.Middleware;
using AlphaOfferService.Models;
using AlphaOfferService.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AlphaOfferService
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            Log.Information("Запуск микро-сервиса AlphaOfferService");

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                );

                var modelPath = Path.Combine(AppContext.BaseDirectory, "model.onnx");
                builder.Services.AddSingleton<IIncomeModel>(new MarkModel(modelPath));
                Log.Information("Создана модель для определения дохода клиента: {ModelPath}", modelPath);

                var clientDbPath = Path.Combine(AppContext.BaseDirectory, "users.sqlite");
                builder.Services.AddDbContext<AlphaBankRepository>(options => 
                    options.UseSqlite($"Data Source={clientDbPath}"));
                builder.Services.AddScoped<IClientRepository, AlphaBankRepository>(provider => 
                    provider.GetRequiredService<AlphaBankRepository>());
                Log.Information("Добавлена база данных: {DbPath}", clientDbPath);

                builder.Services.AddScoped<AlphaBanRepositoryInitializer>();
                builder.Services.AddScoped<IIncomeService, ModelIncomeService>();

                builder.Services.AddControllers();

                var app = builder.Build();

                app.UseSerilogRequestLogging(options =>
                {
                    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
                    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                    {
                        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                        diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress);
                        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                    };
                });

                app.UseMiddleware<ExceptionHandlingMiddleware>();
                app.UseMiddleware<RequestLoggingMiddleware>();

                app.UseHttpsRedirection();
                app.UseAuthorization();

                var apiVersion1 = app.MapGroup("api/v1");
                apiVersion1.MapControllers();

                using (var scope = app.Services.CreateScope())
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Начата инициализация базы данных клиентов");
                    await scope.ServiceProvider.GetRequiredService<AlphaBanRepositoryInitializer>().InitializeDatabase();
                    logger.LogInformation("Инициализация базы данных клиентов завершена");
                }

                Log.Information("Микро-сервис AlphaOfferService успешно запущен!");
                await app.RunAsync();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Произошла ошбика при старте микро-сервиса AlphaOffer!");
                throw;
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }
    }
}
