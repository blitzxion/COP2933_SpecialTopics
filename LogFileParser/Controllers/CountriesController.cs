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
            CountryMetrics model = null;

            using (var context = AppDbContext)
            {

                var data = context.LogRecords.GroupBy(x => new
                {
                    x.Country,
                    x.MessageClass,
                    x.TimestampUTC.Year,
                    x.TimestampUTC.Month,
                    x.TimestampUTC.Day // Group by Day
                })
                .Select(x => new { x.Key, Total = x.Count() });

                var results = data
                    .ToList() // DB CALL HERE!
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
                        CountryName = CountryCodes.GetCountryNameFromCode(x.Key),
                        Data = x.OrderBy(d => d.Date).Select(y => new CountryMessageDetails()
                        {
                            Date = y.Date,
                            MessageClass = y.MessageClass,
                            Total = y.Total
                        })
                    });

                model = new CountryMetrics(results);

            }

            return View(model);
        }

        public ActionResult Details(string country)
        {
            return View();
        }

    }
}