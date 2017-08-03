using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using static ZTI_Project_Client.Constants;

namespace ZTI_Project_Client {
	[Serializable, XmlType(TypeName = STOP)]
	public class Stop {

		[XmlAttribute(AttributeName = Constants.ID)]
		public int ID { get; set; }

		[XmlElement(ElementName = NAME)]
		public string Name { get; set; }

		[XmlElement(ElementName = Constants.NZ)]
		public bool NZ { get; set; }

		[XmlElement(ElementName = LOC_X)]
		public float X { get; set; }

		[XmlElement(ElementName = LOC_Y)]
		public float Y { get; set; }

		[XmlIgnore]
		public Vector2 Location {
			get => new Vector2(X, Y);
			set {
				X = value.X;
				Y = value.Y;
			}
		}
	}
}
