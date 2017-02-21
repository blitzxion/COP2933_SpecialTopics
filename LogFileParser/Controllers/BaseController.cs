using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;

using LogParserModels.Models;

namespace LogFileParser.Controllers
{
	[CopyMessageTempDataFilter]
	public abstract class BaseController<TContext>
		: Controller
		where TContext : DbContext 
	{
		private TContext _appDbContext;
		public TContext AppDbContext
		{
			get
			{
				return _appDbContext ?? (TContext)Activator.CreateInstance(typeof(TContext));
			}
			private set
			{
				_appDbContext = value;
			}
		}
		

		public BaseController()
		{

		}

		public BaseController(TContext appDbContext)
			: this()
		{
			AppDbContext = appDbContext;
		}

		public void ResetAppContext(TContext newContext = null)
		{
			if (_appDbContext != null)
				_appDbContext.Dispose();

			_appDbContext = newContext;
		}

		/// <summary>
		/// Sets a error to display on the next redirect/postback.  This will survive a redirect!
		/// </summary>
		/// <param name="error"></param>
		public void SetRedirectError(string error)
		{
			TempData["error"] = error;
		}
		/// <summary>
		/// Sets a warning to display on the next redirect/postback.  This will survive a redirect!
		/// </summary>
		/// <param name="warning"></param>
		public void SetRedirectWarning(string warning)
		{
			TempData["warning"] = warning;
		}
		/// <summary>
		/// Sets a alert (Action Required) to display on the next redirect/postback.  This will survive a redirect!
		/// </summary>
		/// <param name="warning"></param>
		public void SetRedirectActionRequired(string action)
		{
			TempData["ActionRequired"] = action;
		}
		/// <summary>
		/// Sets a message to display on the next redirect/postback.  This will survive a redirect!
		/// </summary>
		/// <param name="message"></param>
		public void SetRedirectInfo(string message)
		{
			TempData["info"] = message;
		}
		/// <summary>
		/// Sets a succes message to display on the next redirect/postback.  This will survive a redirect!
		/// </summary>
		/// <param name="message"></param>
		public void SetRedirectSuccess(string message)
		{
			TempData["success"] = message;
		}

	}
}