using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Newtonsoft.Json;
using System.Web.Mvc;
using System.IO;

namespace LogFileParser
{

	public class JsonFilter : ActionFilterAttribute
	{
		public string Parameter { get; set; }
		public Type JsonDataType { get; set; }

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (filterContext.HttpContext.Request.ContentType.Contains("application/json"))
			{
				string inputContent;
				using (var sr = new StreamReader(filterContext.HttpContext.Request.InputStream))
				{
					inputContent = sr.ReadToEnd();
				}

				var result = JsonConvert.DeserializeObject(inputContent, JsonDataType);
				filterContext.ActionParameters[Parameter] = result;
			}
		}
	}

}