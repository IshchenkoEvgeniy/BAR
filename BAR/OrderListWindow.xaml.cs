using System;
using System.Windows;
using System.Windows.Controls;
using BAR.Model;
using BAR.Services;
using System.Text;

namespace BAR
{
    public partial class OrderListWindow : Window
    {
        private readonly OrderService _orderService;
        private readonly TestOrderWindow _testOrderWindow;

        public OrderListWindow()
        {
            InitializeComponent();
            _orderService = new OrderService();
            _testOrderWindow = new TestOrderWindow();
            RefreshOrdersList();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshOrdersList();
        }

        private void CreateTestOrder_Click(object sender, RoutedEventArgs e)
        {
            _testOrderWindow.Show();
        }

        private void RefreshOrdersList()
        {
            try
            {
                var orders = _orderService.GetAllOrders();
                OrdersListView.ItemsSource = orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заказов: {ex.Message}", "Ошибка");
            }
        }

        private void ShowDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Order order)
            {
                var details = new StringBuilder();
                details.AppendLine($"Заказ №{order.Id}");
                details.AppendLine($"Пользователь: {order.UserId}");
                details.AppendLine($"Дата: {order.DateTime}");
                details.AppendLine("\nТовары в заказе:");

                foreach (var item in order.Items)
                {
                    details.AppendLine($"- Товар {item.ProductId}: {item.Quantity} шт. x {item.Price:C} = {item.Quantity * item.Price:C}");
                }

                details.AppendLine($"\nИтого: {order.TotalPrice:C}");

                MessageBox.Show(details.ToString(), $"Детали заказа №{order.Id}");
            }
        }
    }
}
