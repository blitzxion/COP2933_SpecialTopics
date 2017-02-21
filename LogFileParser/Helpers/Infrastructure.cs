using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LogFileParser
{
	/// <summary>
	/// Copies any messages/warning/errors from TempData to the ViewBag for use with built in notifications.
	/// </summary>
	public class CopyMessageTempDataFilter : ActionFilterAttribute
	{
		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var controller = filterContext.Controller;

			if (controller.TempData["Error"] != null)
				controller.ViewBag.Error = controller.TempData["Error"];
			if (controller.TempData["Info"] != null)
				controller.ViewBag.Info = controller.TempData["Info"];
			if (controller.TempData["Success"] != null)
				controller.ViewBag.Success = controller.TempData["Success"];
			if (controller.TempData["Warning"] != null)
				controller.ViewBag.Warning = controller.TempData["Warning"];
			if (controller.TempData["ActionRequired"] != null)
				controller.ViewBag.ActionRequired = controller.TempData["ActionRequired"];

			base.OnActionExecuted(filterContext);
		}
	}

	/// <summary>
	/// Controls the processing of application actions by redirecting to a specified URI. The error will be shown by compatible views looking for ViewBag.Error.
	/// </summary>
	public class RedirectWithErrorResult : RedirectResult
	{
		public RedirectWithErrorResult(string url, string error)
			: base(url)
		{
			Error = error;
		}

		public RedirectWithErrorResult(string url, string error, bool permanent)
			: base(url, permanent)
		{
			Error = error;
		}

		public string Error { get; set; }
		public override void ExecuteResult(ControllerContext context)
		{
			context.Controller.TempData["error"] = Error;
			base.ExecuteResult(context);
		}
	}
}