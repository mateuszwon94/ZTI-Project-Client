using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
using ZTI.Project.Client.Data;
using static ZTI.Project.Client.Constants;
using static ZTI.Project.Client.Utils;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ZTI.Project.Client {
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class LineStopPage : Page {
		public LineStopPage() {
			InitializeComponent();
			LoadingIndicator.Visibility = Visibility.Visible;
			LoadingIndicator.IsActive = true;
			GetStops();
			GetLines();

			DispatcherTimer timer = new DispatcherTimer {
				Interval = TimeSpan.FromMilliseconds(100)
			};
			timer.Tick += (sender, e) => {
				if ( Stops != null ) {
					LoadingIndicator.Visibility = Visibility.Collapsed;
					MapCanvas.Visibility = Visibility.Visible;
				}
				if ( Lines != null ) { }
				if ( sender is DispatcherTimer t && Lines != null && Stops != null )
					t.Stop();
			};
			timer.Start();

			stopList_ = new ObservableCollection<Stop>();
			//stopList_.CollectionChanged += (sender, args) => 
			//	StopListView.Visibility = args.NewItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
		}

		private async void GetStops() => Stops = await GetListOfDataFromServer<Stop>(Url.APP + Url.STOPS);
		private async void GetLines() => Lines = await GetListOfDataFromServer<Line>(Url.APP + Url.LINES);

		public List<Stop> Stops;
		public List<Line> Lines;

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
				args.DrawingSession.DrawText(
#if DEBUG
				                             $"{stop.Name}\n({stop.ID})",
#else
											 stop.Name,
#endif
				                             add + stop.X * mul - 30f, stop.Y * mul + 5f,
				                             stop.NZ ? Colors.DimGray : Colors.Black,
				                             new CanvasTextFormat {
					                             FontSize = 15
				                             });
				lock ( stopList_ ) {
					if ( stopList_ != null && stopList_.Count > 0 ) {

						for ( int i = 0 ; i < stopList_.Count - 1 ; ++i ) {
							args.DrawingSession.DrawLine(add + stopList_[i].X * mul, stopList_[i].Y * mul,
							                             add + stopList_[i + 1].X * mul, stopList_[i + 1].Y * mul,
							                             Colors.LightGreen);
							args.DrawingSession.FillCircle(add + stopList_[i].X * mul, stopList_[i].Y * mul, 5f,
							                               Colors.Green);
						}
						args.DrawingSession.FillCircle(add + stopList_.Last().X * mul, stopList_.Last().Y * mul, 5f,
						                               Colors.Green);
					}
				}
			}
		}

		private void StopSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
			if ( args.Reason == AutoSuggestionBoxTextChangeReason.UserInput ) {
				sender.ItemsSource = Stops.Select(stop => stop.Name)
				                          .OrderBy(stopName => stopName)
				                          .Where(stopName => stopName.ToLower().StartsWith(sender.Text.Trim().ToLower()) ||
				                                             stopName.ToLower().Contains(sender.Text.Trim().ToLower()));
			}
		}

		private void StopSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) { }

		private void StopSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args) { }

		private void StopSearchBox_OnGotFocus(object sender, RoutedEventArgs e) {
			if ( sender is AutoSuggestBox searchBox ) {
				searchBox.ItemsSource = Stops?.Select(stop => stop.Name)
				                             .OrderBy(stopName => stopName)
				                             .Where(stopName => stopName.ToLower().StartsWith(searchBox.Text.Trim().ToLower()) ||
				                                                stopName.ToLower().Contains(searchBox.Text.Trim().ToLower()));
			}
		}

		private void LineSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
			if ( args.Reason != AutoSuggestionBoxTextChangeReason.SuggestionChosen )
				lock ( stopList_ ) 
					stopList_.Clear();
				
			if ( args.Reason == AutoSuggestionBoxTextChangeReason.UserInput ) {
				sender.ItemsSource = Lines?.SelectMany(line => {
					                          Stop firstStop = Stops.First(stop => stop.ID == line.Route[0]);
					                          Stop lastStop = Stops.First(stop => stop.ID == line.Route[line.Route.Length - 1]);
					                          return new List<(int num, string dir)>(2) {
						                          (line.Number, $"{line.Variants?[0]} {firstStop} -> {lastStop}"),
						                          (line.Number, $"{line.Variants?[1]} {lastStop} -> {firstStop}")
					                          };
				                          })
				                          .OrderBy(line => line.num)
				                          .ThenBy(line => line.dir)
				                          .Select(line => $"{line.num}{line.dir}")
				                          .Where(line => line.ToLower().StartsWith(sender.Text.Trim().ToLower()) ||
				                                         line.ToLower().Contains(sender.Text.Trim().ToLower()));
			}
		}

		private void LineSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
			Line line =
				Lines?.First(l => l.ToStrings(Stops)
				                   .Any(lineStr => lineStr.StartsWith(args.QueryText,
				                                                      StringComparison.CurrentCultureIgnoreCase)));

			lock ( stopList_ ) {
				stopList_.Clear();
				foreach ( Stop stop in line?.Route?.Select(id => Stops.First(stop => stop.ID == id)) )
					stopList_.Add(stop);
			}
		}

		private void LineSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args) {
			Line line =
				Lines?.First(l => l.ToStrings(Stops)
				                   .Any(lineStr => lineStr == (string)args.SelectedItem));

			lock ( stopList_ ) {
				stopList_.Clear();
				foreach ( Stop stop in line?.Route?.Select(id => Stops.First(stop => stop.ID == id)) )
					stopList_.Add(stop);
			}
		}

		private void LineSearchBox_OnGotFocus(object sender, RoutedEventArgs e) {
			if ( sender is AutoSuggestBox searchBox ) {
				searchBox.ItemsSource = Lines?.SelectMany(line => {
					                             Stop firstStop = Stops.First(stop => stop.ID == line.Route[0]);
					                             Stop lastStop = Stops.First(stop => stop.ID == line.Route[line.Route.Length - 1]);
					                             return new List<(int num, string dir)>(2) {
						                             (line.Number, $"{line.Variants?[0]} {firstStop} -> {lastStop}"),
						                             (line.Number, $"{line.Variants?[1]} {lastStop} -> {firstStop}")
					                             };
				                             })
				                             .OrderBy(line => line.num)
				                             .ThenBy(line => line.dir)
				                             .Select(line => $"{line.num}{line.dir}")
				                             .Where(line => line.ToLower().StartsWith(searchBox.Text.Trim().ToLower()) ||
				                                            line.ToLower().Contains(searchBox.Text.Trim().ToLower()));
			}
		}

		private readonly ObservableCollection<Stop> stopList_; 
	}
}