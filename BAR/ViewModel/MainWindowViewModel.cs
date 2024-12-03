using BAR.Commands;
using BAR.Model.User;
using BAR.Services;
using BAR.View;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace BAR.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private string _currentUserText;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CurrentUserText
        {
            get => _currentUserText;
            set
            {
                _currentUserText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentUserText)));
            }
        }

        public ICommand AccountCommand { get; }
        public ICommand OpenDrinksMenuCommand { get; }
        public ICommand OpenFoodMenuCommand { get; }
        public ICommand OpenCartCommand { get; }
        public ICommand OpenOrderHistoryCommand { get; }
        public ICommand LogOutCommand { get; }

        public MainWindowViewModel()
        {
            _userService = UserService.Instance;
            _userService.CurrentUserChanged += UserService_CurrentUserChanged;
            
            AccountCommand = new RelayCommand(ExecuteAccount);
            OpenDrinksMenuCommand = new RelayCommand(ExecuteOpenDrinksMenu);
            OpenFoodMenuCommand = new RelayCommand(ExecuteOpenFoodMenu);
            OpenCartCommand = new RelayCommand(ExecuteOpenCart);
            OpenOrderHistoryCommand = new RelayCommand(ExecuteOpenOrderHistory);
            LogOutCommand = new RelayCommand(ExecuteLogOut);

            UpdateUserDisplay();
        }

        private void UserService_CurrentUserChanged(object sender, User user)
        {
            UpdateUserDisplay();
        }

        private void UpdateUserDisplay()
        {
            if (_userService.CurrentUser is Admin)
                CurrentUserText = $"Администратор: {_userService.CurrentUser.Name}";
            else if (_userService.CurrentUser is AccountUser)
                CurrentUserText = $"Пользователь: {_userService.CurrentUser.Name}";
            else
                CurrentUserText = "Гость";
        }

        private void ExecuteAccount()
        {
            if (_userService.CurrentUser is Guest)
            {
                var authWindow = new AuthorizationView();
                authWindow.Show();
            }
            else
            {
                MessageBox.Show($"Текущий пользователь: {CurrentUserText}\nТип: {_userService.CurrentUser.GetType().Name}",
                    "Информация об аккаунте", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteOpenDrinksMenu()
        {
            if (_userService.CurrentUser is Admin)
            {
                var adminViewModel = new AdminMenuViewModel("menu.xml", "menu2.xml");
                var adminMenuWindow = new AdminMenuView();
                adminMenuWindow.DataContext = adminViewModel;
                adminMenuWindow.Show();
            }
            else
            {
                var menuWindow = new MenuView();
                menuWindow.DataContext = new MenuViewModel("menu.xml");
                menuWindow.Show();
            }
        }

        private void ExecuteOpenFoodMenu()
        {
            if (_userService.CurrentUser is Guest)
            {
                MessageBox.Show("Для доступа к акционным предложениям необходимо зарегистрироваться!", 
                    "Требуется регистрация", MessageBoxButton.OK, MessageBoxImage.Information);
                var authWindow = new AuthorizationView();
                authWindow.Show();
                return;
            }

            if (_userService.CurrentUser is Admin)
            {
                var adminViewModel = new AdminMenuViewModel("menu2.xml", "menu.xml");
                var adminMenuWindow = new AdminMenuView();
                adminMenuWindow.DataContext = adminViewModel;
                adminMenuWindow.Show();
            }
            else
            {
                var menuWindow = new MenuView();
                menuWindow.DataContext = new MenuViewModel("menu2.xml");
                menuWindow.Show();
            }
        }

        private void ExecuteOpenCart()
        {
            var cartWindow = new CartView();
            cartWindow.DataContext = new CartViewModel();
            cartWindow.Show();
        }

        private void ExecuteOpenOrderHistory()
        {
            if (_userService.CurrentUser is Guest)
            {
                MessageBox.Show("Для просмотра истории заказов необходимо авторизоваться!", 
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                var authWindow = new AuthorizationView();
                authWindow.Show();
                return;
            }

            var historyWindow = new OrderHistoryView();
            historyWindow.DataContext = new OrderHistoryViewModel();
            historyWindow.Show();
        }

        private void ExecuteLogOut()
        {
            if (MessageBox.Show("Вы действительно хотите выйти?", 
                "Подтверждение выхода", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _userService.CurrentUser = new Guest();
                MessageBox.Show("Вы успешно вышли из системы!", 
                    "Выход", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
