using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WorldCities.Data;
using WorldCities.Data.Models;

namespace WorldCities.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        public SeedController(
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult> Import()
        {
            var path = Path.Combine(
                _env.ContentRootPath,
                String.Format("Data/Source/worldcities.xlsx"));

            using (var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read))
            {
                using (var ep = new ExcelPackage(stream))
                {
                    var ws = ep.Workbook.Worksheets[0];

                    var nCountries = 0;
                    var nCities = 0;

                    #region Import all Countries

                    var lstCountries = _context.Countries.ToList();

                    for(int nRow = 2;
                        nRow <= ws.Dimension.End.Row;
                        nRow++)
                    {
                        var row = ws.Cells[nRow, 1, nRow, ws.Dimension.End.Column];
                        var name = row[nRow, 5].GetValue<string>();

                        if (lstCountries.Where(c=>c.Name == name).Count() == 0)
                        {
                            var country = new Country();
                            country.Name = name;
                            country.ISO2 = row[nRow, 6].GetValue<string>();
                            country.ISO3 = row[nRow, 7].GetValue<string>();

                            _context.Countries.Add(country);

                            lstCountries.Add(country);

                            nCountries++;
                        }
                    }

                    // Save all the countries into the Database
                    if (nCountries > 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                    #endregion

                    #region Import all Cities
                    var lstCities = _context.Cities.ToList();

                    for (int nRow = 2; nRow<=ws.Dimension.End.Row; nRow++)
                    {
                        var row = ws.Cells[nRow, 1, nRow, ws.Dimension.End.Column];

                        var name = row[nRow, 1].GetValue<string>();
                        var name_ASCII = row[nRow, 2].GetValue<string>();
                        var countryName = row[nRow, 5].GetValue<string>();
                        var lat = row[nRow, 3].GetValue<decimal>();
                        var lon = row[nRow, 4].GetValue<decimal>();
                        // retrieve country and countryId
                        var country = lstCountries.Where(c => c.Name == countryName).FirstOrDefault();
                        var countryId = country.Id;

                        if(lstCities.Where(
                            c => c.Name == name
                            && c.Lat == lat
                            && c.Lon == lon
                            && c.CountryId == countryId
                        ).Count() == 0)
                        {
                            var city = new City();
                            city.Name = name;
                            city.Name_ASCII = name_ASCII;
                            city.Lat = lat;
                            city.Lon = lon;
                            city.CountryId = countryId;

                            _context.Cities.Add(city);

                            nCities++;
                        }
                    }

                    if (nCities > 0)
                    {
                        await _context.SaveChangesAsync();
                    }

                    #endregion

                    return new JsonResult(new 
                    { 
                        Cities = nCities,
                        Countries = nCountries
                    });
                }
            }
        }
    }
}
