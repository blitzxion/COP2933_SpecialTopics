using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LogFileParser
{
	public class FormattedJsonResult : JsonResult
	{
		private const string _dateFormat = "yyyy-MM-ddTHH:mm:ss";

		public override void ExecuteResult(ControllerContext context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			HttpResponseBase response = context.HttpContext.Response;

			response.ContentType = !string.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

			if (ContentEncoding != null)
				response.ContentEncoding = ContentEncoding;

			if (Data != null)
			{
				// Using Json.NET serializer
				var isoConvert = new IsoDateTimeConverter();
				isoConvert.DateTimeFormat = _dateFormat;
				response.Write(JsonConvert.SerializeObject(Data, new JsonSerializerSettings() {
					ContractResolver = new CamelCasePropertyNamesContractResolver(),
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
					Converters = new List<JsonConverter>() {
						isoConvert
					}
				}));
			}

		}
	}
}