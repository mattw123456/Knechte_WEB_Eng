using Moq;
using WebEngineering.Buildings;

namespace WebEngineering.Tests
{
    public class UnitTest1
    {
        private readonly Mock<IBuildingService> _buildingServiceMock;

        public UnitTest1()
        {
            _buildingServiceMock = new Mock<IBuildingService>();
        }

        [Fact]
        public async Task GetAllBulding_shouldReturnBuildings_WhenBuildingsExist()
        {

            var buildings = new List<Building>
            {
                new Building  { id = Guid.NewGuid(), name = "BuildingA", streetname = "kingstreet", housenumber = "1", country_code = "eo", postalcode = "12345", city = "Munich", deleted_at = null },
                new Building  { id = Guid.NewGuid(), name = "BuildingB", streetname = "vamosstreet", housenumber = "10", country_code = "eo", postalcode = "12246", city = "Paris", deleted_at = null },
                new Building  { id = Guid.NewGuid(), name = "BuildingC", streetname = "welcomestreet", housenumber = "12", country_code = "eo", postalcode = "13347", city = "Stuttgart", deleted_at = DateTime.UtcNow },
            };

            _buildingServiceMock
                .Setup(service => service.GetAllBuildingsAsync(false))
                .ReturnsAsync(buildings);


            var service = _buildingServiceMock.Object;

            var result = await service.GetAllBuildingsAsync();

            Assert.Equal(3, result.Count);
        }
    }
}