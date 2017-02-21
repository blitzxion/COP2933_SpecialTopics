using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogFileParser.Tests
{
	public static class Log
	{
		public static void Debug(object obj)
		{
			System.Diagnostics.Debug.WriteLine(obj);
		}

		public static void Debug(string format, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine(format, args);
		}
	}
}
