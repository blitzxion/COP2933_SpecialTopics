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
    public class MessagesController : BaseController<LogDbContext>
    {
        // GET: Messages
        public ActionResult Index()
        {
            IEnumerable<RecordMetrics> model = null;
            using (var context = AppDbContext)
            {
                model = GetMessageMetricData(context);
            }

            return View(model);
        }

        public ActionResult Details(string messageClass)
        {
            if (string.IsNullOrEmpty(messageClass))
                throw new ArgumentNullException(nameof(messageClass));

            RecordMetrics model = null;

            using (var context = AppDbContext)
            {
                if (!context.LogRecords.Select(x => x.MessageClass).Distinct().Any(x => x.ToLower() == messageClass.ToLower()))
                    throw new ArgumentException("MessageClass not Found");

                model = GetMessageMetricData(context, messageClass).FirstOrDefault();

                if (model == null)
                    throw new ArgumentException("Unable to retrieve details.");

            }

            return View(model);

        }

        // Helpers

        private IEnumerable<RecordMetrics> GetMessageMetricData(LogDbContext context, string messageFilter = null)
        {
            IQueryable<LogRecord> data = context.LogRecords;

            if (!string.IsNullOrEmpty(messageFilter))
                data = data.Where(x => x.MessageClass.ToLower() == messageFilter.ToLower());

            var results = data.GroupBy(x => new
            {
                x.Country,
                x.MessageClass,
                x.TimestampUTC.Year,
                x.TimestampUTC.Month,
                x.TimestampUTC.Day,
            })
            .Select(x => new { x.Key, Count = x.Count() })
            .ToList() // DB Calls
            .Select(x => new
            {
                Date = new DateTime(x.Key.Year, x.Key.Month, x.Key.Day, 0, 0, 0),
                MessageClass = x.Key.MessageClass,
                Total = x.Count,
                Country = x.Key.Country
            })
            .GroupBy(x => x.MessageClass)
            .Select(x => new RecordMetrics(x.OrderBy(v => v.Date).Select(v => new RecordDetails()
            {
                Timestamp = v.Date,
                Name = CountryCodes.GetCountryNameFromCode(v.Country),
                Total = v.Total
            }))
            {
                Name = x.Key
            })
            .ToList();

            return results;
        }


    }
}