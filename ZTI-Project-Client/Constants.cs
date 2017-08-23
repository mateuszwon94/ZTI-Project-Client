using System.Net.Http;

namespace ZTI.Project.Client {
	public static class Constants {
		public const string STOPS = "stops";
		public const string STOP = "stop";
		public const string ID = "id";
		public const string NAME = "name";
		public const string NZ = "nz";
		public const string LOC_X = "loc_x";
		public const string LOC_Y = "loc_y";
		public const string CONNS = "conns";
		public const string CONN = "conn";
		public const string TIMES = "times";
		public const string TIME = "time";
		public const string LOGS = "logs";
		public const string LOG = "log";
		public const string LINES = "lines";
		public const string LINE = "line";
		public const string NUMBER = "number";
		public const string VARIANTS = "variants";
		public const string VARIANT = "variant";
		public const string ROUTE = "route";
		public const string ROUTES = "routes";
		public const string F_PEAK = "f_peak";
		public const string F_NOT_PEAK = "f_not_peak";
		public const string FIRST = "first";
		public const string LAST = "last";
		public const string SCHEDULES = "schedules";
		public const string SCHEDULE = "schedule";
		public const string CHANGE = "change";
		public const string FROM = "from";
		public const string TO = "to";

		public static class Http {
			public static readonly HttpClient Client = new HttpClient();
		}

		public static class Url {
			public const string APP = "http://zti-project-server.eu-gb.mybluemix.net/";
			public const string STOPS = "Stops";
			public const string LINES = "Lines";
			public const string SEARCH_ROUTE = "SearchRoute";
			public const string SCHEDULES = "Schedules";
			public const string LOG = "Log";
		}
	}
}