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
using ZTI.Project.Client.Data;
using static ZTI.Project.Client.Constants;
using static ZTI.Project.Client.Utils;

namespace ZTI.Project.Client {
	public sealed partial class RouteSearchPage : Page {
		public RouteSearchPage() {
			InitializeComponent();
			SearhTimePicker.Time = DateTime.Now.TimeOfDay;
			LoadingIndicator.Visibility = Visibility.Visible;
			LoadingIndicator.IsActive = true;
			GetStops();

			DispatcherTimer timer = new DispatcherTimer {
				Interval = TimeSpan.FromMilliseconds(100)
			};
			timer.Tick += (sender, e) => {
				if ( Stops != null ) {
					LoadingIndicator.Visibility = Visibility.Collapsed;
					MapCanvas.Visibility = Visibility.Visible;
					if ( sender is DispatcherTimer t ) 
						t.Stop();
				}
			};
			timer.Start();
		}

		private async void GetStops() => Stops = await GetListOfDataFromServer<Stop>(Url.APP + Url.STOPS);

		public List<Stop> Stops;
		public Stop From { get; private set; }
		public Stop To { get; private set; }
		public Route Route { get; private set; }

		private void MapCanvas_OnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args) {
			const float mul = 5f;
			const float add = 25f;

			args.DrawingSession.Clear(Colors.White);

			foreach ( Stop stop in Stops ) {
				foreach ( Stop conectedStop in stop.ConnectedStops(Stops) )
					args.DrawingSession.DrawLine(add + stop.X * mul, stop.Y * mul,
					                             add + conectedStop.X * mul, conectedStop.Y * mul,
					                             Colors.Black);

				args.DrawingSession.FillCircle(add + stop.X * mul, stop.Y * mul, 3f,
				                               stop.NZ ? Colors.DimGray : Colors.Black);
				if ( From == stop )
					args.DrawingSession.DrawCircle(add + stop.X * mul, stop.Y * mul, 10f,
					                               Colors.Blue);
				else if ( To == stop )
					args.DrawingSession.DrawCircle(add + stop.X * mul, stop.Y * mul, 10f,
					                               Colors.Red);
				args.DrawingSession.DrawText(
#if DEBUG
				                             $"{stop}\n({stop.ID})",
#else
											 stop,
#endif
				                             add + stop.X * mul - 30f, stop.Y * mul + 5f,
				                             stop.NZ ? Colors.DimGray : Colors.Black,
				                             new CanvasTextFormat {
					                             FontSize = 15
				                             });
			}

			if ( Route != null ) {
				for ( int i = 0 ; i < Route.Count -1 ; ++i ) {
					args.DrawingSession.DrawLine(add + Route[i].X * mul, Route[i].Y * mul,
					                             add + Route[i+1].X * mul, Route[i+1].Y * mul,
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
					From = Stops?.First(stop => stop.Name.StartsWith(args.QueryText, StringComparison.CurrentCultureIgnoreCase));
					if ( From != null ) sender.Text = From?.Name;
				} else if ( sender == ToStopSearchBox ) {
					To = Stops?.First(stop => stop.Name.StartsWith(args.QueryText, StringComparison.CurrentCultureIgnoreCase));
					if ( To != null ) sender.Text = To.Name;
				}
			} catch ( InvalidOperationException ) {
				return;
			}
		}

		private void StopSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args) {
			if ( sender == FromStopSearchBox )
				From = Stops?.First(stop => stop.Name == (string)args.SelectedItem);
			else if ( sender == ToStopSearchBox )
				To = Stops?.First(stop => stop.Name == (string)args.SelectedItem);
		}

		private void StopSearchBox_OnGotFocus(object sender, RoutedEventArgs e) {
			if ( sender is AutoSuggestBox searchBox ) {
				searchBox.ItemsSource = Stops?.Select(stop => stop.Name)
				                             .OrderBy(stopName => stopName)
				                             .Where(stopName => stopName.ToLower().StartsWith(searchBox.Text.Trim().ToLower()) ||
				                                                stopName.ToLower().Contains(searchBox.Text.Trim().ToLower()));
			}
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
	}
}