using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogFileParser.ViewModels
{
    public class RecordDetails : IRecordDetails
    {
        public DateTime Timestamp { get; set; }
        public string Name { get; set; }
        public int Total { get; set; }
    }

    public class RecordMetrics : IRecordMetrics
    {
        public string Name { get; set; }

        public readonly IEnumerable<IRecordDetails> Data;
        public readonly IEnumerable<IRecordDetails> FlattenData;

        public double Average { get { return Math.Round(FlattenData.Average(x => x.Total)); } }
        public int Total { get { return FlattenData.Sum(x => x.Total); } }
        public double StdDev { get { return FlattenData.Select(x => (double)x.Total).StdDev(); } }
        public IRecordDetails MostUsed { get { return Data.GroupBy(x => x.Name).Select(x => new RecordDetails() { Name = x.Key, Total = x.Sum(v => v.Total) }).OrderByDescending(x => x.Total).FirstOrDefault(); } }
        public IRecordDetails LeastUsed { get { return Data.GroupBy(x => x.Name).Select(x => new RecordDetails() { Name = x.Key, Total = x.Sum(v => v.Total) }).OrderBy(x => x.Total).FirstOrDefault(); } }
        public RecordMetrics(IEnumerable<IRecordDetails> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Data = data;

            FlattenData = Data.GroupBy(x => x.Timestamp)
                .Select(x => new RecordDetails()
                {
                    Timestamp = x.Key,
                    Name = string.Join(",", x.Select(y => y.Name).Distinct()),
                    Total = x.Sum(y => y.Total)
                }).OrderBy(x => x.Timestamp);

        }
    }
}