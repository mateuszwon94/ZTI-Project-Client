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
	/// <inheritdoc cref="Page" />
	/// <summary>
	/// Strona z widokiem rozkladu jazd y dla linii i przystanku
	/// </summary>
	public sealed partial class LineStopPage : Page {

		#region Konstruktor

		/// <inheritdoc />
		/// <summary>
		/// konstruktor
		/// </summary>
		public LineStopPage() {
			InitializeComponent();

			// Pokazanie indykatora ladowania
			LoadingIndicator.Visibility = Visibility.Visible;
			LoadingIndicator.IsActive = true;

			// Pobranie przystankow i linii z serwera
			GetStops();
			GetLines();

			// Timer, ktory ma sprawdzac czy linie i przystanki zostaly juz pobrane
			DispatcherTimer timer = new DispatcherTimer {
				Interval = TimeSpan.FromMilliseconds(100)
			};
			timer.Tick += (sender, e) => {
				if ( Stops != null ) { // jesli przystanki sa pobrane ukryj ladowanie i pokaz mape
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

		#endregion Konstruktor

		#region Public Variables

		/// <summary>
		/// lista wszystkich przystankow
		/// </summary>
		public static List<Stop> Stops;

		/// <summary>
		/// lista wszystkich linii
		/// </summary>
		public static List<Line> Lines;

		#endregion Public Variables

		#region Private Methods

		/// <summary>
		/// Asynchroniczna funkcja pobierajaca przystanki serwera
		/// </summary>
		private async void GetStops() => Stops = await GetListOfDataFromServer<Stop>(Url.APP + Url.STOPS);

		/// <summary>
		/// Asynchroniczna funkcja pobierajaca linie z serwera
		/// </summary>
		private async void GetLines() => Lines = await GetListOfDataFromServer<Line>(Url.APP + Url.LINES);

		/// <summary>
		/// Asynchroniczna funkcja zwracajaca rozklad jazdy dla konkretnej linii lub przystanku pobrany z serwera
		/// </summary>
		/// <typeparam name="T">Typ dla jakiego szukamy przystanku <see cref="Stop"/> lub <see cref="Line"/></typeparam>
		/// <param name="val">Linia lub przystanek, dla ktoreg ma byc pobrany rozklad</param>
		/// <returns>Rozklad</returns>
		private async Task<IEnumerable<Schedule>> GetSchedule<T>(T val) {
			ScheduleLoading.Visibility = Visibility.Visible;
			ScheduleLoading.IsActive = true;

			Dictionary<string, string> args = new Dictionary<string, string>();

			if ( val is Line line ) 
				args[LINE] = line.Number.ToString();
			else if ( val is Stop stop )
				args[STOP] = stop.ID.ToString();

			var content = new FormUrlEncodedContent(args);

			var responseMsg = await Http.Client.PostAsync(Url.APP + Url.SCHEDULES, content);
			string response = await responseMsg.Content.ReadAsStringAsync();

			ScheduleLoading.IsActive = false;
			ScheduleLoading.Visibility = Visibility.Collapsed;

			return Schedule.CreateFromXml(response, Stops, Lines);
		}

		#endregion Private Methods

		#region Event Handlers

		/// <summary>
		/// Funkcja wywolywana przy rysowaniu na mapie
		/// </summary>
		/// <param name="sender">Mapa</param>
		/// <param name="args">Argument eventu</param>
		private void MapCanvas_OnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args) {
			const float mul = 5f;
			const float add = 25f;

			args.DrawingSession.Clear(Colors.White);

			// Rysowanie wszystkich przystankow i polaczen miedzy nimi
			foreach ( Stop stop in Stops ) {
				// Rysowanie wszystkich polaczen z obecnego przystanku
				foreach ( Stop conectedStop in stop.ConnectedStops(Stops) )
					args.DrawingSession.DrawLine(add + stop.X * mul, stop.Y * mul,
					                             add + conectedStop.X * mul, conectedStop.Y * mul,
					                             Colors.Black);

				// Rysowanie przystanku i jego nazwy
				args.DrawingSession.FillCircle(add + stop.X * mul, stop.Y * mul, 3f,
				                               stop.NZ ? Colors.DimGray : Colors.Black);
				args.DrawingSession.DrawText(stop.Name,
				                             add + stop.X * mul - 30f, stop.Y * mul + 5f,
				                             stop.NZ ? Colors.DimGray : Colors.Black,
				                             new CanvasTextFormat {
					                             FontSize = 15
				                             });

				// Rysowanie wybranej trasy
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

		/// <summary>
		/// Funkcja wywolywana przy zmianie tekstu w polu wyboru przystnaku
		/// </summary>
		/// <param name="sender">Pole wyboru przystanku</param>
		/// <param name="args">Argumenty eventu</param>
		private void StopSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
			if ( args.Reason == AutoSuggestionBoxTextChangeReason.UserInput ) {
				// Wyczyszczenie wyswietlanego rozkladu
				lock ( scheduleListMutex_ )
					scheduleList_.Clear();
				selectedStop_ = null;

				// Wybranie najbardziej pasujacego przystanku
				sender.ItemsSource = Stops.Select(stop => stop.Name)
				                          .OrderBy(stopName => stopName)
				                          .Where(stopName => stopName.ToLowerInvariant()
				                                                     .StartsWith(sender.Text.Trim().ToLowerInvariant()) ||
				                                             stopName.ToLowerInvariant()
				                                                     .Contains(sender.Text.Trim().ToLowerInvariant()));
			}
		}

		/// <summary>
		/// Funkcja wywoływana przy zatwierdzeniu wyboru w polu przystanku
		/// </summary>
		/// <param name="sender">Pole wyboru przystanku</param>
		/// <param name="args">Argumenty eventu</param>
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

		/// <summary>
		/// Funkcja wywolywana przy kliknieciu na pole wyboru przystanku
		/// </summary>
		/// <param name="sender">Pole wyboru przystanku</param>
		/// <param name="e">Argumenty eventu</param>
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

		/// <summary>
		/// Funkcja wywolywana przy zmianie tekstu w polu wyboru linii
		/// </summary>
		/// <param name="sender">Pole wyboru linii</param>
		/// <param name="args">Argumenty eventu</param>
		private void LineSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
			if ( args.Reason == AutoSuggestionBoxTextChangeReason.UserInput ) {
				lock ( scheduleListMutex_ )
					scheduleList_.Clear();

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

		/// <summary>
		/// Funkcja wywoływana przy zatwierdzeniu wyboru w polu linii
		/// </summary>
		/// <param name="sender">Pole wyboru linii</param>
		/// <param name="args">Argumenty eventu</param>
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

		/// <summary>
		/// Funkcja wywolywana przy kliknieciu na pole wyboru linii
		/// </summary>
		/// <param name="sender">Pole wyboru linii</param>
		/// <param name="e">Argumenty eventu</param>
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

		#endregion Event Handlers

		#region Private Variables

		private readonly object scheduleListMutex_ = new object();
		private readonly ObservableCollection<Schedule> scheduleList_;
		private Stop selectedStop_;

		#endregion
	}
}