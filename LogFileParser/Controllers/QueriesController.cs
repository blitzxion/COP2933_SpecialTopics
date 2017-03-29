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
			var query = AppDbContext.LogRecords.BuildQuery(filterRule.Rules);

			// Get this out of the way first, just easier that way
			// LIMIT
			query = query.Take(filterRule.Limit);

			// GROUP BY, RABBLE
			if(filterRule.GroupBy != null && filterRule.GroupBy.Any())
			{
				// Do something with the properties
				query = query.GroupBy(filterRule.GroupBy).SelectMany(x => x);
				//var selQuery = query.Select("new (" + string.Join(", ", filterRule.Select) + ")");
				
				results = query.ToList();
			}
			else
			{
				// If there wasn't a group, flatten out the query into a list
				results = query.ToList();
			}


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
			public List<string> Select { get; set; }
			public List<string> GroupBy { get; set; }
			public int Limit { get; set; }
			public FilterRule Rules { get; set; }

			public FilteringRules()
			{
				GroupBy = new List<string>();
				Select = new List<string>();
				Limit = 100;
			}

		}

	}
}