using System;
using System.Linq;
using System.Xml.Serialization;

namespace ZTI.Project.Client.Data.Route {
	/// <summary>
	/// Klasa reprezentujaca przystanek na trasie w rozkladzie jazdy
	/// </summary>
	[Serializable, XmlType(TypeName = Constants.STOP)]
	public class Stop {
		/// <summary>
		/// ID przystanku
		/// </summary>
		[XmlAttribute(AttributeName = Constants.ID)]
		public int ID { get; set; }

		/// <summary>
		/// Linia jaka z niego odjechano
		/// </summary>
		[XmlElement(ElementName = Constants.LINE)]
		public int Line { get; set; }

		/// <summary>
		/// Pomocnicza wlasciwosc odpowiadajaca tekstowej reprezentacji czasu
		/// </summary>
		[XmlElement(ElementName = Constants.TIME)]
		public string TimeString {
			get => $"{Time.Hours}:{Time.Minutes}";
			set => Time = TimeSpan.Parse(value);
		}

		/// <summary>
		/// Czas odjazdu z przystanku
		/// </summary>
		[XmlIgnore]
		public TimeSpan Time { get; set; }

		/// <summary>
		/// Tekstowa reprezentacja przystanku
		/// </summary>
		public string String =>
			$"Linia {Line} - "+
			$"odjazd o {TimeString} " +
			$"z przystanku {RouteSearchPage.Stops.First(s => s.ID == ID).Name} ";

	}
}

