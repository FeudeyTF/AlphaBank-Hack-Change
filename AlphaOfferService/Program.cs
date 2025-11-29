using AlphaOfferService.AlphaStructure.Clients;
using AlphaOfferService.AlphaStructure.Entities;
using AlphaOfferService.Core;
using AlphaOfferService.Models;
using AlphaOfferService.Services;

namespace AlphaOfferService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var modelPath = Path.Combine(AppContext.BaseDirectory, "mark_model.onnx");
            builder.Services.AddSingleton<IIncomeModel, MarkModel>(provider => new MarkModel(modelPath));
            builder.Services.AddScoped<IClientRepository, AlphaBankClientRepository>();
            builder.Services.AddScoped<IIncomeService, ModelIncomeService>();

            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseAuthorization();

            var apiVersion1 = app.MapGroup("api/v1");
            apiVersion1.MapControllers();

            app.Run();
        }
    }
}
