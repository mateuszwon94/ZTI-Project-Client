using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ZTI.Project.Client {
	/// <summary>
	/// Strona wyswietlajaca widok menu
	/// </summary>
	public sealed partial class MenuPage : Page {
		public MenuPage() { InitializeComponent(); }

		/// <summary>
		/// Funkcja wywolywana w momencie nacisniecia przycisku hamburger menu. Zwija i rozwija menu
		/// </summary>
		/// <param name="sender">Przycisk hamburger menu</param>
		/// <param name="e">Argumenty eventu</param>
		private void HamburgerButton_OnClick(object sender, RoutedEventArgs e) =>
			HamburgerSplitView.IsPaneOpen = !HamburgerSplitView.IsPaneOpen;

		/// <summary>
		/// Funkcja wywolywana przy wyborze jednego z elementow menu
		/// </summary>
		/// <param name="sender">Lista menu</param>
		/// <param name="e">Argumenty eventu</param>
		private void HamburgerListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if ( HamburgerSplitView.IsPaneOpen ) HamburgerSplitView.IsPaneOpen = false;
			
			if ( SearchListBoxItem.IsSelected ) {
				MainFrame.Navigate(typeof(RouteSearchPage));
			} else if ( CreditsListBoxItem.IsSelected ) {
				MainFrame.Navigate(typeof(CreditsPage));
			} else if (LogsListBoxItem.IsSelected) {
				MainFrame.Navigate(typeof(LogPage));
			} else {
				MainFrame.Navigate(typeof(LineStopPage));
			}
		}
	}
}