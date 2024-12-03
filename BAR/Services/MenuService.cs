using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Windows;
using BAR.Model;

namespace BAR.Services
{
    public class MenuService
    {
        private static MenuService _instance;
        private readonly Dictionary<string, List<MenuItem>> _allMenus;

        private MenuService()
        {
            _allMenus = new Dictionary<string, List<MenuItem>>();
            LoadMenuItems("menu.xml");
        }

        public static MenuService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MenuService();
                }
                return _instance;
            }
        }

        public List<MenuItem> GetMenuItems(string menuName)
        {
            if (_allMenus.ContainsKey(menuName))
                return _allMenus[menuName];
            return new List<MenuItem>();
        }

        public void LoadMenuItems(string menuFileName)
        {
            try
            {
                string projectPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                string menuPath = Path.Combine(projectPath, "Data", menuFileName);
                
                if (!File.Exists(menuPath))
                {
                    MessageBox.Show($"Файл меню {menuFileName} не знайдено!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                XDocument doc = XDocument.Load(menuPath);
                var items = from item in doc.Descendants("MenuItem")
                           select new MenuItem
                           {
                               Id = item.Element("Id")?.Value,
                               Name = item.Element("Name")?.Value,
                               Description = item.Element("Description")?.Value,
                               Price = decimal.Parse(item.Element("Price")?.Value ?? "0"),
                               Category = item.Element("Category")?.Value,
                               IsAvailable = bool.Parse(item.Element("IsAvailable")?.Value ?? "true")
                           };

                var menuList = items.ToList();
                string menuKey = Path.GetFileNameWithoutExtension(menuFileName);
                
                if (_allMenus.ContainsKey(menuKey))
                    _allMenus[menuKey] = menuList;
                else
                    _allMenus.Add(menuKey, menuList);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час завантаження меню {menuFileName}: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SaveMenuItems(string menuFileName)
        {
            try
            {
                string menuKey = Path.GetFileNameWithoutExtension(menuFileName);
                
                string projectPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                string menuPath = Path.Combine(projectPath, "Data", menuFileName);

                var doc = new XDocument(
                    new XElement("Menu",
                        from item in _allMenus[menuKey]
                        select new XElement("MenuItem",
                            new XElement("Id", item.Id),
                            new XElement("Name", item.Name),
                            new XElement("Description", item.Description),
                            new XElement("Price", item.Price),
                            new XElement("Category", item.Category),
                            new XElement("IsAvailable", item.IsAvailable)
                        )
                    )
                );

                doc.Save(menuPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час збереження меню {menuFileName}: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UpdateMenuItems(string menuFileName, List<MenuItem> items)
        {
            string menuKey = Path.GetFileNameWithoutExtension(menuFileName);
            _allMenus[menuKey] = items;
            SaveMenuItems(menuFileName);
        }

        public void AddItem(string menuName, MenuItem item)
        {
            if (string.IsNullOrEmpty(item.Id))
            {
                item.Id = Guid.NewGuid().ToString();
            }
            if (_allMenus.ContainsKey(menuName))
            {
                _allMenus[menuName].Add(item);
            }
            else
            {
                _allMenus.Add(menuName, new List<MenuItem> { item });
            }
            SaveMenuItems($"{menuName}.xml");
        }

        public void UpdateItem(string menuName, MenuItem item)
        {
            if (_allMenus.ContainsKey(menuName))
            {
                var existingItem = _allMenus[menuName].FirstOrDefault(x => x.Id == item.Id);
                if (existingItem != null)
                {
                    existingItem.Name = item.Name;
                    existingItem.Description = item.Description;
                    existingItem.Price = item.Price;
                    existingItem.Category = item.Category;
                    existingItem.IsAvailable = item.IsAvailable;
                    SaveMenuItems($"{menuName}.xml");
                }
            }
        }

        public void DeleteItem(string menuName, string id)
        {
            if (_allMenus.ContainsKey(menuName))
            {
                var item = _allMenus[menuName].FirstOrDefault(x => x.Id == id);
                if (item != null)
                {
                    _allMenus[menuName].Remove(item);
                    SaveMenuItems($"{menuName}.xml");
                }
            }
        }
    }
}
