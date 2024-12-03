using System.Windows;
using BAR.Services;
using BAR.ViewModel;
using System.Linq;

namespace BAR.View
{
    public partial class CartView : Window
    {
        private readonly CartService _cartService = CartService.Instance;
        
        public CartView()
        {
            InitializeComponent();
            DataContext = new CartViewModel();
            
            // Обновляем общую сумму при изменении корзины
            _cartService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CartService.TotalAmount))
                {
                    // Принудительно обновляем привязку
                    var vm = (CartViewModel)DataContext;
                    vm.UpdateTotalAmount(_cartService.TotalAmount);
                }
            };
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_cartService.Items.Any())
            {
                MessageBox.Show(
                    "Корзина пуста. Добавьте товары перед оформлением заказа.",
                    "Пустая корзина",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var transactionWindow = new TransactionView();
            if (transactionWindow.ShowDialog() == true)
            {
                MessageBox.Show("Спасибо за заказ! Ваш заказ успешно оформлен.", "Заказ оформлен", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }
    }
}
