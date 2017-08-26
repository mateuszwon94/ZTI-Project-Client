using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Xml.Serialization;

namespace ZTI.Project.Client.Data {
	/// <summary>
	/// Klasa reprezentujaca przystanek
	/// </summary>
	[Serializable, XmlType(TypeName = Constants.STOP)]
	public class Stop {
		/// <summary>
		/// ID przystanku
		/// </summary>
		[XmlAttribute(AttributeName = Constants.ID)]
		public int ID { get; set; }

		/// <summary>
		/// Nazwa przystanku
		/// </summary>
		[XmlElement(ElementName = Constants.NAME)]
		public string Name { get; set; }

		/// <summary>
		/// Czy przystanek jest na zadanie
		/// </summary>
		[XmlElement(ElementName = Constants.NZ)]
		public bool NZ { get; set; }

		/// <summary>
		/// wspolrzedna X przystanku
		/// </summary>
		[XmlElement(ElementName = Constants.LOC_X)]
		public float X { get; set; }

		/// <summary>
		/// Wspolrzedna Y przystanku
		/// </summary>
		[XmlElement(ElementName = Constants.LOC_Y)]
		public float Y { get; set; }

		/// <summary>
		/// Sasiedne przystanki
		/// </summary>
		[XmlArray(ElementName = Constants.CONNS)]
		[XmlArrayItem(ElementName = Constants.CONN)]
		public int[] Conns { get; set; }

		/// <summary>
		/// Polozenie przystanku
		/// </summary>
		[XmlIgnore]
		public Vector2 Location {
			get => new Vector2(X, Y);
			set {
				X = value.X;
				Y = value.Y;
			}
		}

		/// <summary>
		/// Tekstowa reprezentacja przystanku
		/// </summary>
		/// <returns>Tekstowa reprezentacja przystanku</returns>
		public override string ToString() => $"{Name}{(NZ ? " NZ" : string.Empty)}";

		/// <summary>
		/// Funkcja zwracajaca sasiednie przystanki jako obiekty typu <see cref="Stop"/>
		/// </summary>
		/// <param name="allStops">Wszystkie przystanki na mapie</param>
		/// <returns>Lista sasiadujacych przystankow</returns>
		public IEnumerable<Stop> ConnectedStops(IEnumerable<Stop> allStops)
			=> allStops.Where(stop => Conns.Any(id => id == stop.ID));
	}
}
