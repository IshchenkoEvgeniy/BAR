using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Linq;
using System.IO;
using System.Linq; 
using BAR.Services;

namespace BAR.Model
{
    public class Order
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime DateTime { get; set; }
        public List<OrderItem> Items { get; set; }
        public decimal TotalPrice { get; set; }

        public Order()
        {
            Items = new List<OrderItem>();
        }
    }

    public class OrderItem
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class TestOrderWindow : Window
    {
        private readonly OrderService _orderService;

        public TestOrderWindow()
        {
            _orderService = new OrderService();

            Width = 300;
            Height = 200;
            Title = "Test Order Creation";

            var button = new System.Windows.Controls.Button
            {
                Content = "Create Test Order",
                Width = 150,
                Height = 30
            };
            button.Click += Button_Click;

            Content = button;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var testItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = "1", Quantity = 2, Price = 150.00m },
                    new OrderItem { ProductId = "2", Quantity = 1, Price = 120.00m }
                };

                _orderService.AddOrder("1", testItems);

                MessageBox.Show("Тестовый заказ успешно создан!", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании заказа: {ex.Message}", "Ошибка");
            }
        }
    }
}
