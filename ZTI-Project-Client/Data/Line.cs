using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace ZTI.Project.Client.Data {
	[Serializable, XmlType(TypeName = Constants.LINE)]
	public class Line {
		[XmlAttribute(AttributeName = Constants.NUMBER)]
		public int Number { get; set; }

		[XmlArray(ElementName = Constants.VARIANTS, IsNullable = false)]
		[XmlArrayItem(ElementName = Constants.VARIANT)]
		[DefaultValue(null)]
		public string[] Variants { get; set; }

		[XmlIgnore]
		public bool HasVariants => Variants != null && Variants.Length > 1;

		[XmlArray(ElementName = Constants.ROUTE)]
		[XmlArrayItem(ElementName = Constants.STOP)]
		public int[] Route { get; set; }

		[XmlElement(ElementName = Constants.F_PEAK)]
		public int FPeak { get; set; }

		[XmlElement(ElementName = Constants.F_NOT_PEAK)]
		public int FNotPeak { get; set; }

		[XmlElement(ElementName = Constants.FIRST)]
		public DateTime First { get; set; }

		[XmlElement(ElementName = Constants.LAST)]
		public DateTime Last { get; set; }

		public string[] ToStrings(List<Stop> allStops) {
			Stop firstStop = allStops.First(stop => stop.ID == Route[0]);
			Stop lastStop = allStops.First(stop => stop.ID == Route[Route.Length - 1]);

			if ( HasVariants ) {
				return new[] {
					$"{Number}{Variants[0]} {firstStop} <-> {lastStop}",
					$"{Number}{Variants[1]} {lastStop} <-> {firstStop}",
					$"{Number} {lastStop} <-> {firstStop}"
				};
			} else {
				return new[] {
					$"{Number} {firstStop} <-> {lastStop}",
					$"{Number} {lastStop} <-> {firstStop}"
				};
			}
		}

		public string ToString(List<Stop> allStops) {
			Stop firstStop = allStops.First(stop => stop.ID == Route[0]);
			Stop lastStop = allStops.First(stop => stop.ID == Route[Route.Length - 1]);

			return $"{Number} {firstStop} <-> {lastStop}";
		}
	}
}