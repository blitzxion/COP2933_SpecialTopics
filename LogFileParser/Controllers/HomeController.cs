using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

using LogFileParser.Models;
using LogFileParser.Models.Helpers;

using LogFileParser.ViewModels;
using X.PagedList;
using Newtonsoft.Json;

using CsvHelper;
using CsvHelper.Configuration;
using System.IO;

namespace LogFileParser.Controllers
{
    public class HomeController : BaseController<LogDbContext>
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Globe()
        {
            // I'm getting lazy now
            // going to get the cooridinates for all the countries we have.

            IEnumerable<CountryLocationDetail> countryLocationDetails = null;
            using (var streamReader = new StreamReader(Server.MapPath("~/content/country_centroids_all.csv")))
            {
                using (var csv = new CsvReader(streamReader))
                {
                    csv.Configuration.HasHeaderRecord = true;
                    csv.Configuration.Delimiter = "\t";
                    countryLocationDetails = csv.GetRecords<CountryLocationDetail>().ToList();
                }
            }

            Dictionary<string, int> knownLocations = null;
            using (var context = AppDbContext)
            {
                knownLocations = context.LogRecords
                 .GroupBy(x => x.Country)
                 .Select(x => new { CountryCode = x.Key, Count = x.Count() })
                 .ToList()
                 .ToDictionary(x => x.CountryCode, x => x.Count);
            }

            var max = knownLocations.Max(x => x.Value);
            double max_ratio = 100; // Math.Round(knownLocations.Average(x => x.Value));
            double ratio = max / max_ratio;

            IEnumerable<CountryLocation> countryLocations = countryLocationDetails
            .Where(x => !string.IsNullOrEmpty(x.ISO3136))
            .Select(x => new CountryLocation()
            {
                CountryCode = x.ISO3136,
                LONG = x.LONG,
                LAT = x.LAT,
                Magnitude = (knownLocations.ContainsKey(x.ISO3136)) ? knownLocations[x.ISO3136] : 0
                //Magnitude = (knownLocations.ContainsKey(x.ISO3136)) ? Math.Round(knownLocations[x.ISO3136] / ratio) : 0
            });

            return View(countryLocations);
        }
        
    }

}