using System.Web;
using System.Web.Optimization;

namespace LogFileParser
{
	public class BundleConfig
	{
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
						"~/Scripts/jquery-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
						"~/Scripts/jquery.validate*"));

			// Use the development version of Modernizr to develop with and learn from. Then, when you're
			// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
			bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
						"~/Scripts/modernizr-*"));

			bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
				"~/Scripts/bootstrap.js",
				"~/Scripts/respond.js"));

			// Inspinia script
			bundles.Add(new ScriptBundle("~/bundles/inspinia").Include(
				"~/Scripts/app/inspinia.js"));

			// SlimScroll
			bundles.Add(new ScriptBundle("~/plugins/slimScroll").Include(
				"~/Scripts/plugins/slimScroll/jquery.slimscroll.min.js"));

			// jQuery plugins
			bundles.Add(new ScriptBundle("~/plugins/metsiMenu").Include(
				"~/Scripts/plugins/metisMenu/metisMenu.min.js"));

			bundles.Add(new ScriptBundle("~/plugins/pace").Include(
				"~/Scripts/plugins/pace/pace.min.js"));

			// jasnyBootstrap styles
			bundles.Add(new StyleBundle("~/plugins/jasnyBootstrapStyles").Include(
				"~/Content/plugins/jasny/jasny-bootstrap.min.css"));

			// jasnyBootstrap 
			bundles.Add(new ScriptBundle("~/plugins/jasnyBootstrap").Include(
				"~/Scripts/plugins/jasny/jasny-bootstrap.min.js"));

			// ChartJS chart
			bundles.Add(new ScriptBundle("~/plugins/chartJs").Include(
				"~/Scripts/plugins/chartjs/Chart.min.js"));

			// CanvasJS chart
			bundles.Add(new ScriptBundle("~/plugins/canvasjs").Include(
				"~/Scripts/plugins/canvasjs/canvasjs.min.js"));

			// CanvasJS jQuery chart
			bundles.Add(new ScriptBundle("~/plugins/jqcanvasjs").Include(
				"~/Scripts/plugins/canvasjs/jquery.canvasjs.min.js"));

			// dataTables css styles
			bundles.Add(new StyleBundle("~/Content/plugins/dataTables").Include(
				"~/Content/plugins/dataTables/datatables.min.css"));

			// dataTables 
			bundles.Add(new ScriptBundle("~/plugins/dataTables").Include(
				"~/Scripts/plugins/dataTables/datatables.min.js",
				"~/Scripts/plugins/dataTables/datetime.js"));

			// jQuery Builder JS and CSS
			bundles.Add(new ScriptBundle("~/plugins/queryBuilder").Include(
				"~/Scripts/sql-parser.js",
				"~/Scripts/plugins/jQueryBuilder/query-builder.standalone.js"));

			bundles.Add(new StyleBundle("~/Content/plugins/queryBuilder").Include(
				"~/Content/plugins/jQueryBuilder/query-builder.default.min.css"));

			// MomentJS
			bundles.Add(new ScriptBundle("~/bundles/momentjs").Include(
				"~/Scripts/moment.min.js"));

			// Bootstrap DateTimePicker
			bundles.Add(new ScriptBundle("~/bundles/bs-datetimepicker").Include(
				"~/Scripts/moment.min.js",
				"~/Scripts/bootstrap-datetimepicker.min.js"));

			bundles.Add(new StyleBundle("~/Content/bs-datetimepicker").Include(
				"~/Content/bootstrap-datetimepicker.min.css"));

			// JQuery Sortable
			bundles.Add(new ScriptBundle("~/bundles/jquery-sortable").Include(
				"~/Scripts/jquery-sortable.js"));

			// D3
			bundles.Add(new ScriptBundle("~/bundles/d3").Include("~/Scripts/d3/d3.js"));

			// D3 v3
			bundles.Add(new ScriptBundle("~/bundles/d3v3").Include("~/Scripts/d3v3/d3.js"));

			// Planetary.js
			bundles.Add(new ScriptBundle("~/bundles/planetary").Include(
				"~/Scripts/planetaryjs.js"));

			// jVectorMaps
			bundles.Add(new ScriptBundle("~/bundles/jvectormaps").Include(
				"~/Scripts/jquery-jvectormap.min.js",
				"~/Scripts/jVectorMaps/world-ISO-A3/jquery-jvectormap-world-mill-en-US.js"));

			bundles.Add(new StyleBundle("~/Content/jvectormaps").Include(
				"~/Content/jquery-jvectormap.css"));

			// Cal Heatmap
			bundles.Add(new ScriptBundle("~/bundles/plugins/calHeatmap").Include(
				"~/Scripts/plugins/calHeatmap/cal-heatmap.js"));

			bundles.Add(new StyleBundle("~/Content/plugins/calHeatmap").Include(
				"~/Content/plugins/calHeatmap/cal-heatmap.css"));

			// CSS style (bootstrap/inspinia)
			bundles.Add(new StyleBundle("~/Content/css").Include(
				"~/Content/bootstrap.min.css",
				"~/Content/animate.css",
				"~/Content/spinner.css",
				"~/Content/style.css",
				"~/Content/site.css"));

			// Font Awesome icons
			bundles.Add(new StyleBundle("~/font-awesome/css").Include(
				"~/fonts/font-awesome/css/font-awesome.min.css", new CssRewriteUrlTransform()));

			bundles.Add(new ScriptBundle("~/bundles/utils").Include(
				"~/Scripts/linq.js",
				"~/Scripts/color-hash.js",
				"~/Scripts/utils.js"));

		}
	}
}
