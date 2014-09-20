using System;
using System.Reflection;
using System.Windows;
using MultiType.Ninject;
using MultiType.Services;
using MultiType.ViewModels;
using Ninject;
using Ninject.Modules;

namespace MultiType.Windows
{
	/// <summary>
	/// Interaction logic for LessonSelect.xaml
	/// </summary>
	public partial class LessonSelect : Window
	{
		public LessonSelect(Window owner = null, bool isSinglePlayer = false)
		{
		    Owner = owner;
			InitializeComponent();
			DataContext = new LessonVm(new StandardKernel(new NinjectBindings()).Get<ILessonManagementService>()){IsSinglePlayer = isSinglePlayer};
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
		    DialogResult = true;
			Close();
		}
	}
}
