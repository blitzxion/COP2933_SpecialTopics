using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Microsoft.AspNet.SignalR;
using LogFileParser.Hubs;
using System.Data.Entity;

using LogFileParser.Models;
using LogFileParser.Models.Helpers;

using LogFileParser.ViewModels;
using X.PagedList;
using Newtonsoft.Json;
using System.Linq.Dynamic;
using System.Dynamic;

namespace LogFileParser.Controllers
{


	public class RecordsController : BaseController<LogDbContext>
	{

		string LogFilePath { get { return Server.MapPath("~/App_Data/mailfilter-20161202.log"); } }
		string DBLocation { get { return ConnectionStringParser.GetDataSource(AppDbContext.Database.Connection.ConnectionString); } }


		// GET: Records
		public ActionResult Index(int? page, int? pageSize)
		{
			IPagedList<LogRecord> model = null;

			// Defaults
			page = page ?? 1;
			pageSize = pageSize ?? 25;

			using (var context = AppDbContext)
				model = context.LogRecords
					.OrderBy(x => x.TimestampUTC)
					.ToPagedList(page.Value, pageSize.Value);

			ViewBag.CurrentPage = page;
			ViewBag.PreviousPage = page - 1;
			ViewBag.NextPage = page + 1;
			ViewBag.LastPage = model.PageCount;

			return View(model);
		}

		public ActionResult SampleGraph()
		{
			return View();
		}

		public ActionResult Manage()
		{
			var model = new RecordsManageViewModel();

			model.TotalRecords = AppDbContext.LogRecords?.Count();
			model.LastEntryTimestamp = AppDbContext.LogRecords.OrderByDescending(x => x.TimestampUTC).Take(1).FirstOrDefault()?.TimestampUTC;
			model.DatabaseSize = (new FileInfo(DBLocation))?.Length;

			return View(model);
		}

		// JSON Actions

		public ActionResult GetMessageClasses()
		{
			List<string> data = new List<string>();

			using (var context = AppDbContext)
				data = context.LogRecords.Select(x => x.MessageClass).Distinct().ToList();

			return SerializeToJsonString(data);
		}

		public ActionResult GetDateGroupedMessages(string typeFilter)
		{
			IEnumerable<DateGroupedMessages> model = new List<DateGroupedMessages>();

			using (var context = AppDbContext)
			{
				IQueryable<LogRecord> messages = context.LogRecords;

				// Bust it down to a single type of messages, if asked for
				if (!string.IsNullOrEmpty(typeFilter))
					messages = messages.Where(x => x.MessageClass.Equals(typeFilter, StringComparison.InvariantCultureIgnoreCase));

				// Group by the results, and send back the date/count
				var results = messages
					.GroupBy(x => new
					{
						x.TimestampUTC.Year,
						x.TimestampUTC.Month,
						x.TimestampUTC.Day,
						x.TimestampUTC.Hour
					})
					.Select(x => new { x.Key, Count = x.Count() });

				// Really, Linq? You're going to do this...
				model = results
					.ToList()
					.Select(x => new DateGroupedMessages() { Date = new DateTime(x.Key.Year, x.Key.Month, x.Key.Day, x.Key.Hour, 0, 0), Total = x.Count });
			}

			return SerializeToJsonFormatted(model);
		}

		public ActionResult GetTopOfMessages(int topCount, DateTime? startDate, DateTime? endDate)
		{
			IQueryable<LogRecord> model = null;

			if (topCount <= 0)
				topCount = 3;

			using (var context = AppDbContext)
			{
				model = context.LogRecords;

				if (startDate.HasValue)
				{
					model = model.Where(x => x.TimestampUTC >= startDate);
					if (endDate.HasValue)
						model = model.Where(x => x.TimestampUTC <= endDate);
				}

				var modelCount = model.Count();

				var modelPerc = model
					.GroupBy(x => x.MessageClass)
					.Select(x => new { x.Key, Count = x.Count() })
					.Select(x => new { MessageClass = x.Key, Count = x.Count, Total = modelCount, Perc = ((double)x.Count / modelCount) * 100 }) // No round trips please
					.OrderByDescending(x => x.Count)
					.Take(topCount)
					.ToList();

				return SerializeToJsonFormatted(modelPerc);
			}
		}

		public ActionResult GetMessageTypeVsOthers(string typeFilter)
		{
			if (string.IsNullOrEmpty(typeFilter)) throw new ArgumentNullException(nameof(typeFilter), "A message type filter must be provided.");

			using (var context = AppDbContext)
			{
				IQueryable<LogRecord> messages = context.LogRecords;

				// Group by the results, and send back the date/count
				var results = messages
					.GroupBy(x => new
					{
						x.TimestampUTC.Year,
						x.TimestampUTC.Month,
						x.TimestampUTC.Day,
						x.MessageClass
					})
					.Select(x => new { x.Key, Count = x.Count() });

				// Really, Linq? You're going to do this...
				var model = results
					.ToList()
					.Select(x => new { Date = new DateTime(x.Key.Year, x.Key.Month, x.Key.Day), MessageClass = x.Key.MessageClass, Total = x.Count });

				var targetData = model.Where(x => x.MessageClass.ToLower() == typeFilter.ToLower());
				var otherData = model.Where(x => x.MessageClass.ToLower() != typeFilter.ToLower())
					.GroupBy(x => x.Date)
					.Select(x => new { Date = x.Key, Total = x.Sum(y => y.Total) });

				var targetCount = targetData.Sum(x => x.Total);
				var otherCount = otherData.Sum(x => x.Total);

				var dataModel = new
				{
					Target = typeFilter,
					TargetCount = targetCount,
					OtherCount = otherCount,
					Data = new
					{
						Target = targetData.OrderBy(x => x.Date),
						Others = otherData.OrderBy(x => x.Date)
					}
				};

				return SerializeToJsonFormatted(dataModel);

			}


		}

		public ActionResult GetMessageOccuranceByCountry()
		{
			using (var context = AppDbContext)
			{
				var data = context.LogRecords
					.GroupBy(x => x.Country)
					.Select(x => new {
						Country = x.Key,
						Total = x.Count(),
					});
				
				return SerializeToJsonFormatted(data.ToList());
			}
		}

		public ActionResult Populate()
		{
			if (!Request.IsAjaxRequest())
			{
				SetRedirectError("This method is not allowed via that protocol.");
				return RedirectToActionPermanent("Manage");
			}

			return Json(new { status = "complete", canPopulate = true, path = LogFilePath }, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Delete()
		{
			if (!Request.IsAjaxRequest())
			{
				SetRedirectError("This method is not allowed via that protocol.");
				return RedirectToActionPermanent("Manage");
			}

			return Json(new { status = "complete", canDelete = true, path = LogFilePath }, JsonRequestBehavior.AllowGet);
		}

		// Complex Filtering

		public ActionResult GetRecords(DataTableDateFilteredRequest rules)
		{
			IList<LogRecord> filteredData = null;
			int totalRecords = 0;
			int filteredCount = 0;

			using (var context = AppDbContext)
			{
				context.Configuration.ProxyCreationEnabled = false;
				context.Configuration.LazyLoadingEnabled = false;

				var prepModel = context.LogRecords
					.Include(x => x.FailedTests);

				// Date Filtering
				if (rules.fromDate.HasValue && rules.fromDate != DateTime.MinValue)
				{
					prepModel = prepModel.Where(x => x.TimestampUTC >= rules.fromDate.Value);

					if (rules.toDate.HasValue && rules.toDate != DateTime.MinValue)
					{
						prepModel = prepModel.Where(x => x.TimestampUTC <= rules.toDate.Value);
					}
				}

				// Searching all columns, ugh
				if (rules.search != null && !string.IsNullOrEmpty(rules.search.value))
				{
					var searchVal = rules.search.value.ToLower();
					var isStrictSearch = (searchVal.StartsWith("\"") && searchVal.EndsWith("\""));
					searchVal = searchVal.Replace("\"", "");

					var filter = string.Join(" OR ", rules.columns
						.Where(x => x.searchable && x.data.ToLower() != "timestamputc")
						.Select(x => (isStrictSearch) ? $"{x.data}.ToLower().Equals(@0)" : $"{x.data}.ToLower().Contains(@0)")
					);
					// Should do a little clean up for us
					prepModel = prepModel.Where(filter, searchVal);
				}

				// Column-based searching
				if (rules.columns.Any(x => x.searchable && x.search != null && !string.IsNullOrEmpty(x.search.value)))
				{
					foreach (var searchColumn in rules.columns.Where(x => x.searchable && x.search != null && !string.IsNullOrEmpty(x.search.value) && x.data.ToLower() != "timestamputc"))
					{
						var searchVal = searchColumn.search.value.ToLower();
						var isStrictSearch = (searchVal.StartsWith("\"") && searchVal.EndsWith("\""));
						searchVal = searchVal.Replace("\"", "");

						var filter = (isStrictSearch) ? $"{searchColumn.data}.ToLower().Equals(@0)" : $"{searchColumn.data}.ToLower().Contains(@0)";

						prepModel = prepModel.Where(filter, searchVal);
					}
				}

				// Order (there is a requirement to have one)
				if (rules.order.Any())
				{
					foreach (var orderColumn in rules.order)
					{
						var column = rules.columns[orderColumn.column];
						prepModel = prepModel.OrderBy($"{column.data} {orderColumn.dir}");
					}
				}
				else
					prepModel = prepModel.OrderBy("TimestampUTC DESC");


				filteredCount = prepModel.Count();

				// Don't do Skip()/Take() if they passed in a length that is less than 1
				if (rules.length > 1)
					filteredData = prepModel.Skip(rules.start).Take(rules.length).ToList();
				else
					filteredData = prepModel.ToList();

				totalRecords = 250000; // context.LogRecords.Count(); // I know, complain. This is a school project.
			}

			// Going to remove some stuff
			for (int i = 0; i < filteredData.Count; i++)
				filteredData[i].FailedTests = null; // We don't need it, and i'm lazy.

			return SerializeToJsonFormatted(new DataTableFilterResponse<LogRecord>()
			{
				draw = rules.draw,
				data = filteredData,
				recordsFiltered = filteredCount,
				recordsTotal = totalRecords
			});
		}

		// Old Ajax Queries

		public ActionResult GetMessageMetrics()
		{
			var dataset = new Dictionary<string, Dictionary<long, int>>();
			long[] graphLabels = null;

			using (var context = AppDbContext)
			{
				var lastTimestamp = context.LogRecords.OrderByDescending(x => x.TimestampUTC).FirstOrDefault().TimestampUTC; // get the last timestamp
				var startingTimestamp = new DateTime(lastTimestamp.Year, lastTimestamp.Month, lastTimestamp.Day, 0, 0, 0); // Beginning of the day
				var endingTimestamp = startingTimestamp.AddHours(23).AddMinutes(59); // End of the day

				var messages = context.LogRecords
					.Where(x => x.TimestampUTC >= startingTimestamp && x.TimestampUTC <= endingTimestamp)
					.Select(x => new { x.TimestampUTC, x.MessageClass })
					.ToList() // Returns results from DB
					.GroupBy(x => new { Timestamp = x.TimestampUTC.RoundDown(TimeSpan.FromMinutes(5)), x.MessageClass })
					.Select(x => new MessageData()
					{
						Timestamp = x.Key.Timestamp,
						MessageClass = x.Key.MessageClass,
						Count = x.Count()
					})
					.OrderBy(x => x.Timestamp);


				foreach (var message in messages)
				{
					if (!dataset.Keys.Contains(message.MessageClass))
						dataset.Add(message.MessageClass, new Dictionary<long, int>());

					dataset[message.MessageClass].Add(message.Timestamp.ToJavaScriptMilliseconds(), message.Count);
				}

			}
			return SerializeToJsonString(new { labels = graphLabels, data = dataset });
		}

		public ActionResult GetMessageClassMetrics()
		{
			List<BaseMessageData> data = new List<BaseMessageData>();

			using (var context = AppDbContext)
			{
				data = context.LogRecords
					.GroupBy(x => x.MessageClass)
					.Select(x => new BaseMessageData
					{
						MessageClass = x.Key,
						Count = x.Count()
					})
					.OrderByDescending(x => x.Count)
					.ToList();
			}

			return SerializeToJsonString(data);
		}

		public ActionResult GetTopSendingIPs()
		{
			List<SendingIPData> data = new List<SendingIPData>();

			using (var context = AppDbContext)
			{
				data = context.LogRecords
					.GroupBy(x => x.SendingIP)
					.Select(x => new SendingIPData()
					{
						SendingIP = x.Key,
						Count = x.Count()
					})
					.OrderByDescending(x => x.Count)
					.Take(10)
					.ToList();
			}

			return SerializeToJsonString(data);
		}

		public ActionResult GetTopSendingCountries()
		{
			List<CountriesData> data = new List<CountriesData>();

			using (var context = AppDbContext)
			{
				data = context.LogRecords
					.GroupBy(x => x.Country)
					.Select(x => new CountriesData()
					{
						Country = x.Key,
						Count = x.Count()
					})
					.OrderByDescending(x => x.Count)
					.Take(10)
					.ToList();
			}

			return SerializeToJsonString(data);
		}

		public ActionResult GetTopSendingCountriesOfMessageClass(string msgClass)
		{
			if (string.IsNullOrEmpty(msgClass))
				return null;

			msgClass = msgClass.ToUpper();

			dynamic data;

			using (var context = AppDbContext)
			{
				data = context.LogRecords
					.Where(x => x.MessageClass.ToUpper().Contains(msgClass))
					.GroupBy(x => new { x.MessageClass, x.Country })
					.Select(x => new { x.Key.MessageClass, x.Key.Country, Count = x.Count() })
					.OrderByDescending(x => x.Count)
					.Take(10)
					.ToList();
			}

			return SerializeToJsonString(data);
		}

		public ActionResult GetMessageClassOverPercentage(string msgClass, int perc = 95)
		{
			if (string.IsNullOrEmpty(msgClass))
				return null;

			msgClass = msgClass.ToUpper();

			dynamic data = null;

			using (var context = AppDbContext)
			{
				data = context.LogRecords
					.GroupBy(x => new { x.SendingIP, x.MessageClass })
					.Select(x => new
					{
						x.Key.SendingIP,
						x.Key.MessageClass,
						Total = x.Count()
					})
					.ToList()
					.GroupBy(x => x.SendingIP)
					.Select(x =>
					{
						var totalType = x.Where(v => v.MessageClass == msgClass).Sum(v => v.Total);
						var total = x.Sum(v => v.Total);
						var percType = totalType * 100 / total;

						return new
						{
							SendingIP = x.Key,
							MessageType = msgClass,
							TotalOfType = totalType,
							Total = total,
							PercOfType = percType
						};
					})
					.Where(x => x.PercOfType >= perc)
					.OrderByDescending(x => x.PercOfType)
					.Take(20) // Artifical limiting
					.ToList() // Need to make it usable!
					;

			}

			return SerializeToJsonString(data);
		}



	}
}