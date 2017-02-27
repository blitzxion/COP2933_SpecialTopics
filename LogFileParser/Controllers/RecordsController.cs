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
			return SerializeForJson(new { labels = graphLabels, data = dataset });
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

			return SerializeForJson(data);
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

			return SerializeForJson(data);
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

			return SerializeForJson(data);
		}

		// Utils

		private JsonResult SerializeForJson(object obj)
		{
			return Json(JsonConvert.SerializeObject(obj), JsonRequestBehavior.AllowGet);
		}

	}
}