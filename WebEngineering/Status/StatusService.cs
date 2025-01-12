using Microsoft.EntityFrameworkCore;

namespace WebEngineering.Status
{
    public class StatusService : IStatusService
    {
        private readonly AppDbContext _dbContext;

        public StatusService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public StatusInformationHealth GebeHealthInformationenZurueck()
        {
            var isDbConnected = _dbContext.Database.CanConnect();

            var healthStatus = new StatusInformationHealth
            {
                live = true,
                ready = isDbConnected,
                databases = new
                {
                    assets = new
                    {
                        connected = isDbConnected
                    }
                }
            };

            return healthStatus;
        }

        public StatusInformation GebeStatusInformationenZurueck()
        {
            var authoren = new List<string> { "Finn Marin", "Matt Wagner" };
            var supportedApis = new List<string> { "jwt-v2", "assets-v3", "reservations-v2" };

            return new StatusInformation
            {
                authors = authoren,
                supportedApis = supportedApis
            };
        }
    }
}
