using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using static ZTI.Project.Client.Constants;

namespace ZTI.Project.Client.Data {
	public class Schedule {
		public Stop Stop { get; set; }
		
		public Line Line { get; set; }

		public Stop LineFirstStop { get; private set; }

		public Stop LineLastStop { get; private set; }

		public Dictionary<string, List<TimeSpan>> Times { get; set; }

		public override bool Equals(object obj) {
			if ( obj == null ) return false;
			if ( ReferenceEquals(obj, this) ) return true;
			if ( obj is Schedule other ) {
				if ( Stop != other.Stop ||
				     Line != other.Line ) return false;

				foreach ( string key in Times.Keys ) {
					if ( !other.Times.Keys.Contains(key) ) return false;
					if ( Times[key].Count != other.Times[key].Count ) return false;


					for ( int i = 0 ; i < Times[key].Count; ++i ) 
						if ( Times[key][i] != other.Times[key][i] ) return false;
				}

				return true;
			}

			return false;
		}

		public string HeaderText => $"{Stop.Name} - {Line.Number}";

		public string TimesText1 {
			get {
				StringBuilder sb = new StringBuilder();

				string variant = Line.Variants != null ? Line.Variants[0] : "0";

				int v;
				sb.Append(int.TryParse(variant, out v)
					          ? $"{Stop.Name} - {Line.Number} -> "
					          : $"{Stop.Name} - {Line.Number}{variant}");

				if ( int.TryParse(variant, out v) )
					sb.Append(v == 0 ? LineLastStop.Name : LineFirstStop.Name);

				TimeSpan lastTime = TimeSpan.MinValue;
				int i = 0;
				foreach ( TimeSpan currentTime in Times[variant] ) {
					if ( currentTime.Hours > lastTime.Hours ) {
						sb.Append($"\n  {currentTime.Hours}:\t");
						i = 0;
					} else if ( i == 5 ) {
						sb.Append("\n\t");
						i = 0;
					}

					sb.Append($"{currentTime.Minutes} ");
					lastTime = currentTime;
					++i;
				}

				return sb.ToString();
			}
		}

		public string TimesText2 {
			get {
				StringBuilder sb = new StringBuilder();

				string variant = Line.Variants != null ? Line.Variants[1] : "1";

				int v;
				sb.Append(int.TryParse(variant, out v)
					          ? $"{Stop.Name} - {Line.Number} -> "
					          : $"{Stop.Name} - {Line.Number}{variant}");

				if ( int.TryParse(variant, out v) )
					sb.Append(v == 0 ? LineLastStop.Name : LineFirstStop.Name);

				TimeSpan lastTime = TimeSpan.MinValue;
				int i = 0;
				foreach ( TimeSpan currentTime in Times[variant] ) {
					if ( currentTime.Hours > lastTime.Hours ) {
						sb.Append($"\n  {currentTime.Hours}:\t");
						i = 0;
					} else if ( i == 5 ) {
						sb.Append("\n\t");
						i = 0;
					}

					sb.Append($"{currentTime.Minutes} ");
					lastTime = currentTime;
					++i;
				}

				return sb.ToString();
			}
		}

		public static List<Schedule> CreateFromXml(string xml, List<Stop> allStops, List<Line> allLines) {
			List<Schedule> schedules = new List<Schedule>();

			XmlDocument document = new XmlDocument();
			document.LoadXml(xml);

			foreach ( XmlNode scheduleNode in document.DocumentElement.ChildNodes ) {
				Schedule schedule = new Schedule {
					Stop = allStops.First(s => s.ID == int.Parse(scheduleNode.Attributes[STOP].Value)),
					Line = allLines.First(l => l.Number == int.Parse(scheduleNode.Attributes[LINE].Value)),
					Times = new Dictionary<string, List<TimeSpan>>()
				};
				schedule.LineFirstStop = allStops.First(s => s.ID == schedule.Line.Route[0]);
				schedule.LineLastStop = allStops.First(s => s.ID == schedule.Line.Route[schedule.Line.Route.Length - 1]);

				foreach ( XmlNode timesNode in scheduleNode.ChildNodes ) {
					schedule.Times[timesNode.Attributes[VARIANT].Value] = new List<TimeSpan>();

					foreach ( XmlNode timeNode in timesNode.ChildNodes ) 
						schedule.Times[timesNode.Attributes[VARIANT].Value].Add(TimeSpan.Parse(timeNode.InnerText.Trim()));
				}

				schedules.Add(schedule);
			}

			return schedules;
		}
	}
}
