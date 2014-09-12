using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MultiType.ViewModels;

namespace MultiType.Windows
{
	/// <summary>
	/// Interaction logic for ClientConnect.xaml
	/// </summary>
	public partial class ClientConnect : Window
	{
		public ClientConnect(Window owner)
		{
			// set up the window and its data context
		    Owner = owner;
			InitializeComponent();
            DataContext = new ConnectVm(this);
		}

		/// <summary>
		/// Block improper input to the IP address text box
		/// </summary>
		private void IP_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			const string pattern = @"^\d$";
			if (!e.Text.Equals(".") && !Regex.IsMatch(e.Text, pattern))
				e.Handled = true;
			if (e.Text.Length >= 15)
				e.Handled = true;
		}

		/// <summary>
		/// Block improper input to the Port # input text box
		/// </summary>
		private void Port_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!Regex.IsMatch(e.Text, @"^\d$"))
				e.Handled = true;
			if (e.Text.Length >= 5)
				e.Handled = true;
		}
	}
}
