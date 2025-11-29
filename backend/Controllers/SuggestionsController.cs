using AlphaOfferService.AlphaStructure.Clients;
using AlphaOfferService.AlphaStructure.Services.Suggestions;
using Microsoft.AspNetCore.Mvc;

namespace AlphaOfferService.Controllers
{
    [ApiController]
    [Route("client/[controller]")]
    public class SuggestionsController : ControllerBase
    {
        private readonly IEnumerable<ISuggestionService> _suggestionServices;

        private readonly IClientRepository _clientRepository;

        private readonly ILogger<SuggestionsController> _logger;

        public SuggestionsController(IEnumerable<ISuggestionService> suggestionServices, IClientRepository clientRepository, ILogger<SuggestionsController> logger)
        {
            _suggestionServices = suggestionServices;
            _logger = logger;
            _clientRepository = clientRepository;
        }

        [HttpGet("{clientId}", Name = "GetClientSuggestions")]
        public async Task<IResult> GetClientSuggestionsAsync(string clientId)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Получен запрос на предложения для клиента: {ClientId}", clientId);

            var client = await _clientRepository.GetClientByIdAsync(clientId);
            if (client == null)
                return Results.NotFound(new { Reason = $"Client {clientId} was not found!" });

            List<ISuggestion> suggestions = [];
            foreach(var suggestionService in _suggestionServices)
            {
                var serviceSuggestions = await suggestionService.GetSuggestionsAsync(client);
                suggestions.AddRange(serviceSuggestions.Where(s => !suggestions.Any(s2 => s2.Title == s.Title)));
            }

            return Results.Ok(new { ClientId = clientId, Suggestions = suggestions });
        }
    }
}
