using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogFileParser.Models
{
	public partial class LogRecord
	{
		public DateTime TimestampUTC { get; set; }
		public string MessageClass { get; set; }

		[Key]
		public string MessageId { get; set; }

		public string SendingIP { get; set; }
		public string ReceivingIP { get; set; }
		public string Country { get; set; }
		public string FailedTestsString { get; set; }

		public virtual List<string> FailedTestCollection { get { return FailedTestsString?.Split(',').Select(r => r.Trim()).ToList(); } }

		public virtual ICollection<FailedTest> FailedTests { get; set; }


		public LogRecord()
		{
			FailedTests = new HashSet<FailedTest>();
		}
	}
}

