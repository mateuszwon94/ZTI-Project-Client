using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
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
 	}
}