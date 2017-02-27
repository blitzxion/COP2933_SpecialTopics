using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LogFileParser.Models;
using System.Linq;

namespace LogFileParser.Tests
{
	[TestClass]
	public class DbUnitTests
	{
		string testFile = @"C:\Users\Richard\Dropbox\School\Projects\COP2933_SpecialTopics\TestData\mailfilter-20161202.log";


		[TestMethod]
		public void VerifyDatabaseFileDropAndCreated()
		{
			int result = 0;
			using (var context = new LogDbContext(testFile))
				result = context.LogRecords.Count();

			Log.Debug(@"Found Results: {0}", result);

			Assert.IsTrue((result > 0), "There should be at least 1 result.");
		}

		[TestMethod]
		public void VerifyRecordsAreReturnedWithoutLogFileKnowledge()
		{
			int result = 0;
			using (var context = new LogDbContext(testFile))
				result = context.LogRecords.Count();

			Log.Debug(@"Found Results: {0}", result);

			Assert.IsTrue((result > 0), "There should be at least 1 result after creating a context with a logfile.");

			int newResult = 0;
			using (var context = new LogDbContext())
				newResult = context.LogRecords.Count();

			Assert.IsTrue((newResult > 0), "There should be at least 1 result.");
			Assert.AreEqual(result, newResult, "The results from each context should be the same.");
		}

		[TestMethod]
		public void VerifyRecordPopulatedFailedTestCollection()
		{

			LogRecord result = null;
			using (var context = new LogDbContext(testFile))
				result = context.LogRecords.FirstOrDefault(x => x.FailedTestsString != null);

			Assert.IsNotNull(result, "The query should have returned a single result set.");
			Assert.IsNotNull(result.FailedTestsString, "This record has a value that should have returned a comma seperated value on this call.");
			Assert.IsNotNull(result.FailedTestCollection, "This record has a value that should have returned a collection on this call.");
			Assert.IsTrue(result.FailedTestCollection.Count > 0, "This record has a value that should at least return one value in the collection");
		}

		[TestMethod]
		public void VerifyRecordHasFailedTests()
		{
			LogRecord result = null;
			using (var context = new LogDbContext(testFile))
			{
				result = context.LogRecords.FirstOrDefault(x => x.FailedTestsString != null);

				Assert.IsNotNull(result, "The query should have returned a single result set.");
				Assert.IsNotNull(result.FailedTestsString, "This record has a value that should have returned a comma seperated value on this call.");
				Assert.IsNotNull(result.FailedTests, "This record has a value that should have returned a comma seperated value on this call.");
			}
		}
	}
}
