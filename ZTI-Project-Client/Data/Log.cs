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

		public override string ToString() 
			=> $"{Time.ToString("s").Replace("T", " ")} - {Message}";

		/// <summary>
		/// Funkcja tworzaca liste logow na podstawie XMLa
		/// </summary>
		/// <param name="xml">XML na podstawie ktorego ma byc stworzona lista logow</param>
		/// <returns>lista logow</returns>
		public static List<Log> CreateFromXml(string xml) {
			List<Log> logs = new List<Log>();

			XmlDocument document = new XmlDocument();
			document.LoadXml(xml);

			foreach ( XmlNode logNode in document.DocumentElement.ChildNodes ) {
				logs.Add(new Log {
					Time = DateTime.Parse(logNode.Attributes[Constants.TIME].Value),
					Message = logNode.InnerText.Trim()
				});
			}

			return logs;
		}

		/// <inheritdoc />
		public override bool Equals(object obj) {
			if ( obj == null ) return false;
			if ( ReferenceEquals(this, obj) ) return true;
			if ( obj is Log otherLog ) return Time == otherLog.Time && Message == otherLog.Message;
			return false;
		}

		public static bool operator ==(Log one, Log two) {
			if ( one is null || two is null ) return false;
			return one.Equals(two);
		}

		public static bool operator !=(Log one, Log two) {
			if ( one is null || two is null ) return true;
			return !one.Equals(two);
		}
	}
}