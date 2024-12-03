using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using BAR.Commands;
using BAR.Services;

namespace BAR.ViewModel
{
    public class TransactionViewModel : INotifyPropertyChanged
    {
        private readonly CartService _cartService = CartService.Instance;
        private string _cardNumber;
        private string _month;
        private string _year;
        private string _cvv;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CardNumber
        {
            get => _cardNumber;
            set
            {
                _cardNumber = value;
                OnPropertyChanged(nameof(CardNumber));
            }
        }

        public string Month
        {
            get => _month;
            set
            {
                _month = value;
                OnPropertyChanged(nameof(Month));
            }
        }

        public string Year
        {
            get => _year;
            set
            {
                _year = value;
                OnPropertyChanged(nameof(Year));
            }
        }

        public string CVV
        {
            get => _cvv;
            set
            {
                _cvv = value;
                OnPropertyChanged(nameof(CVV));
            }
        }

        public ICommand ConfirmCommand { get; }

        public TransactionViewModel()
        {
            ConfirmCommand = new RelayCommand<object>(ExecuteConfirm);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ExecuteConfirm(object parameter)
        {
            if (!ValidateCardData())
                return;

            MessageBox.Show("Оплата прошла успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            
            // Очищаем корзину после успешной оплаты
            _cartService.Clear();
            
            if (parameter is Window window)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        private bool ValidateCardData()
        {
            // Проверка номера карты (16 цифр)
            if (string.IsNullOrEmpty(CardNumber) || CardNumber.Length != 16)
            {
                MessageBox.Show("Номер карты должен содержать 16 цифр", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Проверка месяца (1-12)
            if (!int.TryParse(Month, out int month) || month < 1 || month > 12)
            {
                MessageBox.Show("Неверный месяц", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Проверка года (больше текущего)
            if (!int.TryParse(Year, out int year) || year < DateTime.Now.Year % 100)
            {
                MessageBox.Show("Неверный год", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Проверка CVV (3 цифры)
            if (string.IsNullOrEmpty(CVV) || CVV.Length != 3)
            {
                MessageBox.Show("CVV должен содержать 3 цифры", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
    }
}
