using System.Windows;
using PropertyChanged;

namespace MultiType.ViewModels
{

    [ImplementPropertyChanged]
    public class BaseVm
    {
        protected void ShowWindowAsDialog(Window host, Window target)
        {
            host.Hide();
            if (target.ShowDialog() == true)
            {
                host.Show();
            }
            else
            {
                host.Close();
            }
        }
    }
}
