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
	public sealed partial class StopsPage : Page {
		public StopsPage() {
			InitializeComponent();
			GetStopsFromServer("http://localhost:9081/ZTI-Project/Stops");
			
			LoadingIndicator.Visibility = Visibility.Collapsed;
			MapCanvas.Visibility = Visibility.Visible;
		}

		private async void GetStopsFromServer(string url) {
			Stops = new List<Stop>();

			WebResponse response = null;
			for ( int i = 0 ; i < 11 ; ++i ) {
				WebRequest request = WebRequest.Create(url);
				try {
					response = await request.GetResponseAsync();
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

		private void MapCanvas_OnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args) {
			float mul = 5f, add = 50f;
			foreach ( Stop stop in Stops ) {
				foreach ( Stop conectedStop in stop.ConnectedStops(Stops) ) {
					args.DrawingSession.DrawLine(add + stop.X * mul, stop.Y * mul,
					                             add + conectedStop.X * 5f, conectedStop.Y * 5f,
					                             Colors.Black);
				}

				args.DrawingSession.FillCircle(add + stop.X * mul, stop.Y * mul, 3f,
				                               stop.NZ ? Colors.DimGray : Colors.Black);
				args.DrawingSession.DrawText(stop.Name,
				                             add + stop.X * mul - 30f, stop.Y * mul + 5f,
				                             stop.NZ ? Colors.DimGray : Colors.Black,
				                             new CanvasTextFormat {
					                             FontSize = 15
				                             });
			}
		}
	}
}