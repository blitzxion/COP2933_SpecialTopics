using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogFileParser.Models
{
	public class FailedTest
	{
		[Key]
		public long Id { get; set; }

		public string Name { get; set; }

		public string RecordId { get; set; }

		[ForeignKey("RecordId")]
		public virtual LogRecord Record { get; set; }
	}
}
