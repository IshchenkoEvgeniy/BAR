using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using BAR.Model;

namespace BAR.Services
{
    public class OrderHistoryService
    {
        private static OrderHistoryService _instance;
        private readonly string _historyFilePath = "Data/orderHistory.xml";
        private List<OrderHistory> _orderHistory;

        public static OrderHistoryService Instance 
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = new OrderHistoryService();
                }
                return _instance;
            }
        }

        private OrderHistoryService()
        {
            _orderHistory = LoadOrderHistory();
        }

        public void AddOrder(string userId, List<CartItem> items, decimal totalAmount)
        {
            var order = new OrderHistory
            {
                UserId = userId,
                Items = new List<CartItem>(items),
                TotalAmount = totalAmount
            };

            _orderHistory.Add(order);
            SaveOrderHistory();
        }

        public List<OrderHistory> GetUserOrders(string userId)
        {
            return _orderHistory.Where(o => o.UserId == userId)
                              .OrderByDescending(o => o.OrderDate)
                              .ToList();
        }

        private List<OrderHistory> LoadOrderHistory()
        {
            if (!File.Exists(_historyFilePath))
            {
                Directory.CreateDirectory("Data");
                return new List<OrderHistory>();
            }

            try
            {
                var doc = XDocument.Load(_historyFilePath);
                return doc.Root.Elements("Order")
                    .Select(o => new OrderHistory
                    {
                        OrderId = o.Element("OrderId").Value,
                        OrderDate = DateTime.Parse(o.Element("OrderDate").Value),
                        UserId = o.Element("UserId").Value,
                        TotalAmount = decimal.Parse(o.Element("TotalAmount").Value),
                        Items = o.Element("Items").Elements("Item")
                            .Select(i => new CartItem
                            {
                                Id = i.Element("Id").Value,
                                Name = i.Element("Name").Value,
                                Price = decimal.Parse(i.Element("Price").Value),
                                Quantity = int.Parse(i.Element("Quantity").Value)
                            }).ToList()
                    }).ToList();
            }
            catch
            {
                return new List<OrderHistory>();
            }
        }

        private void SaveOrderHistory()
        {
            var doc = new XDocument(
                new XElement("OrderHistory",
                    _orderHistory.Select(o =>
                        new XElement("Order",
                            new XElement("OrderId", o.OrderId),
                            new XElement("OrderDate", o.OrderDate),
                            new XElement("UserId", o.UserId),
                            new XElement("TotalAmount", o.TotalAmount),
                            new XElement("Items",
                                o.Items.Select(i =>
                                    new XElement("Item",
                                        new XElement("Id", i.Id),
                                        new XElement("Name", i.Name),
                                        new XElement("Price", i.Price),
                                        new XElement("Quantity", i.Quantity)
                                    )
                                )
                            )
                        )
                    )
                )
            );

            doc.Save(_historyFilePath);
        }
    }
}
