﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR;
using LogFileParser.Models;
using X.PagedList;
using System.Linq;
using System.Threading.Tasks;

namespace LogFileParser.Hubs
{
	public interface IMessageDataMethods
	{
		void tickerUpdate(dynamic record);
		void tickerStarted();
		void tickerStopped();
		void tickerReset();
	}

	public class LogRecordTicker
	{
		private readonly static Lazy<LogRecordTicker> _instance = new Lazy<LogRecordTicker>(() => new LogRecordTicker(GlobalHost.ConnectionManager.GetHubContext<LogRecordTickerHub, IMessageDataMethods>().Clients));


		public static LogRecordTicker Instance
		{
			get
			{
				return _instance.Value;
			}
		}

		private IHubConnectionContext<IMessageDataMethods> Clients { get; set; }

		private readonly object _tickerStateLock = new object();
		private readonly object _tickerRecordLock = new object();

		private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(2000);

		private Timer _timer;
		private volatile bool _emittingRecords;
		private volatile TickerState _tickerState;

		public TickerState TickerState
		{
			get { return _tickerState; }
			set { _tickerState = value; }
		}

		private LogRecordTicker(IHubConnectionContext<IMessageDataMethods> clients)
		{
			Clients = clients;
		}
		

		public void StartListening()
		{
			lock(_tickerStateLock)
			{
				if(TickerState != TickerState.Open)
				{
					_timer = new Timer(TryEmitRecords, null, _updateInterval, _updateInterval);

					TickerState = TickerState.Open;

					// Broadcast
					BroadcastTickerStateChange(TickerState.Open);
				}
			}
		}

		public void StopListening()
		{
			lock(_tickerStateLock)
			{
				if(TickerState == TickerState.Open)
				{
					if (_timer != null)
						_timer.Dispose();

					TickerState = TickerState.Closed;

					BroadcastTickerStateChange(TickerState.Closed);
				}
			}
		}

		public void Reset()
		{
			lock(_tickerStateLock)
			{
				if (TickerState != TickerState.Closed)
					throw new InvalidOperationException("Ticker must be stopped before resetting.");

				// Broadcast
				BroadcastReset();
			}
		}


		private void TryEmitRecords(object state)
		{
			lock(_tickerRecordLock)
			{
				if(!_emittingRecords)
				{
					_emittingRecords = true;

					using (var context = new LogDbContext())
					{
						var records = context.LogRecords
							.OrderBy(x => x.TimestampUTC)
							.GroupBy(x => new
							{
								x.TimestampUTC.Year,
								x.TimestampUTC.Month,
								x.TimestampUTC.Day,
								x.TimestampUTC.Hour,
								x.TimestampUTC.Minute
							})
							.Select(x => new { x.Key, Count = x.Count() });					

						var rEnum = records.GetEnumerator();

						while(rEnum.MoveNext() && TickerState == TickerState.Open)
						{
							var record = rEnum.Current;

							BroadcastRecord(new Tuple<DateTime, int>(
								new DateTime(record.Key.Year, record.Key.Month, record.Key.Day, record.Key.Hour, record.Key.Minute, 0),
								record.Count
							));

							Task.Delay(150).Wait();
						}
					}

					_emittingRecords = false;
				}
			}
		}

		private void BroadcastTickerStateChange(TickerState state)
		{
			switch (state)
			{
				case TickerState.Closed:
					Clients.All.tickerStopped();
					break;
				case TickerState.Open:
					Clients.All.tickerStarted();
					break;
				default:
					break;
			}
		}

		private void BroadcastRecord(dynamic record)
		{
			Clients.All.tickerUpdate(record);
		}

		private void BroadcastReset()
		{
			Clients.All.tickerReset();
		}

	}

	public enum TickerState
	{
		Closed,
		Open
	}

}