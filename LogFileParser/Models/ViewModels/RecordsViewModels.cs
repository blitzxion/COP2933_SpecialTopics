using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogFileParser.Models.ViewModels
{
	public class RecordsManageViewModel
	{
		public int? TotalRecords { get; set; }
		public DateTime? LastEntryTimestamp { get; set; }
		public long? DatabaseSize { get; set; }

	}

	public class SampleGraphViewModel
	{
		public IEnumerable<MessageData> MessageData { get; set; }
		public IEnumerable<MessagePercentageData> MessagePercentageData { get; set; }
		public IEnumerable<SendingIPData> SendingIPData { get; set; }
		public IEnumerable<BaseMessageData> MessageClassData { get; set; }
		public IEnumerable<CountriesData> TopCountriesData { get; set; }

		public SampleGraphViewModel()
		{
			MessageData = new HashSet<MessageData>();
			MessagePercentageData = new HashSet<MessagePercentageData>();
			SendingIPData = new HashSet<SendingIPData>();
			MessageClassData = new HashSet<BaseMessageData>();
			TopCountriesData = new HashSet<CountriesData>();
		}

	}

	public class BaseGraphData
	{
		public int Count { get; set; }
	}

	public class BaseMessageData : BaseGraphData
	{
		public string MessageClass { get; set; }
	}

	public class MessageData : BaseMessageData
	{
		public DateTime Timestamp { get; set; }
	}

	public class MessagePercentageData : BaseMessageData
	{
		public double Percentage { get; set; }
	}

	public class SendingIPData : BaseGraphData
	{
		public string SendingIP { get; set; }
	}

	public class CountriesData : BaseGraphData
	{
		public string Country { get; set; }
	}

}