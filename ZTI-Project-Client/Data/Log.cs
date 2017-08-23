using System;
using System.Collections.Generic;
using System.Xml;

namespace ZTI.Project.Client.Data {
	public class Log {
		public DateTime Time { get; set; }

		public string Message { get; set; }

		public static IEnumerable<Log> CreateFromXml(string xml) {
			List<Log> logs = new List<Log>();

			XmlDocument document = new XmlDocument();
			document.LoadXml(xml);

			foreach ( XmlNode logNode in document.DocumentElement.ChildNodes ) {
				logs.Add(new Log() {
					Time = DateTime.Parse(logNode.Attributes[Constants.TIME].Value),
					Message = logNode.InnerText
				});
			}

			return logs;
		}
	}
}