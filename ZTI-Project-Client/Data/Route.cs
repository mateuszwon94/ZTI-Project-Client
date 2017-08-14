using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ZTI.Project.Client.Data {
	public class Route : IEnumerable<Stop> {
		public Route(string xml, List<Stop> stops) {
			using ( MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			using ( XmlReader reader = XmlReader.Create(memStream) ) {
				reader.MoveToContent();
				while ( reader.Read() ) {
					if ( reader.NodeType == XmlNodeType.Element && reader.Name == Constants.STOP )
						stops_.Add(stops.First(stop => stop.ID == int.Parse(reader.GetAttribute(Constants.ID))));
				}
			}
		}

		public Stop this[int i] => stops_[i];

		public int Count => stops_.Count;

		private readonly List<Stop> stops_ = new List<Stop>();

		public IEnumerator<Stop> GetEnumerator() => stops_.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}