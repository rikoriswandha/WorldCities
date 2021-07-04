using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WorldCities.Controllers;
using WorldCities.Data;
using WorldCities.Data.Models;
using Xunit;

namespace WorldCities.Tests
{
    public class CitiesController_Tests
    {
        /// <summary>
        /// Test the GetCity() method
        /// </summary>
        [Fact]
        public async void GetCity()
        {
            #region Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "WorldCities").Options;

            var storeOptions = Options.Create(new OperationalStoreOptions());

            using (var context = new ApplicationDbContext(options, storeOptions))
            {
                context.Add(new City()
                {
                    Id = 1,
                    CountryId = 1,
                    Lat = 1,
                    Lon = 1,
                    Name = "TestCity1"
                });
                context.SaveChanges();
            }
            City city_existing = null;
            City city_notExisting = null;
            #endregion

            #region Act
            using (var context = new ApplicationDbContext(options, storeOptions))
            {
                var controller = new CitiesController(context);
                city_existing = (await controller.GetCity(1)).Value;
                city_notExisting = (await controller.GetCity(2)).Value;

            }
            #endregion

            #region  Assert
            Assert.NotNull(city_existing);
            Assert.Null(city_notExisting);
            #endregion
        }
    }
}