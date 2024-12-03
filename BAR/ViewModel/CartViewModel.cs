using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using BAR.Commands;
using BAR.Model;
using BAR.Services;

namespace BAR.ViewModel
{
    public class CartViewModel : INotifyPropertyChanged
    {
        private readonly CartService _cartService = CartService.Instance;
        private decimal _totalAmount;
        
        public ObservableCollection<CartItem> Items => _cartService.Items;
        
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
        
        public int TotalItems => _cartService.TotalItems;
        
        public bool HasItems => TotalItems > 0;

        public ICommand IncrementCommand { get; }
        public ICommand DecrementCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand CheckoutCommand { get; }

        public CartViewModel()
        {
            IncrementCommand = new RelayCommand<string>(IncrementQuantity);
            DecrementCommand = new RelayCommand<string>(DecrementQuantity);
            RemoveItemCommand = new RelayCommand<string>(RemoveItem);
            ClearCartCommand = new RelayCommand(ClearCart);
            CheckoutCommand = new RelayCommand(Checkout, CanCheckout);
            
            _cartService.PropertyChanged += CartService_PropertyChanged;
            TotalAmount = _cartService.TotalAmount;
        }

        private void CartService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CartService.TotalAmount):
                    TotalAmount = _cartService.TotalAmount;
                    break;
                case nameof(CartService.TotalItems):
                    OnPropertyChanged(nameof(TotalItems));
                    OnPropertyChanged(nameof(HasItems));
                    break;
            }
        }

        private void IncrementQuantity(string itemId)
        {
            _cartService.IncrementQuantity(itemId);
        }

        private void DecrementQuantity(string itemId)
        {
            _cartService.DecrementQuantity(itemId);
        }

        private void RemoveItem(string itemId)
        {
            if (MessageBox.Show(
                "Ви впевнені, що хочете видалити цей товар із кошика?",
                "Підтвердження видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _cartService.RemoveItem(itemId);
            }
        }

        private void ClearCart()
        {
            if (MessageBox.Show(
                "Ви впевнені, що хочете очистити кошик?",
                "Підтвердження очищення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _cartService.Clear();
            }
        }

        private bool CanCheckout()
        {
            return HasItems;
        }

        private void Checkout()
        {
            // TODO: Реализовать оформление заказа
            MessageBox.Show(
                $"Замовлення на суму {TotalAmount:C} оформлений!",
                "Замовлення оформлено",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            
            _cartService.Clear();
        }

        public void UpdateTotalAmount(decimal newAmount)
        {
            TotalAmount = newAmount;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
