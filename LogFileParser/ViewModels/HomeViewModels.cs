using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogFileParser.ViewModels
{
    public class CountryLocation
    {
        public string CountryCode { get; set; }
        public double LAT { get; set; }
        public double LONG { get; set; }
        public double Magnitude { get; set; }
    }

    public class CountryLocationDetail
    {
        public double LAT { get; set; }
        public double LONG { get; set; }
        public int DMS_LAT { get; set; }
        public int DMS_LONG { get; set; }
        public string MGRS { get; set; }
        public string JOG { get; set; }
        public string AFFIL { get; set; }
        public string FIPS10 { get; set; }
        public string SHORT_NAME { get; set; }
        public string FULL_NAME { get; set; }
        public DateTime MOD_DATE { get; set; }
        public string ISO3136 { get; set; }
    }

    public class CountryComparer : IEqualityComparer<CountryLocation>
    {
        public bool Equals(CountryLocation p1, CountryLocation p2)
        {
            return p1.CountryCode == p2.CountryCode;
        }

        public int GetHashCode(CountryLocation p)
        {
            return p.CountryCode.GetHashCode();
        }
    }

}