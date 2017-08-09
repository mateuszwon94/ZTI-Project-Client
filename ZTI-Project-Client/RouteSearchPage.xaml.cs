using System;
using System.Net;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Input;
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
using static System.String;
using static ZTI.Project.Client.Constants;

namespace ZTI.Project.Client {
	public sealed partial class RouteSearchPage : Page {
		public RouteSearchPage() {
			InitializeComponent();
			GetStopsFromServer(Url.APP + Url.STOPS);
		}

		private async void GetStopsFromServer(string url) {
			Stops = new List<Stop>();

			for ( int i = 0 ; i < 11 ; ++i ) {
				try {
					using ( Stream response = await Http.Client.GetStreamAsync(url) ) {
						XmlSerializer deserializer = new XmlSerializer(typeof(List<Stop>),
						                                               new XmlRootAttribute(ROOT));
						Stops = (List<Stop>)deserializer.Deserialize(response);
					}
					break;
				} catch ( Exception ex )
					when ( ex is HttpRequestException ||
					       ex.InnerException is HttpRequestException ) {
					if ( i == 10 ) return;
				}
			}

			LoadingIndicator.Visibility = Visibility.Collapsed;
			MapCanvas.Visibility = Visibility.Visible;
		}

		public List<Stop> Stops;
		public Stop From { get; private set; }
		public Stop To { get; private set; }
		public Route Route { get; private set; }

		private void MapCanvas_OnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args) {
			const float mul = 5f;
			const float add = 50f;

			args.DrawingSession.Clear(Colors.White);
			foreach ( Stop stop in Stops ) {
				foreach ( Stop conectedStop in stop.ConnectedStops(Stops) ) 
					args.DrawingSession.DrawLine(add + stop.X * mul, stop.Y * mul,
					                             add + conectedStop.X * 5f, conectedStop.Y * 5f,
					                             Colors.Black);

				args.DrawingSession.FillCircle(add + stop.X * mul, stop.Y * mul, 3f,
				                               stop.NZ ? Colors.DimGray : Colors.Black);
				if ( From == stop ) 
					args.DrawingSession.DrawCircle(add + stop.X * mul, stop.Y * mul, 10f,
					                               Colors.Blue);
				if ( To == stop )
					args.DrawingSession.DrawCircle(add + stop.X * mul, stop.Y * mul, 10f,
					                               Colors.Red);
				args.DrawingSession.DrawText(stop.Name,
				                             add + stop.X * mul - 30f, stop.Y * mul + 5f,
				                             stop.NZ ? Colors.DimGray : Colors.Black,
				                             new CanvasTextFormat {
					                             FontSize = 15
				                             });
			}

			if ( Route != null ) {
				for ( int i = 0 ; i < Route.Count -1 ; ++i ) {
					args.DrawingSession.DrawLine(add + Route[i].X * mul, Route[i].Y * mul,
					                             add + Route[i+1].X * 5f, Route[i+1].Y * 5f,
					                             Colors.LightGreen);
					args.DrawingSession.FillCircle(add + Route[i].X * mul, Route[i].Y * mul, 5f,
					                               Colors.Green);
				}
				args.DrawingSession.FillCircle(add + Route[Route.Count -1].X * mul, Route[Route.Count - 1].Y * mul, 5f,
				                               Colors.Green);
			}
		}

		private void StopSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
			if ( args.Reason == AutoSuggestionBoxTextChangeReason.UserInput ) {
				if ( sender == FromStopSearchBox )
					From = null;
				else if ( sender == ToStopSearchBox )
					To = null;

				sender.ItemsSource = Stops.Select(stop => stop.Name)
				                          .OrderBy(stopName => stopName)
				                          .Where(stopName => stopName.ToLower().StartsWith(sender.Text.Trim().ToLower()) ||
				                                             stopName.ToLower().Contains(sender.Text.Trim().ToLower()));
			}
		}

		private void StopSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
			try {
				if ( sender == FromStopSearchBox ) {
					From = Stops.First(stop => string.Equals(stop.Name, args.QueryText, StringComparison.CurrentCultureIgnoreCase));
					sender.Text = From.Name;
				} else if ( sender == ToStopSearchBox ) {
					To = Stops.First(stop => string.Equals(stop.Name, args.QueryText, StringComparison.CurrentCultureIgnoreCase));
					sender.Text = To.Name;
				}
			} catch ( InvalidOperationException ) {
				return;
			}
		}

		private void StopSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args) {
			if ( sender == FromStopSearchBox )
				From = Stops.First(stop => stop.Name == (string)args.SelectedItem);
			else if ( sender == ToStopSearchBox )
				To = Stops.First(stop => stop.Name == (string)args.SelectedItem);
		}

		private async void SearchButton_OnClick(object sender, RoutedEventArgs e) {
			if ( From != null && To != null ) {
				var stops = new Dictionary<string, string> {
					["from"] = From.ID.ToString(),
					["to"] = To.ID.ToString()
				};

				var content = new FormUrlEncodedContent(stops);

				var responseMsg = await Http.Client.PostAsync(Url.APP + Url.SEARCH_ROUTE, content);
				string response = await responseMsg.Content.ReadAsStringAsync();

				Route = new Route(response, Stops);
				From = Route[0];
				To = Route[Route.Count - 1];
			}
		}

		private void StopSearchBox_OnGotFocus(object sender, RoutedEventArgs e) {
			if ( sender is AutoSuggestBox searchBox ) {
				searchBox.ItemsSource = Stops.Select(stop => stop.Name)
				                             .OrderBy(stopName => stopName)
				                             .Where(stopName => stopName.ToLower().StartsWith(searchBox.Text.Trim().ToLower()) ||
				                                                stopName.ToLower().Contains(searchBox.Text.Trim().ToLower()));
			}
		}
	}
}