using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using ZTI.Project.Client.Data;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ZTI.Project.Client {
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class LogPage : Page {
		public LogPage() {
			InitializeComponent();

			LoadLogs();
		}

		private async void LoadLogs() {
			foreach ( Log log in await Utils.GetListOfLogsFromServer() ) 
				logs_.Add(log);
		}

		private readonly ObservableCollection<Log> logs_ = new ObservableCollection<Log>();
	}
}