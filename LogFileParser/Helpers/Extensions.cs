using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace LogFileParser
{
	public static class UrlHelperExtensions
	{
		/// <summary>
		/// Returns a full qualified action URL
		/// </summary>
		public static string QualifiedAction(this UrlHelper url, string actionName, string controllerName, object routeValues = null)
		{
			return url.Action(actionName, controllerName, routeValues, url.RequestContext.HttpContext.Request.Url.Scheme);
		}
	}

	public static class DateTimeExtensions
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

	public static class StringExtenstions
	{
		public static string Truncate(this string value, int maxLength, string overText = "...")
		{
			if (string.IsNullOrEmpty(value)) return value;
			return value.Length <= maxLength ? value : string.Format("{0}{1}", value.Substring(0, maxLength), overText);
		}

		public static string RandomString(int len = 8, Random rnd = null)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = rnd ?? new Random();
			return new string(Enumerable.Repeat(chars, len).Select(s => s[random.Next(s.Length)]).ToArray());
		}

		public static string RandomEmail(Random rnd = null)
		{
			Random r = rnd ?? new Random();
			return string.Format(@"{0}@{1}.com", RandomString(8, r), RandomString(5, r));
		}

		/// <summary>
		/// A nicer way of calling <see cref="System.String.IsNullOrEmpty(string)"/>
		/// </summary>
		/// <param name="value">The string to test.</param>
		/// <returns>true if the value parameter is null or an empty string (""); otherwise, false.</returns>
		public static bool IsNullOrEmpty(this string value)
		{
			return string.IsNullOrEmpty(value);
		}

		/// <summary>
		/// A nicer way of calling the inverse of <see cref="System.String.IsNullOrEmpty(string)"/>
		/// </summary>
		/// <param name="value">The string to test.</param>
		/// <returns>true if the value parameter is not null or an empty string (""); otherwise, false.</returns>
		public static bool IsNotNullOrEmpty(this string value)
		{
			return !value.IsNullOrEmpty();
		}

		/// <summary>
		/// A nicer way of calling <see cref="System.String.Format(string, object[])"/>
		/// </summary>
		/// <param name="format">A composite format string.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		/// <returns>A copy of format in which the format items have been replaced by the string representation of the corresponding objects in args.</returns>
		public static string FormatWith(this string format, params object[] args)
		{
			return string.Format(format, args);
		}

		/// <summary>
		/// Allows for using strings in null coalescing operations
		/// </summary>
		/// <param name="value">The string value to check</param>
		/// <returns>Null if <paramref name="value"/> is empty or the original value of <paramref name="value"/>.</returns>
		public static string NullIfEmpty(this string value)
		{
			if (value == string.Empty)
				return null;

			return value;
		}

		/// <summary>
		/// Slugifies a string
		/// </summary>
		/// <param name="value">The string value to slugify</param>
		/// <param name="maxLength">An optional maximum length of the generated slug</param>
		/// <returns>A URL safe slug representation of the input <paramref name="value"/>.</returns>
		public static string ToSlug(this string value, int? maxLength = null)
		{
			Ensure.Argument.NotNull(value, "value");

			// if it's already a valid slug, return it
			if (RegexUtils.SlugRegex.IsMatch(value))
				return value;

			return GenerateSlug(value, maxLength);
		}

		/// <summary>
		/// Converts a string into a slug that allows segments e.g. <example>.blog/2012/07/01/title</example>.
		/// Normally used to validate user entered slugs.
		/// </summary>
		/// <param name="value">The string value to slugify</param>
		/// <returns>A URL safe slug with segments.</returns>
		public static string ToSlugWithSegments(this string value)
		{
			Ensure.Argument.NotNull(value, "value");

			var segments = value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			var result = segments.Aggregate(string.Empty, (slug, segment) => slug += "/" + segment.ToSlug());
			return result.Trim('/');
		}

		/// <summary>
		/// Separates a PascalCase string
		/// </summary>
		/// <example>
		/// "ThisIsPascalCase".SeparatePascalCase(); // returns "This Is Pascal Case"
		/// </example>
		/// <param name="value">The value to split</param>
		/// <returns>The original string separated on each uppercase character.</returns>
		public static string SeparatePascalCase(this string value)
		{
			Ensure.Argument.NotNullOrEmpty(value, "value");
			return Regex.Replace(value, "([A-Z])", " $1").Trim();
		}

		/// <summary>
		/// Credit for this method goes to http://stackoverflow.com/questions/2920744/url-slugify-alrogithm-in-cs
		/// </summary>
		private static string GenerateSlug(string value, int? maxLength = null)
		{
			// prepare string, remove accents, lower case and convert hyphens to whitespace
			var result = RemoveAccent(value).Replace("-", " ").ToLowerInvariant();

			result = Regex.Replace(result, @"[^a-z0-9\s-]", string.Empty); // remove invalid characters
			result = Regex.Replace(result, @"\s+", " ").Trim(); // convert multiple spaces into one space

			if (maxLength.HasValue) // cut and trim
				result = result.Substring(0, result.Length <= maxLength ? result.Length : maxLength.Value).Trim();

			return Regex.Replace(result, @"\s", "-"); // replace all spaces with hyphens
		}

		/// <summary>
		/// Returns a string array containing the trimmed substrings in this <paramref name="value"/>
		/// that are delimited by the provided <paramref name="separators"/>.
		/// </summary>
		public static IEnumerable<string> SplitAndTrim(this string value, params char[] separators)
		{
			Ensure.Argument.NotNull(value, "source");
			return value.Trim().Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
		}

		/// <summary>
		/// Checks if the <paramref name="source"/> contains the <paramref name="input"/> based on the provided <paramref name="comparison"/> rules.
		/// </summary>
		public static bool Contains(this string source, string input, StringComparison comparison)
		{
			return source.IndexOf(input, comparison) >= 0;
		}

		/// <summary>
		/// Limits the length of the <paramref name="source"/> to the specified <paramref name="maxLength"/>.
		/// </summary>
		public static string Limit(this string source, int maxLength, string suffix = null)
		{
			if (suffix.IsNotNullOrEmpty())
			{
				maxLength = maxLength - suffix.Length;
			}

			if (source.Length <= maxLength)
			{
				return source;
			}

			return string.Concat(source.Substring(0, maxLength).Trim(), suffix ?? string.Empty);
		}

		private static string RemoveAccent(string value)
		{
			var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(value);
			return Encoding.ASCII.GetString(bytes);
		}

	}

	public static class EnumerableExtensions
	{
		public static T PickRandom<T>(this IEnumerable<T> source)
		{
			return source.PickRandom(1).Single();
		}

		public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
		{
			return source.Shuffle().Take(count);
		}

		public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int minCount, int MaxCount, Random rnd = null)
		{
			var random = rnd ?? new Random();
			return source.Shuffle().Take(rnd.Next(minCount, MaxCount));
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
		{
			return source.OrderBy(x => Guid.NewGuid());
		}
	}

	public static class PathUtils
	{
		/// <summary>
		/// Makes a filename safe for use within a URL
		/// </summary>
		public static string MakeFileNameSafeForUrls(string fileName)
		{
			var extension = Path.GetExtension(fileName);
			var safeFileName = Path.GetFileNameWithoutExtension(fileName).ToSlug();
			return Path.Combine(Path.GetDirectoryName(fileName), safeFileName + extension);
		}

		/// <summary>
		/// Combines two URL paths
		/// </summary>
		public static string CombinePaths(string path1, string path2)
		{
			if (path2.IsNullOrEmpty())
				return path1;

			if (path1.IsNullOrEmpty())
				return path2;

			if (path2.StartsWith("http://") || path2.StartsWith("https://"))
				return path2;

			var ch = path1[path1.Length - 1];

			if (ch != '/')
				return (path1.TrimEnd('/') + '/' + path2.TrimStart('/'));

			return (path1 + path2);
		}
	}

	public static class RegexUtils
	{
		/// <summary>
		/// A regular expression for validating slugs.
		/// Does not allow leading or trailing hypens or whitespace
		/// </summary>
		public static readonly Regex SlugRegex = new Regex(@"(^[a-z0-9])([a-z0-9_-]+)*([a-z0-9])$");

		/// <summary>
		/// A regular expression for validating slugs with segments
		/// Does not allow leading or trailing hypens or whitespace
		/// </summary>
		public static readonly Regex SlugWithSegmentsRegex = new Regex(@"^(?!-)[a-z0-9_-]+(?<!-)(/(?!-)[a-z0-9_-]+(?<!-))*$");

		/// <summary>
		/// A regular expression for validating IPAddresses. Taken from http://net.tutsplus.com/tutorials/other/8-regular-expressions-you-should-know/
		/// </summary>
		public static readonly Regex IPAddressRegex = new Regex(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");

		/// <summary>
		/// A regular expression for validating Email Addresses. Taken from http://net.tutsplus.com/tutorials/other/8-regular-expressions-you-should-know/
		/// </summary>
		public static readonly Regex EmailRegex = new Regex(@"^([a-z0-9_\.-]+)@([\da-z\.-]+)\.([a-z\.]{2,6})$");

		/// <summary>
		/// A regular expression for validating absolute Urls. Taken from http://net.tutsplus.com/tutorials/other/8-regular-expressions-you-should-know/
		/// </summary>
		public static readonly Regex UrlRegex = new Regex(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$");

		/// <summary>
		/// A regular expression for validating that string is a positive number GREATER THAN zero.
		/// </summary>
		public static readonly Regex PositiveNumberRegex = new Regex(@"^[1-9]+[0-9]*$");
	}

	public static class EntityExetensions
	{
		public static IQueryable<T> SelectDynamic<T>(this IQueryable<T> source, IEnumerable<string> fieldNames)
		{
			Dictionary<string, PropertyInfo> sourceProperties = fieldNames.ToDictionary(name => name, name => source.ElementType.GetProperty(name));
			Type dynamicType = LinqRuntimeTypeBuilder.GetDynamicType(sourceProperties.Values);

			ParameterExpression sourceItem = Expression.Parameter(source.ElementType, "t");
			IEnumerable<MemberBinding> bindings = dynamicType.GetFields().Select(p => Expression.Bind(p, Expression.Property(sourceItem, sourceProperties[p.Name]))).OfType<MemberBinding>();

			Expression selector = Expression.Lambda(Expression.MemberInit(Expression.New(dynamicType.GetConstructor(Type.EmptyTypes)), bindings), sourceItem);

			return source.Provider.CreateQuery<T>(Expression.Call(
				typeof(Queryable), 
				"Select", 
				new Type[] {
					source.ElementType,
					dynamicType
				}, 
				Expression.Constant(source), 
				selector
			));
		}

		public static IEnumerable<IGrouping<string, T>> GroupBy<T>(this IEnumerable<T> source, params string[] properties)
		{
			return source.GroupBy(GetGroupKey<T>(properties).Compile());
		}

		public static IQueryable<IGrouping<string, T>> GroupBy<T>(this IQueryable<T> source, params string[] properties)
		{
			return source.GroupBy(GetGroupKey<T>(properties));
		}

		public static IQueryable<IGrouping<string, T>> GroupBy<T>(this IQueryable<T> source, IEnumerable<string> properties)
		{
			return source.GroupBy(GetGroupKey<T>(properties.ToArray()));
		}

		private static Expression<Func<T, string>> GetGroupKey<T>(params string[] properties)
		{
			if (!properties.Any())
				throw new ArgumentException(
					"At least one property needs to be specified", "properties");

			var parameter = Expression.Parameter(typeof(T));
			var propertyExpressions = properties.Select(x => GetDeepPropertyExpression(parameter, x)).ToArray();

			Expression body = null;
			if (propertyExpressions.Length == 1)
				body = propertyExpressions[0];
			else
			{
				var concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string), typeof(string) });
				var separator = Expression.Constant(";");
				body = propertyExpressions.Aggregate((x, y) => Expression.Call(concatMethod, x, separator, y));
			}

			return Expression.Lambda<Func<T, string>>(body, parameter);
		}

		private static Expression GetDeepPropertyExpression( Expression initialInstance, string property)
		{
			Expression result = null;
			foreach (var propertyName in property.Split('.'))
			{
				Expression instance = result;
				if (instance == null)
					instance = initialInstance;
				result = Expression.Property(instance, propertyName);
			}
			return result;
		}

	}

	public static class LinqRuntimeTypeBuilder
	{
		//private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static AssemblyName assemblyName = new AssemblyName() { Name = "DynamicLinqTypes" };
		private static ModuleBuilder moduleBuilder = null;
		private static Dictionary<string, Type> builtTypes = new Dictionary<string, Type>();

		static LinqRuntimeTypeBuilder()
		{
			moduleBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run).DefineDynamicModule(assemblyName.Name);
		}

		private static string GetTypeKey(Dictionary<string, Type> fields)
		{
			//TODO: optimize the type caching -- if fields are simply reordered, that doesn't mean that they're actually different types, so this needs to be smarter
			string key = string.Empty;
			foreach (var field in fields)
				key += field.Key + ";" + field.Value.Name + ";";

			return key;
		}

		public static Type GetDynamicType(Dictionary<string, Type> fields)
		{
			if (null == fields)
				throw new ArgumentNullException("fields");
			if (0 == fields.Count)
				throw new ArgumentOutOfRangeException("fields", "fields must have at least 1 field definition");

			try
			{
				Monitor.Enter(builtTypes);
				string className = GetTypeKey(fields);

				if (builtTypes.ContainsKey(className))
					return builtTypes[className];

				TypeBuilder typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable);

				foreach (var field in fields)
					typeBuilder.DefineField(field.Key, field.Value, FieldAttributes.Public);

				builtTypes[className] = typeBuilder.CreateType();

				return builtTypes[className];
			}
			catch (Exception ex)
			{
				//log.Error(ex);
			}
			finally
			{
				Monitor.Exit(builtTypes);
			}

			return null;
		}


		private static string GetTypeKey(IEnumerable<PropertyInfo> fields)
		{
			return GetTypeKey(fields.ToDictionary(f => f.Name, f => f.PropertyType));
		}

		public static Type GetDynamicType(IEnumerable<PropertyInfo> fields)
		{
			return GetDynamicType(fields.ToDictionary(f => f.Name, f => f.PropertyType));
		}
	}

}