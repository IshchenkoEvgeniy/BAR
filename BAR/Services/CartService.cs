using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using BAR.Model;
using BAR.Model.User;

namespace BAR.Services
{
    public class CartService : INotifyPropertyChanged
    {
        private static CartService _instance;
        private readonly ObservableCollection<CartItem> _items;
        private readonly string _cartDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BAR",
            "Carts");
        
        public static CartService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CartService();
                }
                return _instance;
            }
        }
        
        public ObservableCollection<CartItem> Items => _items;

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            private set
            {
                if (_totalAmount != value)
                {
                    _totalAmount = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _totalItems;
        public int TotalItems
        {
            get => _totalItems;
            private set
            {
                if (_totalItems != value)
                {
                    _totalItems = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private CartService()
        {
            _items = new ObservableCollection<CartItem>();
            _items.CollectionChanged += Items_CollectionChanged;
            
            UserService.Instance.CurrentUserChanged += UserService_CurrentUserChanged;
            LoadCartForCurrentUser();
        }

        private void UserService_CurrentUserChanged(object sender, User e)
        {
            _items.Clear();
            if (e != null && !(e is Guest) && !(e is Admin))
            {
                LoadCartForCurrentUser();
            }
        }

        private string GetCartFilePath()
        {
            var user = UserService.Instance.CurrentUser;
            if (user == null || user is Guest || user is Admin)
                return null;
                
            var accountUser = user as AccountUser;
            if (accountUser == null)
                return null;
                
            return Path.Combine(_cartDirectory, $"cart_{accountUser.Email}.xml");
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CartItem item in e.NewItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CartItem item in e.OldItems)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }

            UpdateTotals();
            SaveCartForCurrentUser();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartItem.Quantity) || e.PropertyName == nameof(CartItem.Price))
            {
                UpdateTotals();
                SaveCartForCurrentUser();
            }
        }

        private void UpdateTotals()
        {
            TotalAmount = _items.Sum(item => item.Total);
            TotalItems = _items.Sum(item => item.Quantity);
        }
        
        public void AddItem(CartItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var existingItem = _items.FirstOrDefault(i => i.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                _items.Add(item);
            }
        }
        
        public void RemoveItem(string itemId)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                _items.Remove(item);
            }
        }

        public void UpdateQuantity(string itemId, int quantity)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                try
                {
                    if (quantity <= 0)
                        _items.Remove(item);
                    else
                        item.Quantity = quantity;
                }
                catch (ArgumentException)
                {
                    // Игнорируем некорректные значения количества
                }
            }
        }

        public void IncrementQuantity(string itemId)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                try
                {
                    item.Quantity++;
                }
                catch (ArgumentException)
                {
                    // Игнорируем переполнение
                }
            }
        }

        public void DecrementQuantity(string itemId)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                if (item.Quantity <= 1)
                    _items.Remove(item);
                else
                    item.Quantity--;
            }
        }
        
        public void Clear()
        {
            _items.Clear();
        }

        private void SaveCartForCurrentUser()
        {
            var cartPath = GetCartFilePath();
            if (cartPath == null)
                return;

            try
            {
                if (!Directory.Exists(_cartDirectory))
                    Directory.CreateDirectory(_cartDirectory);

                var serializer = new XmlSerializer(typeof(CartItem[]));
                using (var writer = new StreamWriter(cartPath))
                {
                    serializer.Serialize(writer, _items.ToArray());
                }
            }
            catch (Exception)
            {
                // Логируем ошибку сохранения
            }
        }

        private void LoadCartForCurrentUser()
        {
            var cartPath = GetCartFilePath();
            if (cartPath == null)
                return;

            try
            {
                if (File.Exists(cartPath))
                {
                    var serializer = new XmlSerializer(typeof(CartItem[]));
                    using (var reader = new StreamReader(cartPath))
                    {
                        var items = (CartItem[])serializer.Deserialize(reader);
                        foreach (var item in items)
                        {
                            _items.Add(item);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Логируем ошибку загрузки
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
