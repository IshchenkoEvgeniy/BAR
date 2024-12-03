using System;
using BAR.Model.User;

namespace BAR.Services
{
    public class UserService
    {
        private static UserService _instance;
        private User _currentUser;
        public event EventHandler<User> CurrentUserChanged;

        private UserService()
        {
            _currentUser = new Guest();
        }

        public static UserService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UserService();
                return _instance;
            }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser is Guest && value != null)
                {
                    CartService.Instance.Clear(); // Очищаем корзину гостя при входе
                }
                _currentUser = value;
                CurrentUserChanged?.Invoke(this, _currentUser);
            }
        }

        public void Logout()
        {
            CartService.Instance.Clear(); // Очищаем корзину при выходе
            CurrentUser = new Guest();
        }
    }
}
