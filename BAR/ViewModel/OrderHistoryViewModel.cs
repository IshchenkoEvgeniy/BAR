using System.Collections.ObjectModel;
using System.ComponentModel;
using BAR.Model;
using BAR.Services;

namespace BAR.ViewModel
{
    public class OrderHistoryViewModel : INotifyPropertyChanged
    {
        private readonly OrderHistoryService _orderHistoryService;
        private readonly UserService _userService;
        private ObservableCollection<OrderHistory> _orders;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<OrderHistory> Orders
        {
            get => _orders;
            set
            {
                _orders = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Orders)));
            }
        }

        public OrderHistoryViewModel()
        {
            _orderHistoryService = OrderHistoryService.Instance;
            _userService = UserService.Instance;
            LoadOrders();
        }

        private void LoadOrders()
        {
            var userId = _userService.CurrentUser?.Id;
            if (userId != null)
            {
                var orders = _orderHistoryService.GetUserOrders(userId);
                Orders = new ObservableCollection<OrderHistory>(orders);
            }
        }
    }
}
