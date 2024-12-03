using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BAR.Model
{
    public class CartItem : INotifyPropertyChanged
    {
        private int _quantity;
        private decimal _price;
        
        public string Id { get; set; }
        public string Name { get; set; }
        
        public decimal Price
        {
            get => _price;
            set
            {
                if (_price != value)
                {
                    _price = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Total));
                }
            }
        }
        
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Кількість не може бути від'ємною");
                    
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Total));
                }
            }
        }
        
        public decimal Total => Price * Quantity;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
