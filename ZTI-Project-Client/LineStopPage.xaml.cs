using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using ZTI.Project.Client.Data;
using static ZTI.Project.Client.Constants;
using static ZTI.Project.Client.Utils;

namespace ZTI.Project.Client {
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

			scheduleList_ = new ObservableCollection<Schedule>();
		}

		private async void GetStops() => Stops = await GetListOfDataFromServer<Stop>(Url.APP + Url.STOPS);
		private async void GetLines() => Lines = await GetListOfDataFromServer<Line>(Url.APP + Url.LINES);

		public static List<Stop> Stops;
		public static List<Line> Lines;

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
				args.DrawingSession.DrawText(stop.Name,
				                             add + stop.X * mul - 30f, stop.Y * mul + 5f,
				                             stop.NZ ? Colors.DimGray : Colors.Black,
				                             new CanvasTextFormat {
					                             FontSize = 15
				                             });
				lock ( scheduleListMutex_ ) {
					if ( scheduleList_ != null && scheduleList_.Count > 0 && selectedStop_ == null ) {

						for ( int i = 0 ; i < scheduleList_.Count - 1 ; ++i ) {
							args.DrawingSession.DrawLine(add + scheduleList_[i].Stop.X * mul, scheduleList_[i].Stop.Y * mul,
							                             add + scheduleList_[i + 1].Stop.X * mul, scheduleList_[i + 1].Stop.Y * mul,
							                             Colors.LightGreen);
							args.DrawingSession.FillCircle(add + scheduleList_[i].Stop.X * mul, scheduleList_[i].Stop.Y * mul, 5f,
							                               Colors.Green);
						}
						args.DrawingSession.FillCircle(add + scheduleList_.Last().Stop.X * mul, scheduleList_.Last().Stop.Y * mul, 5f,
						                               Colors.Green);
					} else if ( selectedStop_ != null ) {
						args.DrawingSession.DrawCircle(add + selectedStop_.X * mul, selectedStop_.Y * mul, 10f,
						                               Colors.Blue);
					}
				}
			}
		}

		private void StopSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
			lock ( scheduleListMutex_ )
				scheduleList_.Clear();
			selectedStop_ = null;

			if ( args.Reason == AutoSuggestionBoxTextChangeReason.UserInput ) {
				sender.ItemsSource = Stops.Select(stop => stop.Name)
				                          .OrderBy(stopName => stopName)
				                          .Where(stopName => stopName.ToLowerInvariant()
				                                                     .StartsWith(sender.Text.Trim().ToLowerInvariant()) ||
				                                             stopName.ToLowerInvariant()
				                                                     .Contains(sender.Text.Trim().ToLowerInvariant()));
			}
		}

		private async void StopSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
			selectedStop_ =
				Stops?.First(s => s.Name.Trim()
				                   .StartsWith(args.QueryText != string.Empty ? args.QueryText : (string)args.ChosenSuggestion,
				                               StringComparison.CurrentCultureIgnoreCase));

			sender.Text = selectedStop_.Name + (selectedStop_.NZ ? " NZ" : string.Empty);

			lock ( scheduleListMutex_ )
				scheduleList_.Clear();

			IEnumerable<Schedule> schedules = await GetSchedule(selectedStop_);
			lock ( scheduleListMutex_ ) {
				foreach ( Schedule schedile in schedules )
					scheduleList_.Add(schedile);
			}
		}

		private void StopSearchBox_OnGotFocus(object sender, RoutedEventArgs e) {
			if ( sender is AutoSuggestBox searchBox ) {
				searchBox.ItemsSource = Stops?.Select(stop => stop.Name)
				                             .OrderBy(stopName => stopName)
				                             .Where(stopName => stopName.ToLowerInvariant()
				                                                        .StartsWith(searchBox.Text.Trim().ToLowerInvariant()) ||
				                                                stopName.ToLowerInvariant()
				                                                        .Contains(searchBox.Text.Trim().ToLowerInvariant()));
			}
		}

		private void LineSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
			lock ( scheduleListMutex_ )
				scheduleList_.Clear();

			if ( args.Reason == AutoSuggestionBoxTextChangeReason.UserInput ) {
				sender.ItemsSource = Lines?.SelectMany(line => {
					                          Stop firstStop = Stops.First(stop => stop.ID == line.Route[0]);
					                          Stop lastStop = Stops.First(stop => stop.ID == line.Route[line.Route.Length - 1]);
					                          if ( line.Route[0] == line.Route[line.Route.Length - 1] )
						                          return new List<(int num, string dir)>(1) {
							                          (line.Number, $" {firstStop} <-> {lastStop}")
						                          };
					                          else
						                          return new List<(int num, string dir)>(2) {
							                          (line.Number, $" {firstStop} <-> {lastStop}"),
							                          (line.Number, $" {lastStop} <-> {firstStop}")
						                          };
				                          })
				                          .OrderBy(line => line.num)
				                          .ThenBy(line => line.dir)
				                          .Select(line => $"{line.num}{line.dir}")
				                          .Where(line =>
					                                 line.ToLowerInvariant().StartsWith(sender.Text.Trim().ToLowerInvariant()) ||
					                                 line.ToLowerInvariant().Contains(sender.Text.Trim().ToLowerInvariant()));
			}
		}

		private async void LineSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
			Line line =
				Lines?.First(l => l.ToStrings(Stops)
				                   .Any(lineStr =>
					                        lineStr
						                        .StartsWith(args.QueryText != string.Empty ? args.QueryText : (string)args.ChosenSuggestion,
						                                    StringComparison.CurrentCultureIgnoreCase)));

			try {
				sender.Text = line.ToStrings(Stops)[2];
			} catch ( IndexOutOfRangeException ) {
				sender.Text = line.ToStrings(Stops)[0];
			}

			lock ( scheduleListMutex_ )
				scheduleList_.Clear();

			IEnumerable<Schedule> schedules = await GetSchedule(line);
			lock ( scheduleListMutex_ ) {
				foreach ( Schedule schedile in schedules )
					scheduleList_.Add(schedile);
			}
		}

		private void LineSearchBox_OnGotFocus(object sender, RoutedEventArgs e) {
			if ( sender is AutoSuggestBox searchBox ) {
				searchBox.ItemsSource = Lines?.SelectMany(line => {
					                             Stop firstStop = Stops.First(stop => stop.ID == line.Route[0]);
					                             Stop lastStop = Stops.First(stop => stop.ID == line.Route[line.Route.Length - 1]);
					                             if ( line.Route[0] == line.Route[line.Route.Length - 1] )
						                             return new List<(int num, string dir)>(1) {
							                             (line.Number, $" {firstStop} <-> {lastStop}")
						                             };
					                             else
						                             return new List<(int num, string dir)>(2) {
							                             (line.Number, $" {firstStop} <-> {lastStop}"),
							                             (line.Number, $" {lastStop} <-> {firstStop}")
						                             };
				                             })
				                             .OrderBy(line => line.num)
				                             .ThenBy(line => line.dir)
				                             .Select(line => $"{line.num}{line.dir}")
				                             .Where(line => line.ToLowerInvariant()
				                                                .StartsWith(searchBox.Text.Trim().ToLowerInvariant()) ||
				                                            line.ToLowerInvariant()
				                                                .Contains(searchBox.Text.Trim().ToLowerInvariant()));
			}
		}

		private async Task<IEnumerable<Schedule>> GetSchedule(Line line) {
			ScheduleLoading.Visibility = Visibility.Visible;
			ScheduleLoading.IsActive = true;

			var args = new Dictionary<string, string> {
				[LINE] = line.Number.ToString(),
			};

			var content = new FormUrlEncodedContent(args);

			var responseMsg = await Http.Client.PostAsync(Url.APP + Url.SCHEDULES, content);
			string response = await responseMsg.Content.ReadAsStringAsync();

			ScheduleLoading.IsActive = false;
			ScheduleLoading.Visibility = Visibility.Collapsed;

			return Schedule.CreateFromXml(response, Stops, Lines);
		}

		private async Task<IEnumerable<Schedule>> GetSchedule(Stop stop) {
			ScheduleLoading.Visibility = Visibility.Visible;
			ScheduleLoading.IsActive = true;

			var args = new Dictionary<string, string> {
				[STOP] = stop.ID.ToString(),
			};

			var content = new FormUrlEncodedContent(args);

			var responseMsg = await Http.Client.PostAsync(Url.APP + Url.SCHEDULES, content);
			string response = await responseMsg.Content.ReadAsStringAsync();

			ScheduleLoading.IsActive = false;
			ScheduleLoading.Visibility = Visibility.Collapsed;

			return Schedule.CreateFromXml(response, Stops, Lines);
		}

		private readonly object scheduleListMutex_ = new object();
		private readonly ObservableCollection<Schedule> scheduleList_;
		private Stop selectedStop_;
	}
}