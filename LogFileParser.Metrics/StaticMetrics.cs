using System;
using System.Collections.Generic;
using System.Linq;
//using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using LogFileParser.Models;


namespace LogFileParser.Metrics
{
    public static class StaticMetrics
    {

		public static IEnumerable<LogRecord> GetRecordsByDateRange(this LogDbContext context, 
			DateTime? startDate = null, DateTime? endDate = null)
		{
			startDate = startDate.GetValueOrDefault(DateTime.MinValue);
			endDate = endDate.GetValueOrDefault(DateTime.MaxValue);


			return null;
		}

    }
}
