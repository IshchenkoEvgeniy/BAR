using System.Windows;
using BAR.ViewModel;

namespace BAR.View
{
    public partial class CartView : Window
    {
        public CartView()
        {
            InitializeComponent();
            DataContext = new CartViewModel();
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            var transactionWindow = new TransactionView();
            if (transactionWindow.ShowDialog() == true)
            {
                MessageBox.Show("Спасибо за заказ! Ваш заказ успешно оформлен.", "Заказ оформлен", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }
    }
}
