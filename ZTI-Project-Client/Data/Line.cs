using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace ZTI.Project.Client.Data {
	/// <summary>
	/// Klasa przechowujaca informacje na temat linii
	/// </summary>
	[Serializable, XmlType(TypeName = Constants.LINE)]
	public class Line {
		/// <summary>
		/// Numer linii
		/// </summary>
		[XmlAttribute(AttributeName = Constants.NUMBER)]
		public int Number { get; set; }

		/// <summary>
		/// Nazwy wariantow linii
		/// </summary>
		[XmlArray(ElementName = Constants.VARIANTS, IsNullable = false)]
		[XmlArrayItem(ElementName = Constants.VARIANT)]
		[DefaultValue(null)]
		public string[] Variants { get; set; }

		/// <summary>
		/// Czy posiada nazwy wariantow linii
		/// </summary>
		[XmlIgnore]
		public bool HasVariants => Variants != null && Variants.Length > 1;

		/// <summary>
		/// Trasa linii
		/// </summary>
		[XmlArray(ElementName = Constants.ROUTE)]
		[XmlArrayItem(ElementName = Constants.STOP)]
		public int[] Route { get; set; }

		/// <summary>
		/// Czestotliwosc w szczycie
		/// </summary>
		[XmlElement(ElementName = Constants.F_PEAK)]
		public int FPeak { get; set; }

		/// <summary>
		/// Czyestotliwosc poza szczytem
		/// </summary>
		[XmlElement(ElementName = Constants.F_NOT_PEAK)]
		public int FNotPeak { get; set; }

		/// <summary>
		/// Pierwszy odjazd
		/// </summary>
		[XmlElement(ElementName = Constants.FIRST)]
		public DateTime First { get; set; }

		/// <summary>
		/// Ostatni odjazd
		/// </summary>
		[XmlElement(ElementName = Constants.LAST)]
		public DateTime Last { get; set; }

		/// <summary>
		/// Funkcja zwracajaca tekstowa reprezentacje wariantow linii
		/// </summary>
		/// <param name="allStops">Wszystkie przystanku w miescie</param>
		/// <returns>Tablica reprezentacji tekstowej wariantow linii</returns>
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

		/// <summary>
		/// Funkcja zwracajaca tekstowa reprezetacje linii
		/// </summary>
		/// <param name="allStops">Wszystkie przystanki w miescie</param>
		/// <returns>Tekstowa reprezentacja linii</returns>
		public string ToString(List<Stop> allStops) {
			Stop firstStop = allStops.First(stop => stop.ID == Route[0]);
			Stop lastStop = allStops.First(stop => stop.ID == Route[Route.Length - 1]);

			return $"{Number} {firstStop} <-> {lastStop}";
		}
	}
}