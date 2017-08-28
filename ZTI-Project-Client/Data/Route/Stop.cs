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

		public override bool Equals(object obj) {
			if ( obj == null ) return false;
			if ( ReferenceEquals(obj, this) ) return true;
			if ( obj is Stop other ) return ID == other.ID && Line == other.Line && Time == other.Time;
			return false;
		}

		public static bool operator ==(Stop one, Stop two) {
			if ( one is null || two is null ) return false;
			return one.Equals(two);
		}

		public static bool operator !=(Stop one, Stop two) {
			if ( one is null || two is null ) return true;
			return !one.Equals(two);
		}
	}
}

