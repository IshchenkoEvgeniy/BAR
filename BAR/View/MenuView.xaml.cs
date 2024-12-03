using System.Windows;
using BAR.ViewModel;

namespace BAR.View
{
    public partial class MenuView : Window
    {
        public MenuView()
        {
            InitializeComponent();
            DataContext = new MenuViewModel();
        }
    }
}
