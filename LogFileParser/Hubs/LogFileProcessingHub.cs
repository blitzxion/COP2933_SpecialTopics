using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

using LogParserModels.Models;
using System.Data.Entity;
using LogFileParser.Helpers;
using EntityFramework.Extensions;
using LogParserModels.Helpers;

namespace LogFileParser.Hubs
{
	public interface ILogFileProcessing
	{
		void ProcessFileStart();
		void ProcessFileProgress(int position, int count);
		void ProcessFileComplete();
		void ProcessFileError(string reason);
		void ProcessFileMilestone(int position);
	}

	public class LogFileProcessingHub : Hub<ILogFileProcessing>
	{

		public void ProcessFile(string filename)
		{
			// We're not going to use the DBContext's seed here, we're going to do it manually
			try
			{
				Clients.Caller.ProcessFileStart();

				LogDbContext context = new LogDbContext();

				var sourceRecords = LogDBContextInitializer.GetRecordsFromFile(filename);
				var position = 0;
				var total = sourceRecords.Count();


				foreach (var record in sourceRecords) {
					foreach (string test in record.FailedTestCollection)
						record.FailedTests.Add(new FailedTest() { Name = test, RecordId = record.MessageId });

					context = EntityPerformance.SaveToContext(context, record, ++position, onSave: pos => {
						Clients.Caller.ProcessFileProgress(pos, total);
					});
				}

				Clients.Caller.ProcessFileComplete();

			}
			catch (Exception ex)
			{
				Clients.Caller.ProcessFileError(ex.ToString());
			}
		}

		public void ClearDB()
		{
			try
			{
				Clients.Caller.ProcessFileStart();

				var context = new LogDbContext();
				var file = ConnectionStringParser.GetDataSource(context.Database.Connection.ConnectionString);

				context.Database.Connection.Close();
				GC.Collect();
				GC.WaitForPendingFinalizers();
				System.IO.File.Delete(file);

				using (context = new LogDbContext())
					context.Database.Initialize(true);

				Clients.Caller.ProcessFileComplete();

			}
			catch (Exception ex)
			{
				Clients.Caller.ProcessFileError(ex.ToString());
			}
		}

		//public void ClearDB()
		//{
		//	int position, total = 0;

		//	try
		//	{
		//		using (var context = new LogDbContext())
		//		{
		//			Clients.Caller.ProcessFileStart();

		//			position = 0;
		//			total = context.LogRecords.Count();
		//			double lastPositionPerc = 0;

		//			// We're borrowing this
		//			foreach (LogRecord record in context.LogRecords)
		//			{
		//				record.FailedTests.Clear();
		//				context.LogRecords.Remove(record);
		//				Clients.Caller.ProcessFileProgress(position, total);

		//				TrackProgress(position, total, lastPositionPerc, x =>
		//				{
		//					lastPositionPerc = x;
		//					Clients.Caller.ProcessFileMilestone(position);
		//					context.SaveChanges();
		//				});

		//				position++; // Don't forget this
		//			}

		//			context.SaveChanges(); // Just in case the last one doesn't get fired.
		//			Clients.Caller.ProcessFileComplete();
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		Clients.Caller.ProcessFileError(ex.ToString());
		//	}
		//}

		void TrackProgress(int pos, int count, double actionPrecetage, Action<double> action)
		{
			double perc = ((double)pos / count) * 100;
			double fPerc = Math.Floor(perc);

			if (fPerc != actionPrecetage)
				action(fPerc);
		}

		//LogDbContext AddToContext(LogDbContext context, LogRecord record, int count, int commitCount, bool recreateContext)
		//{
		//	context.Set<LogRecord>().Add(record);

		//	if (count % commitCount == 0)
		//	{
		//		context.SaveChanges();
		//		if (recreateContext)
		//		{
		//			context.Dispose();
		//			context = new LogDbContext();
		//			context.Configuration.AutoDetectChangesEnabled = false;
		//		}
		//	}

		//	return context;
		//}

		//LogDbContext RemoveFromContext(LogDbContext context, LogRecord record, int count, int commitCount, bool recreateContext)
		//{
		//	context.Set<LogRecord>().Remove(record);

		//	if (count % commitCount == 0)
		//	{
		//		context.SaveChanges();
		//		if (recreateContext)
		//		{
		//			context.Dispose();
		//			context = new LogDbContext();
		//			context.Configuration.AutoDetectChangesEnabled = false;
		//		}
		//	}

		//	return context;
		//}

	}
}