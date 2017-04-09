using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using LogFileParser.Models;
using LogFileParser.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Castle.DynamicLinqQueryBuilder;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using X.PagedList;

namespace LogFileParser.Controllers
{
    public class QueriesController : BaseController<LogDbContext>
    {

		// GET: Queries
		public ActionResult Index()
        {
			// Prepare for Query Builder
			ViewBag.FilterDefinition = GetDefaultFilterDefinition(typeof(LogRecord));
			ViewBag.Model = AppDbContext.LogRecords.Take(10).ToList();
			return View();
        }

		public ActionResult CountryMessages()
		{
			ViewBag.FilterDefinition = GetDefaultFilterDefinition(typeof(LogRecord));
			ViewBag.Model = AppDbContext.LogRecords.Take(10).ToList();

			return View();
		}

		public ActionResult CountryMessagesGlobal()
		{
			ViewBag.FilterDefinition = GetDefaultFilterDefinition(typeof(LogRecord));
			ViewBag.Model = AppDbContext.LogRecords.Take(10).ToList();

			return View();
		}

		[HttpPost]
		public ActionResult TestQuery(FilteringRules filterRule)
		{
			IPagedList<LogRecord> model = null;

			// Prepare for Query Builder, using Filter!
			var query = AppDbContext.LogRecords.BuildQuery(filterRule.Rules);
			var page = filterRule.Page.GetValueOrDefault(1);
			var pageSize = filterRule.Limit.GetValueOrDefault(10);

			model = query
				.OrderByDescending(x => x.TimestampUTC)
				.ToPagedList(page, pageSize);

			var objData = new {
				data = model,
				CurrentPage = page,
				PreviousPage = page - 1,
				NextPage = page + 1,
				LastPage = model.PageCount
			};

			return SerializeToJsonString(objData);
		}

		public ActionResult MessagesPerCountryQuery(FilteringRules filterRule)
		{
			// Prepare for Query Builder, using Filter!
			var query = AppDbContext.LogRecords
				.BuildQuery(filterRule.Rules) // can provide times here, or country
				.GroupBy(x => new { x.Country, x.MessageClass })
				.Select(x => new { x.Key.Country, x.Key.MessageClass, Total = x.Count() });
				
			var page = filterRule.Page.GetValueOrDefault(1);
			var pageSize = filterRule.Limit.GetValueOrDefault(10);

			var model = query
				.OrderBy(x => x.Total)
				.ToPagedList(page, pageSize);

			var objData = new
			{
				data = model,
				CurrentPage = page,
				PreviousPage = page - 1,
				NextPage = page + 1,
				LastPage = model.PageCount
			};

			return SerializeToJsonString(objData);
		}
		
		// Utils

		private string GetDefaultFilterDefinition(Type type)
		{
			var colDefs = type.GetDefaultColumnDefinitionsForType(false);

			// Thanks for helping, but you didn't help with this one...
			colDefs.ForEach(x => { x.Id = x.Field; });

			return JsonConvert.SerializeObject(colDefs, new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
			});
		}

		public class FilteringRules
		{
			//public List<FilterField> SelectFields { get; set; }
			public int? Limit { get; set; }
			public FilterRule Rules { get; set; }

			public int? Page { get; set; }

			public FilteringRules()
			{
				//SelectFields = new List<FilterField>();
				Limit = 100;
				Page = 1;
			}

		}

		//public class FilterField
		//{
		//	public string Name { get; set; }
		//	public string Augment { get; set; }
		//}

	}
}