using AlphaOfferService.AlphaStructure.Clients;
using AlphaOfferService.AlphaStructure.Entities;
using AlphaOfferService.AlphaStructure.Entities.Suggestions.Income;
using AlphaOfferService.AlphaStructure.Entities.Suggestions.Salary;
using AlphaOfferService.AlphaStructure.Services;
using AlphaOfferService.AlphaStructure.Services.Suggestions;
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

                Log.Information("Начато создание модели для определения дохода клиента");
                var modelPath = Path.Combine(AppContext.BaseDirectory, "model.onnx");
                builder.Services.AddSingleton<IIncomeModel>(new MarkModel(modelPath));
                Log.Information("Создана модель для определения дохода клиента: {ModelPath}", modelPath);

                var clientDbPath = Path.Combine(AppContext.BaseDirectory, "alphabase.sqlite");
                builder.Services.AddDbContext<AlphaBankRepository>(options =>
                    options.UseSqlite($"Data Source={clientDbPath}"));
                builder.Services.AddScoped<IClientRepository, AlphaBankRepository>(provider =>
                    provider.GetRequiredService<AlphaBankRepository>());
                builder.Services.AddScoped<ISuggestionsRepository<IncomeSuggestion>, AlphaBankRepository>(provider =>
                    provider.GetRequiredService<AlphaBankRepository>());
                builder.Services.AddScoped<ISuggestionsRepository<SalarySuggestion>, AlphaBankRepository>(provider =>
                    provider.GetRequiredService<AlphaBankRepository>());
                builder.Services.AddScoped<ISuggestionsRepository<CashflowAtmSuggestion>, AlphaBankRepository>(provider =>
                    provider.GetRequiredService<AlphaBankRepository>());

                Log.Information("Добавлена база данных: {DbPath}", clientDbPath);

                builder.Services.AddScoped<AlphaBankRepositoryInitializer>();

                builder.Services.AddScoped<IIncomeService, ModelIncomeService>();
                builder.Services.AddScoped<ISuggestionService, IncomeSuggestionService>();
                builder.Services.AddScoped<ISuggestionService, AverageSalarySuggestionService>();
                builder.Services.AddScoped<ISuggestionService, CashflowAtmSuggestionService>();

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
                    await scope.ServiceProvider.GetRequiredService<AlphaBankRepositoryInitializer>().InitializeDatabase();
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
