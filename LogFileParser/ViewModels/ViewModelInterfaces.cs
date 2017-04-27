using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogFileParser.ViewModels
{
    public interface IRecordDetails
    {
        DateTime Timestamp { get; set; }
        string Name { get; set; }
        int Total { get; set; }
    }

    public interface IRecordMetrics
    {
        string Name { get; set; }

        double Average { get; }
        int Total { get; }
        double StdDev { get; }
        IRecordDetails MostUsed { get; }
        IRecordDetails LeastUsed { get; }
    }

}
