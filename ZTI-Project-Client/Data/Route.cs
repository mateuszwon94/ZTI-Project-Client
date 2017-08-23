using System;
using System.Linq;
using System.Xml.Serialization;

namespace ZTI.Project.Client.Data.Route {
	[Serializable, XmlType(TypeName = Constants.STOP)]
	public class Stop {
		[XmlAttribute(AttributeName = Constants.ID)]
		public int ID { get; set; }

		[XmlElement(ElementName = Constants.LINE)]
		public int Line { get; set; }

		[XmlElement(ElementName = Constants.TIME)]
		public string TimeString {
			get => $"{Time.Hours}:{Time.Minutes}";
			set => Time = TimeSpan.Parse(value);
		}

		[XmlIgnore]
		public TimeSpan Time { get; set; }

		public string String =>
			$"Linia {Line} - "+
			$"odjazd o {TimeString} " +
			$"z przystanku {RouteSearchPage.Stops.First(s => s.ID == ID).Name} ";

	}
}

