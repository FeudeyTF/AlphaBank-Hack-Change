using AlphaOfferService.AlphaStructure.Clients;
using AlphaOfferService.Core;
using System.Diagnostics;

namespace AlphaOfferService.Services
{
    internal class ModelIncomeService : IIncomeService
    {
        private readonly IIncomeModel _incomeModel;

        private readonly ILogger<ModelIncomeService> _logger;

        public ModelIncomeService(IIncomeModel incomeModel, ILogger<ModelIncomeService> logger)
        {
            _incomeModel = incomeModel;
            _logger = logger;
        }

        public async Task<double> GetClientIncomeAsync(IClient client)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Начато вычисление дохода клиента: {ClientId}", client.Id);

            var stopwatch = Stopwatch.StartNew();
            var income = await _incomeModel.CalculateClientIncome(client);
            stopwatch.Stop();

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Доход клиента {ClientId} успешно вычислен за {ElapsedMs}ms. Результат: {Income:F2}",
                    client.Id,
                    stopwatch.ElapsedMilliseconds,
                    income
                );
            }

            return income;
        }
    }
}
