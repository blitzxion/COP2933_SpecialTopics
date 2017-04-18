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
	public class LogRecordTickerHub : Hub<IMessageDataMethods>
	{

		private readonly LogRecordTicker _logRecordTicker;

		public LogRecordTickerHub()
			:this(LogRecordTicker.Instance)
		{

		}

		public LogRecordTickerHub(LogRecordTicker logRecordTicker)
		{
			_logRecordTicker = logRecordTicker;
		}

		public string GetTickerState()
		{
			return _logRecordTicker.TickerState.ToString();
		}

		public void StartListening()
		{
			_logRecordTicker.StartListening();
		}

		public void StopListening()
		{
			_logRecordTicker.StopListening();
		}

		public void Reset()
		{
			_logRecordTicker.Reset();
		}

	}
}