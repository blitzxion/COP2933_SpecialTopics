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
			ViewBag.Model = AppDbContext.LogRecords.Take(100).ToList(); // Just display the first 100 records
			return View();
        }

		[HttpPost]
		public ActionResult TestQuery(FilteringRules filterRule)
		{
			IPagedList<LogRecord> model = null;

			// Prepare for Query Builder, using Filter!
			var query = AppDbContext.LogRecords.BuildQuery(filterRule.Rules);

			// Get this out of the way first, just easier that way
			// LIMIT
			//if(filterRule.Limit.HasValue)
			//	query = query.Take(filterRule.Limit.GetValueOrDefault(10)); // 10 to be safe

			// Select (only if there are no group by fields)
			//if(filterRule.SelectFields != null && filterRule.SelectFields.Any(x => !string.IsNullOrEmpty(x.Name)))
			//{
			//	if(filterRule.SelectFields.Any(x => !string.IsNullOrEmpty(x.Augment)))
			//	{
			//		// TODO: Figure this crap out...
			//	} else
			//		query = query.Select($"new ({string.Join(", ", filterRule.SelectFields.Select(x => x.Name))})");
			//}

			//var jsonResults = JsonConvert.SerializeObject(query, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
			//results = JsonConvert.DeserializeObject<List<LogRecord>>(jsonResults);

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

			return SerializeForJson(objData);
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