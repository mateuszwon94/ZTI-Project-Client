using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MetroLog;
using static ZTI.Project.Client.Constants;

namespace ZTI.Project.Client {
	public sealed partial class MainPage : Page {
		public MainPage() {
			InitializeComponent();
			Stops = new List<Stop>();

			WebResponse response = null;
			for ( int i = 0 ; i < 11 ; ++i ) {
				WebRequest request = WebRequest.Create("http://localhost:9081/ZTI-Project/Stops");
				try {
					response = request.GetResponseAsync().Result;
					break;
				} catch ( Exception ex )
					when ( ex is WebException ||
					       ex.InnerException is WebException ) {
					if ( i == 10 ) throw;
				}
			}

			using ( StreamReader reader = new StreamReader(response?.GetResponseStream()) ) {
				XmlSerializer deserializer = new XmlSerializer(typeof(List<Stop>),
				                                               new XmlRootAttribute(ROOT));
				Stops = (List<Stop>)deserializer.Deserialize(reader);
			}

		}

		public List<Stop> Stops;

		private void Canvas_OnDraw(CanvasControl sender, CanvasDrawEventArgs args) {
			foreach ( Stop stop in Stops ) {
				foreach ( Stop conectedStop in stop.ConnectedStops(Stops)) {
					args.DrawingSession.DrawLine(stop.X * 5f, stop.Y * 5f,
					                             conectedStop.X * 5f, conectedStop.Y * 5f,
												 Colors.Black);
				}
				
				args.DrawingSession.FillCircle(stop.X * 5f, stop.Y * 5f, 3f,
				                               stop.NZ ? Colors.DimGray : Colors.Black);
				args.DrawingSession.DrawText(stop.Name,
				                             stop.X * 5f - 50f, stop.Y * 5f + 5f,
				                             stop.NZ ? Colors.DimGray : Colors.Black,
				                             new CanvasTextFormat {
					                             FontSize = 15,
					                             FontFamily = "Times New Roman"
				                             });
			}
		}
	}
}