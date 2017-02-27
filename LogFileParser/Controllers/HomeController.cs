using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

using LogFileParser.Models;
using LogFileParser.Models.Helpers;

using LogFileParser.ViewModels;
using X.PagedList;
using Newtonsoft.Json;

namespace LogFileParser.Controllers
{
	public class HomeController : BaseController<LogDbContext>
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Test()
		{
			return View();
		}

	}
}