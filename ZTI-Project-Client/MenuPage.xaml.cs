using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=234238

namespace ZTI.Project.Client {
	/// <summary>
	/// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
	/// </summary>
	public sealed partial class MenuPage : Page {
		public MenuPage() { InitializeComponent(); }

		private void HamburgerButton_OnClick(object sender, RoutedEventArgs e) =>
			HamburgerSplitView.IsPaneOpen = !HamburgerSplitView.IsPaneOpen;

		private void HamburgerListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if ( HamburgerSplitView.IsPaneOpen ) HamburgerSplitView.IsPaneOpen = false;

			if ( SearchListBoxItem.IsSelected ) {
				MainFrame.Navigate(typeof(RouteSearchPage));
			} else if ( CreditsListBoxItem.IsSelected ) {
				MainFrame.Navigate(typeof(CreditsPage));
			} else {
				MainFrame.Navigate(typeof(LineStopPage));
			}
		}
	}
}