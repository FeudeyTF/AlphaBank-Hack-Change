using AlphaOfferService.AlphaStructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlphaOfferService.AlphaStructure.Services
{
    public class AlphaBanRepositoryInitializer
    {
        private readonly AlphaBankRepository _repository;
        private readonly ILogger<AlphaBanRepositoryInitializer> _logger;

        public AlphaBanRepositoryInitializer(
            AlphaBankRepository repository,
            ILogger<AlphaBanRepositoryInitializer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task InitializeDatabase()
        {
            _logger.LogInformation("Starting database initialization");

            try
            {
                if (await _repository.Database.CanConnectAsync())
                {
                    _logger.LogInformation("Database connection successful");
                }
                else
                {
                    _logger.LogWarning("Database does not exist. Creating database...");
                    await _repository.Database.EnsureCreatedAsync();
                    _logger.LogInformation("Database created successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database initialization failed");
                throw;
            }
        }
    }
}
