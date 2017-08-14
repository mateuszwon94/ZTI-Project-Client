using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;

namespace ZTI.Project.Client.Data {
	[Serializable, XmlType(TypeName = Constants.STOP)]
	public class Stop {
		[XmlAttribute(AttributeName = Constants.ID)]
		public int ID { get; set; }

		[XmlElement(ElementName = Constants.NAME)]
		public string Name { get; set; }

		[XmlElement(ElementName = Constants.NZ)]
		public bool NZ { get; set; }

		[XmlElement(ElementName = Constants.LOC_X)]
		public float X { get; set; }

		[XmlElement(ElementName = Constants.LOC_Y)]
		public float Y { get; set; }

		[XmlArray(ElementName = Constants.CONNS)]
		[XmlArrayItem(ElementName = Constants.CONN)]
		public int[] Conns { get; set; }

		[XmlIgnore]
		public Vector2 Location {
			get => new Vector2(X, Y);
			set {
				X = value.X;
				Y = value.Y;
			}
		}

		public override string ToString() => $"{Name}{(NZ ? " NZ" : string.Empty)}";

		public IEnumerable<Stop> ConnectedStops(IEnumerable<Stop> allStops)
			=> allStops.Where(stop => Conns.Any(id => id == stop.ID));
	}
}
