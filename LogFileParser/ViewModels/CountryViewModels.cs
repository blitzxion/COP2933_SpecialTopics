using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LogFileParser.Helpers;

namespace LogFileParser.ViewModels
{
    [Obsolete("Do not use this, just use an IEnumerable", true)]
    public class CountryMetrics
    {
        public readonly IEnumerable<CountryMetricDetails> Data;
        public CountryMetricDetails TopSendingCountry
        {
            get
            {
                if (!Data.Any()) return null;
                return Data.OrderByDescending(d => d.Total).FirstOrDefault();
            }
        }
        public CountryMetricDetails LeastSendingCountry
        {
            get
            {
                if (!Data.Any()) return null;
                return Data.OrderBy(d => d.Total).FirstOrDefault();
            }
        }
        public DateTime FirstDateSeen
        {
            get
            {
                if (!Data.Any()) return DateTime.MinValue;
                return Data.Select(x => x.Data.Min(d => d.Timestamp)).FirstOrDefault();
            }
        }
        public DateTime LastDateSeen
        {
            get
            {
                if (!Data.Any()) return DateTime.MinValue;
                return Data.Select(x => x.Data.Max(d => d.Timestamp)).FirstOrDefault();
            }
        }

        public CountryMetrics(IEnumerable<CountryMetricDetails> data)
        {
            Data = data;
        }

    }

    public class CountryMetricDetails : IRecordMetrics
    {
        public string CountryCode { get; set; }
        public string Name { get; set; }
        public IEnumerable<CountryMessageDetails> Data { get; set; }

        public IEnumerable<CountryMessageDetails> FlattenData
        {
            get
            {
                if (!Data.Any()) return default(IEnumerable<CountryMessageDetails>);
                return Data.GroupBy(d => d.Timestamp)
                    .Select(d => new CountryMessageDetails()
                    {
                        Timestamp = d.Key,
                        Name = string.Join(",", d.Select(v => v.Name).Distinct()),
                        Total = d.Sum(v => v.Total)
                    }).OrderBy(d => d.Timestamp);
            }
        }

        public double Average
        {
            get
            {
                if (!FlattenData.Any()) return 0;
                return Math.Round(FlattenData.Average(d => d.Total));
            }
        }

        public int Total
        {
            get
            {
                if (!FlattenData.Any()) return 0;
                return FlattenData.Sum(x => x.Total);
            }
        }

        public double StdDev
        {
            get
            {
                if (!FlattenData.Any()) return 0;
                return FlattenData.Select(m => (double)m.Total).StdDev();
            }
        }

        public IRecordDetails MostUsed
        {
            get
            {
                if (!Data.Any()) return null;
                return Data.GroupBy(d => d.Name).Select(x => new CountryMessageDetails() { Name = x.Key, Total = x.Sum(v => v.Total) }).OrderByDescending(x => x.Total).FirstOrDefault();
            }
        }

        public IRecordDetails LeastUsed
        {
            get
            {
                if (!Data.Any()) return null;
                return Data.GroupBy(d => d.Name).Select(x => new CountryMessageDetails() { Name = x.Key, Total = x.Sum(v => v.Total) }).OrderBy(x => x.Total).FirstOrDefault();
            }
        }

        public CountryMetricDetails()
        {
            Data = new List<CountryMessageDetails>();
        }

    }

    public class CountryMessageDetails : IRecordDetails
    {
        public DateTime Timestamp { get; set; }
        public string Name { get; set; }
        public int Total { get; set; }
    }
}