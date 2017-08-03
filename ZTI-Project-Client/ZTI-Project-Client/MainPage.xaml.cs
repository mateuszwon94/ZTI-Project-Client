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
using static ZTI_Project_Client.Constants;

namespace ZTI_Project_Client {
	public sealed partial class MainPage : Page {
		public MainPage() {
			InitializeComponent();
			Stops = new List<Stop>();

			WebRequest request;
			WebResponse response;
			while ( true ) {
				request = WebRequest.Create("http://localhost:9081/ZTI-Project/Stops");
				try {
					response = request.GetResponseAsync().Result;
					break;
				} catch ( Exception ex )
					when ( ex is WebException ||
					       ex.InnerException is WebException ) {
				}
			}
			
			using ( StreamReader reader = new StreamReader(response.GetResponseStream()) ) {
				XmlSerializer deserializer = new XmlSerializer(typeof(List<Stop>),
				                                               new XmlRootAttribute(ROOT));
				Stops = (List<Stop>)deserializer.Deserialize(reader);
			}

		}

		public List<Stop> Stops;

		private void Canvas_OnDraw(CanvasControl sender, CanvasDrawEventArgs args) {
			foreach ( Stop stop in Stops ) {
				args.DrawingSession.FillCircle(stop.X * 5, stop.Y * 5, 3f,
				                               stop.NZ ? Colors.DimGray : Colors.Black);
				args.DrawingSession.DrawText(stop.Name,
				                             stop.X * 5 - 15, stop.Y * 5,
				                             stop.NZ ? Colors.DimGray : Colors.Black,
				                             new CanvasTextFormat() {
					                             FontSize = 15,
					                             FontFamily = "Times New Roman"
				                             });
			}
		}
	}
}