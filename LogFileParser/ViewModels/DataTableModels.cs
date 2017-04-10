using LogFileParser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogFileParser.ViewModels
{
	public class DataTableDateFilteredRequest : DataTableFilterRequest
	{
		public DateTime? fromDate { get; set; }
		public DateTime? toDate { get; set; }
	}



	public class DataTableFilterRequest
	{
		public int draw { get; set; }
		public int start { get; set; }
		public int length { get; set; }
		public DataTableSearch search { get; set; }
		public IList<DataTableOrder> order { get; set; } = new List<DataTableOrder>();
		public IList<DataTableColumn> columns { get; set; } = new List<DataTableColumn>();
	}

	public class DataTableSearch
	{
		public string value { get; set; }
		public string regex { get; set; }
	}

	public class DataTableOrder
	{
		public int column { get; set; }
		public string dir { get; set; }
	}

	public class DataTableColumn
	{
		public string data { get; set; }
		public string name { get; set; }
		public bool searchable { get; set; }
		public bool orderable { get; set; }
		public DataTableSearch search { get; set; } = new DataTableSearch();
	}

	public class DataTableFilterResponse<TData>
	{
		public int draw { get; set; }
		public int recordsTotal { get; set; }
		public int recordsFiltered { get; set; }
		public IList<TData> data { get; set; } = new List<TData>();
		public string error { get; set; }

	}

	public class DataTableFilterResponseExtra<TData> : DataTableFilterResponse<TData>
	{
		public string DT_RowId { get; set; }
		public string DT_RowClass { get; set; }
		public Dictionary<string, string> DT_RowData { get; set; } = new Dictionary<string, string>();
		public Dictionary<string, string> DT_RowAttr { get; set; } = new Dictionary<string, string>();
	}

}