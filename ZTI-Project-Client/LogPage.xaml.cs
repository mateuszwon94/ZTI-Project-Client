using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using ZTI.Project.Client.Data;

namespace ZTI.Project.Client {
	/// <summary>
	/// Strona wyswietlajaca logowanie
	/// </summary>
	public sealed partial class LogPage : Page {
		public LogPage() {
			InitializeComponent();

			LoadLogs();
		}

		/// <summary>
		/// Funkcja ladujaca logu z serwera
		/// </summary>
		private async void LoadLogs() {
			foreach ( Log log in await Utils.GetListOfLogsFromServer() ) 
				logs_.Add(log);
		}

		private readonly ObservableCollection<Log> logs_ = new ObservableCollection<Log>();
	}
}