using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LogFileParser.Helpers;

namespace LogFileParser.ViewModels
{
    public class CountryMetrics
    {
        public readonly IEnumerable<CountryMetricDetails> Data;
        public CountryMetricDetails TopSendingCountry
        {
            get
            {
                if (!Data.Any()) return null;
                return Data.OrderByDescending(d => d.TotalMessagesSent).FirstOrDefault();
            }
        }
        public CountryMetricDetails LeastSendingCountry
        {
            get
            {
                if (!Data.Any()) return null;
                return Data.OrderBy(d => d.TotalMessagesSent).FirstOrDefault();
            }
        }
        public DateTime FirstDateSeen
        {
            get
            {
                if (!Data.Any()) return DateTime.MinValue;
                return Data.Select(x => x.Data.Min(d => d.Date.Value)).FirstOrDefault();
            }
        }
        public DateTime LastDateSeen
        {
            get
            {
                if (!Data.Any()) return DateTime.MinValue;
                return Data.Select(x => x.Data.Max(d => d.Date.Value)).FirstOrDefault();
            }
        }

        public CountryMetrics(IEnumerable<CountryMetricDetails> data)
        {
            Data = data;
        }

    }

    public class CountryMetricDetails
    {
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public IEnumerable<CountryMessageDetails> Data { get; set; }

        public IEnumerable<CountryMessageDetails> FlattenData
        {
            get
            {
                if (!Data.Any()) return default(IEnumerable<CountryMessageDetails>);
                return Data.GroupBy(d => d.Date)
                    .Select(d => new CountryMessageDetails()
                    {
                        Date = d.Key,
                        MessageClass = string.Join(",", d.Select(v => v.MessageClass).Distinct()),
                        Total = d.Sum(v => v.Total)
                    }).OrderBy(d => d.Date);
            }
        }

        public double AverageMessagesSent
        {
            get
            {
                if (!FlattenData.Any()) return 0;
                return Math.Round(FlattenData.Average(d => d.Total));
            }
        }

        public int TotalMessagesSent
        {
            get
            {
                if (!FlattenData.Any()) return 0;
                return FlattenData.Sum(x => x.Total);
            }
        }

        public double MessageStdDev
        {
            get
            {
                if (!FlattenData.Any()) return 0;
                return FlattenData.Select(m => (double)m.Total).StdDev();
            }
        }

        public CountryMessageDetails MostSentMessage
        {
            get
            {
                if (!Data.Any()) return null;
                return Data.GroupBy(d => d.MessageClass).Select(x => new CountryMessageDetails() { MessageClass = x.Key, Total = x.Sum(v => v.Total) }).OrderByDescending(x => x.Total).FirstOrDefault();
            }
        }

        public CountryMessageDetails LeastSentMessage
        {
            get
            {
                if (!Data.Any()) return null;
                return Data.GroupBy(d => d.MessageClass).Select(x => new CountryMessageDetails() { MessageClass = x.Key, Total = x.Sum(v => v.Total) }).OrderBy(x => x.Total).FirstOrDefault();
            }
        }

        public CountryMetricDetails()
        {
            Data = new List<CountryMessageDetails>();
        }

    }

    public class CountryMessageDetails
    {
        public DateTime? Date { get; set; }
        public string MessageClass { get; set; }
        public int Total { get; set; }
    }
}