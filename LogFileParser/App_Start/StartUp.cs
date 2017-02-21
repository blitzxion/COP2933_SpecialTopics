using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(LogFileParser.StartUp))]
namespace LogFileParser
{
	public class StartUp
	{
		public void Configuration(IAppBuilder app)
		{
			app.MapSignalR();
		}
	}
}