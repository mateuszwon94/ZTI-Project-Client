using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml.Serialization;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
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

		private readonly object routeMutex_ = new object();
		public static List<Stop> Stops;
		public Stop From { get; private set; }
		public Stop To { get; private set; }
		public readonly ObservableCollection<Data.Route.Stop> Route = new ObservableCollection<Data.Route.Stop>();

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
				args.DrawingSession.DrawText(stop.ToString(),
				                             add + stop.X * mul - 30f, stop.Y * mul + 5f,
				                             stop.NZ ? Colors.DimGray : Colors.Black,
				                             new CanvasTextFormat {
					                             FontSize = 15
				                             });
			}

			lock ( routeMutex_ ) {
				if ( Route.Count > 0 ) {
					Stop currStop = Stops.First(s => s.ID == Route[0].ID);
					for ( int i = 0 ; i < Route.Count - 1 ; ++i ) {
						Stop nextStop = Stops.First(s => s.ID == Route[i + 1].ID);
						args.DrawingSession.DrawLine(add + currStop.X * mul, currStop.Y * mul,
						                             add + nextStop.X * mul, nextStop.Y * mul,
						                             lineColors[Route[i].Line], 3);
						args.DrawingSession.FillCircle(add + currStop.X * mul, currStop.Y * mul, 5f,
						                               Colors.Green);
						currStop = nextStop;
					}
					currStop = Stops.First(s => s.ID == Route[Route.Count - 1].ID);
					args.DrawingSession.FillCircle(add + currStop.X * mul, currStop.Y * mul, 5f,
					                               Colors.Green);
				}
			}
		}

		private void StopSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
			if ( args.Reason == AutoSuggestionBoxTextChangeReason.UserInput ) {
				if ( sender == FromStopSearchBox )
					From = null;
				else if ( sender == ToStopSearchBox )
					To = null;

				lock ( routeMutex_ )
					Route.Clear();

				sender.ItemsSource = Stops.Select(stop => stop.Name)
				                          .OrderBy(stopName => stopName)
				                          .Where(stopName => stopName.ToLower().StartsWith(sender.Text.Trim().ToLower()) ||
				                                             stopName.ToLower().Contains(sender.Text.Trim().ToLower()));
			}
		}

		private void StopSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
			try {
				if ( sender == FromStopSearchBox ) {
					From = Stops?.First(stop => stop.Name.Trim()
					                                .StartsWith(args.QueryText != string.Empty ? args.QueryText : (string)args.ChosenSuggestion,
					                                            StringComparison.CurrentCultureIgnoreCase));
					if ( From != null ) sender.Text = From?.Name;
				} else if ( sender == ToStopSearchBox ) {
					To = Stops?.First(stop => stop.Name.Trim()
					                              .StartsWith(args.QueryText != string.Empty ? args.QueryText : (string)args.ChosenSuggestion,
					                                          StringComparison.CurrentCultureIgnoreCase));
					if ( To != null ) sender.Text = To.Name;
				}
			} catch ( InvalidOperationException ) {
				return;
			}
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
				lock ( routeMutex_ )
					Route.Clear();

				var stops = new Dictionary<string, string> {
					[FROM] = From.ID.ToString(),
					[TO] = To.ID.ToString(),
					[TIME] = SearhTimePicker.Time.ToString()
				};

				var content = new FormUrlEncodedContent(stops);

				var responseMsg = await Http.Client.PostAsync(Url.APP + Url.SEARCH_ROUTE, content);
				string response = await responseMsg.Content.ReadAsStringAsync();

				using ( Stream responseStream = new MemoryStream(Encoding.UTF8.GetBytes(response)) ) {
					XmlSerializer deserializer = new XmlSerializer(typeof(List<Data.Route.Stop>), new XmlRootAttribute(ROUTE));

					List<Data.Route.Stop> list = (List<Data.Route.Stop>)deserializer.Deserialize(responseStream);
					lock ( routeMutex_ ) {
						foreach ( Data.Route.Stop stop in list )
							Route.Add(stop);
					}
				}
			}
		}

		private static readonly Dictionary<int, Color> lineColors = new Dictionary<int, Color>(11) {
			[0] = Colors.Brown,
			[1] = Colors.BlueViolet,
			[2] = Colors.DarkKhaki,
			[3] = Colors.DarkOrange,
			[4] = Colors.Indigo,
			[5] = Colors.Navy,
			[6] = Colors.Violet,
			[7] = Colors.Tomato,
			[8] = Colors.Olive,
			[9] = Colors.DarkOrchid,
			[10] = Colors.IndianRed,
		};
	}
}