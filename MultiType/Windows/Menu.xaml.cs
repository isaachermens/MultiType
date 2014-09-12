using System.Windows;
using MultiType.ViewModels;

namespace MultiType.Windows
{
	/// <summary>
	/// Interaction logic for Menu.xaml
	/// </summary>
	public partial class Menu : Window
	{
		public Menu()
		{
			InitializeComponent();
		    DataContext = new MenuVm();
		}
	}
}
