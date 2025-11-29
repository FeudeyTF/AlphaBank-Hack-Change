using AlphaOfferService.AlphaStructure.Clients;
using AlphaOfferService.Core;
using Microsoft.AspNetCore.Mvc;

namespace AlphaOfferService.Controllers
{
    [ApiController]
    [Route("client/[controller]")]
    public class IncomeController : ControllerBase
    {
        private readonly IIncomeService _incomeService;

        private readonly IClientRepository _clientRepository;

        private readonly ILogger<IncomeController> _logger;

        public IncomeController(IIncomeService incomeService, IClientRepository clientRepository, ILogger<IncomeController> logger)
        {
            _incomeService = incomeService;
            _clientRepository = clientRepository;
            _logger = logger;
        }

        [HttpGet("{clientId}", Name = "GetClientIncome")]
        public async Task<IResult> GetClientIncomeAsync(string clientId)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Requesting income for client: {ClientId}", clientId);

            var client = await _clientRepository.GetClientByIdAsync(clientId);
            if (client == null)
                return Results.NotFound(new { Reason = $"Client {clientId} was not found!" });

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Найден клиент: {ClientId}. Вычисляется доход...", clientId);

            var income = await _incomeService.GetClientIncomeAsync(client);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Доход клиента {ClientId} успешно вычислен: {Income:F2}",
                    clientId,
                    income
                );
            }

            return Results.Ok(new { ClientId = clientId, Income = income });
        }
    }
}
