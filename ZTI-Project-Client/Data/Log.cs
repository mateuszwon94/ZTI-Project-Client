using System;
using System.Collections.Generic;
using System.Xml;

namespace ZTI.Project.Client.Data {
	/// <summary>
	/// Klasa reprezentujaca logi serwera
	/// </summary>
	public class Log {
		/// <summary>
		/// Czas w ktorym zapisano zdarzenie
		/// </summary>
		public DateTime Time { get; set; }

		/// <summary>
		/// tekst zdarzenia
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Funkcja tworzaca liste logow na podstawie XMLa
		/// </summary>
		/// <param name="xml">XML na podstawie ktorego ma byc stworzona lista logow</param>
		/// <returns>lista logow</returns>
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