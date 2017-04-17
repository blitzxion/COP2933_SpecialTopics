using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.AspNet.SignalR;

using LogFileParser.Models;
using System.Data.Entity;
using LogFileParser.Helpers;
using EntityFramework.Extensions;
using LogFileParser.Models.Helpers;

namespace LogFileParser.Hubs
{
	public interface IMessageDataMethods
	{
		void OnFeedUpdate();
	}

	public class MessageDataHub : Hub<IMessageDataMethods>
	{

		public void StartLiveFeed()
		{
			// This method will simply start feeding out data to listeners
			// Will start the loop if it isn't already started?
			// Want to make sure of that so we don't have multiple loops going on
		}

	}
}