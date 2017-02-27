using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogFileParser.Metrics
{
	internal static class DateTimeExtensions
	{
		private static readonly long DatetimeMinTimeTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;

		public static long ToJavaScriptMilliseconds(this DateTime dt)
		{
			return ((dt.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 10000);
		}

		public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
		{
			int diff = dt.DayOfWeek - startOfWeek;
			if (diff < 0)
				diff += 7;

			return dt.AddDays(-1 * diff).Date;
		}

		public static DateTime FirstDayOfMonth(this DateTime value)
		{
			return new DateTime(value.Year, value.Month, 1);
		}

		public static int DaysInMonth(this DateTime value)
		{
			return DateTime.DaysInMonth(value.Year, value.Month);
		}

		public static DateTime LastDayOfMonth(this DateTime value)
		{
			return new DateTime(value.Year, value.Month, value.DaysInMonth());
		}

		public static DateTime RandomDate(DateTime startDate, DateTime endDate, Random rnd = null)
		{
			TimeSpan timeSpan = endDate - startDate;
			var randomTest = rnd ?? new Random();
			TimeSpan newSpan = new TimeSpan(0, randomTest.Next(0, (int)timeSpan.TotalMinutes), 0);
			DateTime newDate = startDate + newSpan;
			return newDate;
		}

		public static DateTime RoundUp(this DateTime dt, TimeSpan d)
		{
			var modTicks = dt.Ticks % d.Ticks;
			var delta = modTicks != 0 ? d.Ticks - modTicks : 0;
			return new DateTime(dt.Ticks + delta, dt.Kind);
		}

		public static DateTime RoundDown(this DateTime dt, TimeSpan d)
		{
			var delta = dt.Ticks % d.Ticks;
			return new DateTime(dt.Ticks - delta, dt.Kind);
		}

		public static DateTime RoundToNearest(this DateTime dt, TimeSpan d)
		{
			var delta = dt.Ticks % d.Ticks;
			bool roundUp = delta > d.Ticks / 2;
			var offset = roundUp ? d.Ticks : 0;

			return new DateTime(dt.Ticks + offset - delta, dt.Kind);
		}

		/// <summary>
		/// Yields every minute between start and end range.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<DateTime> MinutesBetween(this DateTime start, DateTime? end, MinutesBetweenMode mode = MinutesBetweenMode.Inclusive)
		{
			if (end == start || start > end) return Enumerable.Empty<DateTime>();

			int adjustment = 1;
			if (mode == MinutesBetweenMode.Inclusive) adjustment = 1;
			if (mode == MinutesBetweenMode.EndExclusive) adjustment = 0;
			if (mode == MinutesBetweenMode.StartExclusive) start = start.AddMinutes(1);
			if (mode == MinutesBetweenMode.StartAndEndExclusive)
			{
				adjustment = 0;
				start = start.AddMinutes(1);
			}

			if (end == start || start > end) return Enumerable.Empty<DateTime>();
			return Enumerable.Range(0, (int)((end - start)?.TotalMinutes) + adjustment).Select(d => start.AddMinutes(d));
		}

		/// <summary>
		/// Yields every minute between start and end range.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<DateTime?> MinutesBetween(this DateTime? start, DateTime? end, MinutesBetweenMode mode = MinutesBetweenMode.Inclusive)
		{
			if (end == start || start > end) return Enumerable.Empty<DateTime?>();

			int adjustment = 1;
			if (mode == MinutesBetweenMode.Inclusive) adjustment = 1;
			if (mode == MinutesBetweenMode.EndExclusive) adjustment = 0;
			if (mode == MinutesBetweenMode.StartExclusive) start = start?.AddMinutes(1);
			if (mode == MinutesBetweenMode.StartAndEndExclusive)
			{
				adjustment = 0;
				start = start?.AddMinutes(1);
			}

			if (end == start || start > end) return Enumerable.Empty<DateTime?>();
			return Enumerable.Range(0, (int)((end - start).GetValueOrDefault().TotalMinutes) + adjustment).Select(d => start?.AddMinutes(d));
		}

		public enum MinutesBetweenMode
		{
			Inclusive = 0,
			StartExclusive,
			EndExclusive,
			StartAndEndExclusive
		}

	}
}
