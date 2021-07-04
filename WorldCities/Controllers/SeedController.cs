using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WorldCities.Data;
using OfficeOpenXml;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using WorldCities.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Security;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace WorldCities.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public SeedController(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult> Import()
        {
            if (!_env.IsDevelopment())
                throw new SecurityException("Not allowed");

            var path = Path.Combine(
                _env.ContentRootPath,
                "Data/Source/worldcities.xlsx"
            );

            using var stream = System.IO.File.OpenRead(path);
            using var excelPackage = new ExcelPackage(stream);

            // Get the first worksheet
            var worksheet = excelPackage.Workbook.Worksheets[0];

            // Define how many rows we want to process
            var nEndRow = worksheet.Dimension.End.Row;

            // Initialize the record counters
            var numberOfCountriesAdded = 0;
            var numberOfCitiesAdded = 0;

            // Create a lookup dictionary
            // containing all the countries already existing
            // into the Database (it will be empty on 1st run)
            var countriesByName = _context.Countries
                .AsNoTracking()
                .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            // Iterates through all rows, skipping the 1st one
            for (int nRow = 2; nRow <= nEndRow; nRow++)
            {
                var row = worksheet.Cells[
                    nRow, 1, nRow, worksheet.Dimension.End.Column];

                var countryName = row[nRow, 5].GetValue<string>();
                var iso2 = row[nRow, 6].GetValue<string>();
                var iso3 = row[nRow, 7].GetValue<string>();

                // Skip this country if it already exists in the database
                if (countriesByName.ContainsKey(countryName))
                    continue;

                // Create the Country entity and fill it with xlsx data
                var country = new Country
                {
                    Name = countryName,
                    ISO2 = iso2,
                    ISO3 = iso3
                };

                // Add new country to the DB Context
                await _context.Countries.AddAsync(country);

                // Store the country in our lookup to retrieve its Id later on
                countriesByName.Add(countryName, country);

                // Increment the counter
                numberOfCountriesAdded++;
            }

            // Save all the countries into the db
            if (numberOfCountriesAdded > 0)
                await _context.SaveChangesAsync();

            // Create a lookup dictionary for cities
            var cities = _context.Cities
                .AsNoTracking()
                .ToDictionary(x => (
                    Name: x.Name,
                    Lat: x.Lat,
                    Lon: x.Lon,
                    CountryId: x.CountryId
                ));

            // Iterates thru all rows, skipping the first one
            for (int nRow = 2; nRow <= nEndRow; nRow++)
            {
                var row = worksheet.Cells[
                    nRow, 1, nRow, worksheet.Dimension.End.Column
                ];

                var name = row[nRow, 1].GetValue<string>();
                var nameAscii = row[nRow, 2].GetValue<string>();
                var lat = row[nRow, 3].GetValue<decimal>();
                var lon = row[nRow, 4].GetValue<decimal>();
                var countryName = row[nRow, 5].GetValue<string>();

                // Retrieve country id by countryName
                var countryId = countriesByName[countryName].Id;

                // Skip if already exists
                if (cities.ContainsKey((
                    Name: name,
                    Lat: lat,
                    Lon: lon,
                    CountryId: countryId)))
                    continue;

                // Create new City entity and fill it with xlsx data
                var city = new City
                {
                    Name = name,
                    Name_ASCII = nameAscii,
                    Lat = lat,
                    Lon = lon,
                    CountryId = countryId
                };

                // Add to the db context
                _context.Cities.Add(city);

                // Increment the counter
                numberOfCitiesAdded++;
            }

            // Save cities to db
            if (numberOfCitiesAdded > 0)
                await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                Cities = numberOfCitiesAdded,
                Countries = numberOfCountriesAdded
            });
        }

        [HttpGet]
        public async Task<ActionResult> CreateDefaultUsers()
        {
            // setup the default role names
            string role_RegisteredUser = "RegisteredUser";
            string role_Administrator = "Administrator";

            // create the default roles (if they don't exist yet)
            if (await _roleManager.FindByNameAsync(role_RegisteredUser) == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(role_RegisteredUser));
            }

            if (await _roleManager.FindByNameAsync(role_Administrator) == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(role_Administrator));
            }

            // create a list to track the newly created users
            var addedUserList = new List<ApplicationUser>();

            // check if the admin user already exists
            var email_Admin = "admin@email.com";
            if (await _userManager.FindByNameAsync(email_Admin) == null)
            {
                // create a new admin ApplicationUser account
                var user_Admin = new ApplicationUser()
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = email_Admin,
                    Email = email_Admin,
                };

                // insert the admin user into the DB
                await _userManager.CreateAsync(user_Admin, "MySecr3t$");

                // assign the "RegisteredUser" and "Administrator" roles
                await _userManager.AddToRoleAsync(user_Admin, role_RegisteredUser);
                await _userManager.AddToRoleAsync(user_Admin, role_Administrator);


                // Confirm the email and remove Lockout
                user_Admin.EmailConfirmed = true;
                user_Admin.LockoutEnabled = false;

                // Add the admin user to the added users list
                addedUserList.Add(user_Admin);
            }

            // check if the standard user already exists
            var email_User = "user@email.com";
            if (await _userManager.FindByNameAsync(email_User) == null)
            {
                // create a new standard ApplicationUser account
                var user_User = new ApplicationUser()
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = email_User,
                    Email = email_User,
                };

                // insert the standard user into the DB
                await _userManager.CreateAsync(user_User, "MySecr3t$");

                // Assign the "RegisteredUser" role
                await _userManager.AddToRoleAsync(user_User, role_RegisteredUser);

                // confirm the email and remove Lockout
                user_User.EmailConfirmed = true;
                user_User.LockoutEnabled = false;

                // add the standard user to the added users list
                addedUserList.Add(user_User);
            }

            // If we added at least one user, persist the changes into the DB
            if (addedUserList.Count > 0) await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                Count = addedUserList.Count,
                Users = addedUserList
            });

        }
    }
}