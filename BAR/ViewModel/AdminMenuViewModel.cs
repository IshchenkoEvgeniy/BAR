using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BAR.Model;
using BAR.Services;
using System.ComponentModel;
using System.Windows;
using BAR.Commands;
using System.Collections.Specialized;

namespace BAR.ViewModel
{
    public class AdminMenuViewModel : INotifyPropertyChanged
    {
        private readonly MenuService _menuService;
        private readonly UserService _userService;
        private readonly MenuViewModel _menuViewModel;
        private readonly MenuViewModel _promotionsViewModel;
        private ObservableCollection<MenuItem> _menuItems;
        private MenuItem _selectedItem;
        private int _selectedMenuType;
        private readonly string _menuFileName;
        private readonly string _promotionsFileName;

        public event PropertyChangedEventHandler PropertyChanged;

        public AdminMenuViewModel(string menuFileName, string promotionsFileName)
        {
            _menuFileName = menuFileName;
            _promotionsFileName = promotionsFileName;
            _menuService = MenuService.Instance;
            _userService = UserService.Instance;
            _menuViewModel = new MenuViewModel(_menuFileName);
            _promotionsViewModel = new MenuViewModel(_promotionsFileName);
            LoadMenuItems();
            InitializeCommands();
        }

        public int SelectedMenuType
        {
            get => _selectedMenuType;
            set
            {
                if (_selectedMenuType != value)
                {
                    _selectedMenuType = value;
                    OnPropertyChanged(nameof(SelectedMenuType));
                    LoadMenuItems();
                }
            }
        }

        private void LoadMenuItems()
        {
            try
            {
                var currentViewModel = SelectedMenuType == 0 ? _menuViewModel : _promotionsViewModel;
                MenuItems = new ObservableCollection<MenuItem>(currentViewModel.MenuItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки меню: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public ObservableCollection<MenuItem> MenuItems
        {
            get => _menuItems;
            set
            {
                _menuItems = value;
                OnPropertyChanged(nameof(MenuItems));
            }
        }

        public MenuItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string AdminDisplayName => $"Администратор: {_userService.CurrentUser?.Name ?? "Неизвестный"}";

        #region Commands
        public ICommand AddItemCommand { get; private set; }
        public ICommand DeleteItemCommand { get; private set; }
        public ICommand SaveChangesCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }

        private void InitializeCommands()
        {
            AddItemCommand = new RelayCommand(AddItem);
            DeleteItemCommand = new RelayCommand(DeleteItem, CanDeleteItem);
            SaveChangesCommand = new RelayCommand(SaveChanges);
            LogoutCommand = new RelayCommand(Logout);
        }
        #endregion

        private void AddItem()
        {
            try
            {
                var newItem = new MenuItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = SelectedMenuType == 0 ? "Новый напиток" : "Новая акция",
                    Description = "Описание",
                    Price = 0,
                    Category = SelectedMenuType == 0 ? "Напитки" : "Акции",
                    IsAvailable = true
                };

                MenuItems.Add(newItem);
                SaveChanges();
                SelectedItem = newItem;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteItem()
        {
            if (SelectedItem == null) return;

            try
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить этот {(SelectedMenuType == 0 ? "напиток" : "акцию")}?", 
                    "Подтверждение удаления", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    MenuItems.Remove(SelectedItem);
                    SaveChanges();
                    SelectedItem = null;
                    MessageBox.Show("Элемент успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanDeleteItem()
        {
            return SelectedItem != null;
        }

        private void SaveChanges()
        {
            try
            {
                var currentFileName = SelectedMenuType == 0 ? _menuFileName : _promotionsFileName;
                _menuService.SaveMenuItems(currentFileName);
                
                // Update the corresponding view model
                if (SelectedMenuType == 0)
                    _menuViewModel.MenuItems = new ObservableCollection<MenuItem>(MenuItems);
                else
                    _promotionsViewModel.MenuItems = new ObservableCollection<MenuItem>(MenuItems);

                MessageBox.Show("Изменения сохранены успешно!", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Logout()
        {
            try
            {
                _userService.Logout();
                var mainWindow = new MainWindow();
                mainWindow.Show();
                Application.Current.Windows[0].Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выходе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
