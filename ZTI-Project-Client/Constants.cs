namespace ZTI.Project.Client {
	public static class Constants {
		public const string ROOT  = "stops";
		public const string STOP  = "stop";
		public const string ID    = "id";
		public const string NAME  = "name";
		public const string NZ    = "nz";
		public const string LOC_X = "loc_x";
		public const string LOC_Y = "loc_y";
		public const string CONNS = "conns";
		public const string CONN  = "conn";

		public static class Url {
#if DEBUG
			public const string APP = "http://localhost:9081/ZTI-Project";
#else
			public const string APP = "http://IBM-Bluemix-site/ZTI-Project";
#endif
			public const string STOPS = "/Stops";
		}
	}
}