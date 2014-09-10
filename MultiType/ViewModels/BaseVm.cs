using System.Windows;
using PropertyChanged;

namespace MultiType.ViewModels
{

    [ImplementPropertyChanged]
    public class BaseVm
    {
        protected Window HostWindow { get; set; }
        protected void ShowWindowAsDialog(Window target)
        {
            HostWindow.Hide();
            if (target.ShowDialog() == true)
            {
                HostWindow.Show();
            }
            else
            {
                HostWindow.Close();
            }
        }
    }
}
