namespace LogFileParser.Models
{
	using CsvHelper;
	using CsvHelper.Configuration;
	using SQLite.CodeFirst;
	using System.Collections.Generic;
	using System.Data.Entity;
	using System.IO;
	using System.Linq;

	public class LogDbContext : DbContext
	{
		public string SourceLogFile { get; set; }

		public DbSet<LogRecord> LogRecords { get; set; }

		public DbSet<FailedTest> FailedTests { get; set; }

		const string CONTEXT_NAME = @"LogDbContext";

		public LogDbContext() : base(CONTEXT_NAME) { }

		public LogDbContext(string sourceLogFile) : base(CONTEXT_NAME) { SourceLogFile = sourceLogFile; }
		
		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			Database.SetInitializer(new LogDBContextInitializer(modelBuilder, SourceLogFile));
		}
	}

	public class LogDBContextInitializer : SqliteDropCreateDatabaseWhenModelChanges<LogDbContext>
	{
		readonly string sourceFile;

		public LogDBContextInitializer(DbModelBuilder modelBuilder, string sourceFile)
			: base(modelBuilder)
		{
			this.sourceFile = sourceFile;
		}

		protected override void Seed(LogDbContext context)
		{
			// Load the table with the data from the passed in file.

			if (string.IsNullOrEmpty(sourceFile))
				return;
			
			foreach (LogRecord record in GetRecordsFromFile(sourceFile).Take(10))
			{
				foreach (var test in record.FailedTestCollection)
				{
					record.FailedTests.Add(new FailedTest() { Name = test, RecordId = record.MessageId });
				}

				context.LogRecords.Add(record);
			}

		}

		public static IEnumerable<LogRecord> GetRecordsFromFile(string fileName)
		{
			if (!File.Exists(fileName))
				yield break;

			using (var streamReader = new StreamReader(fileName))
			{
				using (var csv = new CsvReader(streamReader))
				{
					csv.Configuration.RegisterClassMap<LogRecordClassMap>(); // Need to map to class since no header exists
					csv.Configuration.HasHeaderRecord = false; // The file doesn't have an header
					csv.Configuration.Delimiter = "\t"; // Tabbed delimited
					while (csv.Read()) // Read each line and return it
						yield return csv.GetRecord<LogRecord>();
				}
			}
		}

		class LogRecordClassMap : CsvClassMap<LogRecord>
		{
			public LogRecordClassMap()
			{
				Map(x => x.TimestampUTC);
				Map(x => x.MessageClass);
				Map(x => x.MessageId);
				Map(x => x.SendingIP);
				Map(x => x.ReceivingIP);
				Map(x => x.Country);
				Map(m => m.FailedTestsString); //.ConvertUsing(row => row.GetField(6).Split(',').Select(r => r.Trim()).ToList()); // Use this if you don't have a new type/table for the FailTests List
			}
		}

	}

}