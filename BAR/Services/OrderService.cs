using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using BAR.Model;
using System.Globalization;
namespace BAR.Services
{
    
    public class OrderService
    {
        
        private readonly string _dataPath;
        private readonly string _ordersPath;
        private static int _lastOrderId = 0;

       
        public OrderService()
        {
           
            _dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Data");
            _ordersPath = Path.Combine(_dataPath, "Orders.xml");
           
            InitializeLastOrderId();
        }

        
        private void InitializeLastOrderId()
        {
            
            if (File.Exists(_ordersPath))
            {
               
                var doc = XDocument.Load(_ordersPath);
                
                var lastOrder = doc.Descendants("Order")
                    .Select(o => int.Parse(o.Element("Id").Value))
                    .DefaultIfEmpty(0)
                    .Max();
               
                _lastOrderId = lastOrder;
            }
        }

        
        private void EnsureDirectoryExists()
        {
            
            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }
        }

        
        private XDocument GetOrCreateXmlDocument()
        {
            
            EnsureDirectoryExists();

            
            if (!File.Exists(_ordersPath))
            {
                var doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement("Orders")
                );
                doc.Save(_ordersPath);
                return doc;
            }

            
            return XDocument.Load(_ordersPath);
        }

        
        public void AddOrder(string userId, List<OrderItem> items)
        {
            
            var xmlDoc = GetOrCreateXmlDocument();
            
            _lastOrderId++;

           
            var order = new Order
            {
                Id = _lastOrderId.ToString(),
                UserId = userId,
                DateTime = DateTime.Now,
                Items = items,
                TotalPrice = items.Sum(item => item.Price * item.Quantity)
            };

            
            var orderElement = CreateOrderElement(order);
            
            xmlDoc.Element("Orders").Add(orderElement);
            
            xmlDoc.Save(_ordersPath);
        }

        
        private XElement CreateOrderElement(Order order)
        {
            return new XElement("Order",
                new XElement("Id", order.Id),
                new XElement("UserId", order.UserId),
                new XElement("DateTime", order.DateTime.ToString("O")),
                new XElement("Items",
                    order.Items.Select(item =>
                        new XElement("OrderItem",
                            new XElement("ProductId", item.ProductId),
                            new XElement("Quantity", item.Quantity.ToString(CultureInfo.InvariantCulture)),
                            new XElement("Price", item.Price.ToString(CultureInfo.InvariantCulture))
                        )
                    )
                ),
                new XElement("TotalPrice", order.TotalPrice.ToString(CultureInfo.InvariantCulture))
            );
        }

        
        public List<Order> GetAllOrders()
        {
            
            if (!File.Exists(_ordersPath))
                return new List<Order>();

            
            var doc = XDocument.Load(_ordersPath);
            
            return doc.Descendants("Order").Select(ParseOrderElement).ToList();
        }

        
        private Order ParseOrderElement(XElement orderElement)
        {
            return new Order
            {
                Id = orderElement.Element("Id").Value,
                UserId = orderElement.Element("UserId").Value,
                DateTime = DateTime.Parse(orderElement.Element("DateTime").Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                Items = orderElement.Element("Items")
                    .Elements("OrderItem")
                    .Select(item => new OrderItem
                    {
                        ProductId = item.Element("ProductId").Value,
                        Quantity = int.Parse(item.Element("Quantity").Value, CultureInfo.InvariantCulture),
                        Price = decimal.Parse(item.Element("Price").Value, CultureInfo.InvariantCulture)
                    })
                    .ToList(),
                TotalPrice = decimal.Parse(orderElement.Element("TotalPrice").Value, CultureInfo.InvariantCulture)
            };
        }
    }
}
