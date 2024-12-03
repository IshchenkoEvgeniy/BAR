using System.Windows;
using BAR.ViewModel;

namespace BAR.View
{
    public partial class DrinksMenuView : Window
    {
        public DrinksMenuView()
        {
            InitializeComponent();
            DataContext = new MenuViewModel();
        }
    }
}
