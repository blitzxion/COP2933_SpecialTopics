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
			// Prepare for Query Builder, using Filter!
			List<LogRecord> results = new List<LogRecord>();
			IQueryable query = AppDbContext.LogRecords.BuildQuery(filterRule.Rules);

			// Get this out of the way first, just easier that way
			// LIMIT
			query = query.Take(filterRule.Limit);

			// Select (only if there are no group by fields)
			if(filterRule.SelectFields != null && filterRule.SelectFields.Any(x => !string.IsNullOrEmpty(x.Name)))
			{
				if(filterRule.SelectFields.Any(x => !string.IsNullOrEmpty(x.Augment)))
				{
					// TODO: Figure this crap out...
				} else
					query = query.Select($"new ({string.Join(", ", filterRule.SelectFields.Select(x => x.Name))})");
			}

			

			var jsonResults = JsonConvert.SerializeObject(query, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
			results = JsonConvert.DeserializeObject<List<LogRecord>>(jsonResults);

			return SerializeForJson(results);
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
			public List<FilterField> SelectFields { get; set; }
			public int Limit { get; set; }
			public FilterRule Rules { get; set; }

			public FilteringRules()
			{
				SelectFields = new List<FilterField>();
				Limit = 100;
			}

		}

		public class FilterField
		{
			public string Name { get; set; }
			public string Augment { get; set; }
		}

	}
}