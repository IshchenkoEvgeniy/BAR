using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BAR.Commands;
using BAR.Model;
using BAR.Services;
using BAR.View;

namespace BAR.ViewModel
{
    public class MenuViewModel : INotifyPropertyChanged
    {
        private readonly MenuService _menuService;
        private readonly CartService _cartService;
        private ObservableCollection<MenuItem> _menuItems;
        private string _currentMenu;

        public ICommand AddToCartCommand { get; }
        public ICommand OpenCartCommand { get; }

        public MenuViewModel(string menuFile = "menu.xml")
        {
            _menuService = MenuService.Instance;
            _cartService = CartService.Instance;
            _currentMenu = System.IO.Path.GetFileNameWithoutExtension(menuFile);
            
            AddToCartCommand = new RelayCommand<MenuItem>(AddToCart);
            OpenCartCommand = new RelayCommand(OpenCart);
            
            LoadMenu(menuFile);
        }

        private void AddToCart(MenuItem menuItem)
        {
            var cartItem = new CartItem
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Price = menuItem.Price,
                Quantity = 1
            };
            _cartService.AddItem(cartItem);
        }

        private void OpenCart()
        {
            var cartView = new CartView();
            cartView.Show();
        }

        public ObservableCollection<MenuItem> MenuItems
        {
            get => _menuItems;
            set
            {
                _menuItems = value;
                OnPropertyChanged();
            }
        }

        public void LoadMenu(string menuFile)
        {
            _menuService.LoadMenuItems(menuFile);
            _currentMenu = System.IO.Path.GetFileNameWithoutExtension(menuFile);
            MenuItems = new ObservableCollection<MenuItem>(_menuService.GetMenuItems(_currentMenu));
        }

        public void AddItem(MenuItem item)
        {
            _menuService.AddItem(_currentMenu, item);
            LoadMenu($"{_currentMenu}.xml");
        }

        public void UpdateItem(MenuItem item)
        {
            _menuService.UpdateItem(_currentMenu, item);
            LoadMenu($"{_currentMenu}.xml");
        }

        public void DeleteItem(string id)
        {
            _menuService.DeleteItem(_currentMenu, id);
            LoadMenu($"{_currentMenu}.xml");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
