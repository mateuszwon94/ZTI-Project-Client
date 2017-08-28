using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Windows.UI.ViewManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using ZTI.Project.Client.Data;
using System.Diagnostics;

namespace ZTI.Project.Client.Test {
	[TestClass]
	public class XmlDeserialization {
		[TestMethod]
		public void TestLogDeserialization() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
			sb.AppendLine("<logs xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"");
			sb.AppendLine("\txmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");

			List<Log> logs = new List<Log>();
			DateTime time = DateTime.Now.ToUniversalTime();
			time = time.AddTicks(-time.Ticks % TimeSpan.TicksPerSecond);
			for ( int i = 0; i < 10 ; ++i ) {
				string message = "Message";
				logs.Add(new Log {
					Time = time,
					Message = message
				});

				sb.AppendLine($"\t<log time=\"{time.ToString("s").Replace("T", " ")}\">");
				sb.AppendLine($"\t\t{message}");
				sb.AppendLine("\t</log>");

				time = time.AddMinutes(10);
			}

			sb.AppendLine("</logs>");

			ICollection generatedLogs = Log.CreateFromXml(sb.ToString());
			CollectionAssert.AreEqual(logs, generatedLogs);
		}

		[TestMethod]
		public void TestScheduleDeserialization() {
			List<Line> lines = new List<Line> {
				new Line {
					Number = 4,
					Route = new int[10]
				}
			};

			List<Stop> stops = new List<Stop>();
			for ( int i = 1 ; i < 11 ; ++i ) {
				stops.Add(new Stop {
					ID = i
				});

				lines[0].Route[i - 1] = i;
			}

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
			sb.AppendLine("<schedules xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"");
			sb.AppendLine("\txmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");

			List<Schedule> schedules = new List<Schedule>();
			
			for ( int stop = 1 ; stop < 11 ; ++stop ) {
				Schedule newSchedule = new Schedule() {
					Stop = stops[stop - 1],
					Line = lines[0],
					Times = new Dictionary<string, List<TimeSpan>>()
				};

				sb.AppendLine($"\t<schedule stop=\"{stop}\" line=\"4\" >");
				foreach ( string variant in new[] {"0", "1"} ) {
					newSchedule.Times[variant] = new List<TimeSpan>();

					TimeSpan time = TimeSpan.Parse("10:00");
					sb.AppendLine($"\t\t<times variant=\"{variant}\" >");
					for ( int i = 0 ; i < 10 ; ++i ) {
						newSchedule.Times[variant].Add(time);

						sb.AppendLine("\t\t\t<time>");
						sb.AppendLine($"\t\t\t\t{time:hh\\:mm}");
						sb.AppendLine("\t\t\t</time>");

						time = time.Add(TimeSpan.FromMinutes(10));
					}

					sb.AppendLine("\t</times>");
				}
				sb.AppendLine("\t</schedule>");

				schedules.Add(newSchedule);
			}
			sb.AppendLine("</schedules>");

			ICollection generatedSchedules = Schedule.CreateFromXml(sb.ToString(), stops, lines);
			CollectionAssert.AreEqual(schedules, generatedSchedules);
		}

		/// <summary>
		///  Gets or sets the test context which provides
		///  information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }
	}
}
