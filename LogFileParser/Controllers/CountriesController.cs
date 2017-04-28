using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LogFileParser.Models;
using LogFileParser.ViewModels;
using LogFileParser.Helpers;

namespace LogFileParser.Controllers
{
    public class CountriesController : BaseController<LogDbContext>
    {
        // GET: Countries
        public ActionResult Index()
        {
            // Just group all countries up by name, date (day)
            IEnumerable<CountryMetricDetails> model = null;
            using (var context = AppDbContext)
            {
                var results = GetCountriesData(context);
                model = results;
            }
            return View(model);
        }

        public ActionResult Details(string countryCode)
        {
            // Just group all countries up by name, date (day)
            CountryMetricDetails model = null;
            using (var context = AppDbContext)
            {

                var results = GetCountriesData(context, countryCode);
                model = results.FirstOrDefault();
            }
            return View(model);
        }

        // JSON

        public ActionResult GetAllCountryData()
        {
            CountryMetricDetails model = null;
            using (var context = AppDbContext)
            {
                var results = GetCountriesData(context);
                model = results.FirstOrDefault();
            }

            return SerializeToJsonFormatted(model);
        }

        public ActionResult GetCountryData(string countryCode)
        {
            CountryMetricDetails model = null;
            using (var context = AppDbContext)
            {
                var results = GetCountriesData(context, countryCode);
                model = results.FirstOrDefault();
            }

            return SerializeToJsonFormatted(model);
        }

        // Helpers

        private IEnumerable<CountryMetricDetails> GetCountriesData(LogDbContext context, string countryCodeFilter = null)
        {
            IQueryable<LogRecord> data = context.LogRecords;

            if (!string.IsNullOrEmpty(countryCodeFilter))
                data = data.Where(x => x.Country.ToLower() == countryCodeFilter.ToLower());

            var groupedData = data
                .GroupBy(x => new
                {
                    x.Country,
                    x.MessageClass,
                    x.TimestampUTC.Year,
                    x.TimestampUTC.Month,
                    x.TimestampUTC.Day // Group by Day
                })
                .Select(x => new { x.Key, Total = x.Count() });

            var results = groupedData
                .ToList() // DB Call
                .Select(x => new
                {
                    Country = x.Key.Country,
                    Date = new DateTime(x.Key.Year, x.Key.Month, x.Key.Day, 0, 0, 0), // Grouped by Day
                    MessageClass = x.Key.MessageClass,
                    Total = x.Total,
                })
                .GroupBy(x => x.Country) // Group by Country to get a child list of messages/totals
                .Select(x => new CountryMetricDetails()
                {
                    CountryCode = x.Key,
                    Name = CountryCodes.GetCountryNameFromCode(x.Key),
                    Data = x.OrderBy(d => d.Date).Select(y => new CountryMessageDetails()
                    {
                        Timestamp = y.Date,
                        Name = y.MessageClass,
                        Total = y.Total
                    })
                });

            return results;

        }

    }
}