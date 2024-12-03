using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Xml.Linq;
using System.IO;
using BAR.Model.User;
using System.Windows.Controls;
using BAR.View;
using System.Linq;
using BAR.Services;

namespace BAR.ViewModel
{
    public class AuthorizationViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private string _email;
        
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand OpenRegistrationCommand { get; }

        public AuthorizationViewModel()
        {
            _userService = UserService.Instance;
            LoginCommand = new RelayCommand<PasswordBox>(Login);
            OpenRegistrationCommand = new RelayCommand(OpenRegistration);
        }

        private void Login(PasswordBox passwordBox)
        {
            if (string.IsNullOrEmpty(Email) || passwordBox == null || string.IsNullOrEmpty(passwordBox.Password))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            try
            {
                var user = AuthenticateUser(Email, passwordBox.Password);
                if (user != null)
                {
                    _userService.CurrentUser = user;
                    MessageBox.Show($"Добро пожаловать, {user.Name}!");
                    CloseCurrentWindow();
                }
                else
                {
                    MessageBox.Show("Неверный email или пароль!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}");
            }
        }

        private User AuthenticateUser(string email, string password)
        {
            string xmlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\users.xml");
            if (!File.Exists(xmlFile))
            {
                throw new Exception("База данных пользователей не найдена!");
            }

            var doc = XDocument.Load(xmlFile);
            var userElement = doc.Root?.Elements("User")
                .FirstOrDefault(u => 
                    u.Element("Email")?.Value == email && 
                    u.Element("PasswordHash")?.Value == password);

            if (userElement == null) return null;

            return userElement.Element("Type")?.Value == "Admin"
                ? new Admin
                {
                    Id = userElement.Element("Id")?.Value,
                    Name = userElement.Element("Name")?.Value,
                    Email = email
                }
                : new AccountUser
                {
                    Id = userElement.Element("Id")?.Value,
                    Name = userElement.Element("Name")?.Value,
                    Email = email,
                    BonusPoints = int.Parse(userElement.Element("BonusPoints")?.Value ?? "0")
                };
        }

        private void OpenRegistration()
        {
            new RegistrationView().Show();
            CloseCurrentWindow();
        }

        private void CloseCurrentWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
