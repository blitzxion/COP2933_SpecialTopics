using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace LogFileParser
{
	public static class HMTLHelperExtensions
	{
		public static string IsSelected(this HtmlHelper html, string controller = null, string action = null, string cssClass = null)
		{

			if (string.IsNullOrEmpty(cssClass))
				cssClass = "active";

			string currentAction = (string)html.ViewContext.RouteData.Values["action"];
			string currentController = (string)html.ViewContext.RouteData.Values["controller"];

			if (string.IsNullOrEmpty(controller))
				controller = currentController;

			if (string.IsNullOrEmpty(action))
				action = currentAction;

			return controller == currentController && action == currentAction ?
				cssClass : string.Empty;
		}

		public static string PageClass(this HtmlHelper html)
		{
			string currentAction = (string)html.ViewContext.RouteData.Values["action"];
			return currentAction;
		}
	}
}
