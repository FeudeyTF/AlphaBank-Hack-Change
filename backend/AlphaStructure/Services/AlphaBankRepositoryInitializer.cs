using AlphaOfferService.AlphaStructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlphaOfferService.AlphaStructure.Services
{
    public class AlphaBankRepositoryInitializer
    {
        private readonly AlphaBankRepository _repository;

        private readonly ILogger<AlphaBankRepositoryInitializer> _logger;

        public AlphaBankRepositoryInitializer(AlphaBankRepository repository, ILogger<AlphaBankRepositoryInitializer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task InitializeDatabase()
        {
            _logger.LogInformation("Начата инициализация базы данных");

            try
            {
                if (await _repository.Database.CanConnectAsync())
                {
                    _logger.LogInformation("База данных инициализирована");
                }
                else
                {
                    _logger.LogWarning("База данных не найдена. Создание новой базы данных...");
                    await _repository.Database.EnsureCreatedAsync();
                    _logger.LogInformation("База данных успешно создана");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Произошла ошибка при создании базы данных");
                throw;
            }
        }
    }
}
