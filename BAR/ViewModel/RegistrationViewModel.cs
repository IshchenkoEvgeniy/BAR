using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Xml.Linq;
using System.IO;
using System.Text.RegularExpressions;
using BAR.Model.User;
using System.Windows.Controls;
using BAR.View;
using System.Linq;

namespace BAR.ViewModel
{
    public class RegistrationViewModel : INotifyPropertyChanged
    {
        private string _name;
        private string _email;
        private bool _isAdmin;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public bool IsAdmin
        {
            get => _isAdmin;
            set
            {
                _isAdmin = value;
                OnPropertyChanged(nameof(IsAdmin));
            }
        }

        public ICommand RegisterCommand { get; private set; }
        public ICommand BackToLoginCommand { get; private set; }

        public RegistrationViewModel()
        {
            RegisterCommand = new RelayCommand<PasswordBox>(Register);
            BackToLoginCommand = new RelayCommand(BackToLogin);
        }

        private bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        private void Register(PasswordBox passwordBox)
        {
            if (passwordBox == null) return;

            var password = passwordBox.Password;
            var confirmPasswordBox = (passwordBox.Parent as StackPanel)?.Children
                .OfType<PasswordBox>()
                .FirstOrDefault(p => p.Name == "ConfirmPasswordBox");

            if (confirmPasswordBox == null) return;
            var confirmPassword = confirmPasswordBox.Password;

            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Заповніть усі поля!");
                return;
            }

            if (!IsValidEmail(Email))
            {
                MessageBox.Show("Введіть коректний email!");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Паролі не збігаються!");
                return;
            }

            try
            {
                SaveUserToXml(password);
                MessageBox.Show("Реєстрація успішна!");
                BackToLogin();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час реєстрації: {ex.Message}");
            }
        }

        private void SaveUserToXml(string password)
        {
            string projectPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            string xmlFile = Path.Combine(projectPath, "Data", "users.xml");
            string dataFolder = Path.GetDirectoryName(xmlFile);

            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }

            XDocument doc;
            if (File.Exists(xmlFile))
            {
                doc = XDocument.Load(xmlFile);

                var existingUser = doc.Root.Elements("User")
                    .FirstOrDefault(u => u.Element("Email").Value == Email);

                if (existingUser != null)
                {
                    throw new Exception("Користувач з таким email вже існує!");
                }
            }
            else
            {
                doc = new XDocument(new XElement("Users"));
            }

            XElement userElement = new XElement("User",
                new XElement("Id", Guid.NewGuid().ToString()),
                new XElement("Name", Name),
                new XElement("Email", Email),
                new XElement("PasswordHash", password),
                new XElement("Type", IsAdmin ? "Admin" : "AccountUser"),
                new XElement("BonusPoints", "0")
            );

            doc.Root.Add(userElement);
            doc.Save(xmlFile);
        }

        private void BackToLogin()
        {
            var loginWindow = new AuthorizationView();
            loginWindow.Show();
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

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) =>
            parameter is T typedParameter && (_canExecute?.Invoke(typedParameter) ?? true);

        public void Execute(object parameter)
        {
            if (parameter is T typedParameter)
            {
                _execute(typedParameter);
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();
    }
}