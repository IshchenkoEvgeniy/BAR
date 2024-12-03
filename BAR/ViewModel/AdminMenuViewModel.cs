using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BAR.Model;
using BAR.Services;
using System.ComponentModel;
using System.Windows;
using BAR.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;

namespace BAR.ViewModel
{
    public class AdminMenuViewModel : INotifyPropertyChanged
    {
        private readonly MenuService _menuService;
        private readonly UserService _userService;
        private readonly List<string> _menuFiles;
        private readonly List<MenuViewModel> _menuViewModels;
        private ObservableCollection<MenuItem> _menuItems;
        private MenuItem _selectedItem;
        private int _selectedMenuType;

        public event PropertyChangedEventHandler PropertyChanged;

        public AdminMenuViewModel(params string[] menuFiles)
        {
            _menuFiles = menuFiles.ToList();
            _menuService = MenuService.Instance;
            _userService = UserService.Instance;
            
            _menuViewModels = new List<MenuViewModel>();
            foreach (var menuFile in _menuFiles)
            {
                _menuViewModels.Add(new MenuViewModel(menuFile));
            }
            
            LoadMenuItems();
            InitializeCommands();
        }

        public int SelectedMenuType
        {
            get => _selectedMenuType;
            set
            {
                if (_selectedMenuType != value && value >= 0 && value < MenuCount)
                {
                    _selectedMenuType = value;
                    OnPropertyChanged(nameof(SelectedMenuType));
                    LoadMenuItems();
                }
            }
        }

        public int MenuCount => _menuFiles.Count;

        private void LoadMenuItems()
        {
            try
            {
                var currentViewModel = _menuViewModels[SelectedMenuType];
                MenuItems = new ObservableCollection<MenuItem>(currentViewModel.MenuItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження меню: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        public string AdminDisplayName => $"Адміністратор: {_userService.CurrentUser?.Name ?? "Невідомий"}";

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
                    Name = "Новий елемент",
                    Description = "Опис",
                    Price = 0,
                    Category = "Категорія",
                    IsAvailable = true
                };

                MenuItems.Add(newItem);
                SaveChanges();
                SelectedItem = newItem;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час додавання: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteItem()
        {
            if (SelectedItem == null) return;

            try
            {
                var result = MessageBox.Show($"Ви впевнені, що хочете видалити цей елемент?",
                    "Підтвердження видалення", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    MenuItems.Remove(SelectedItem);
                    SaveChanges();
                    SelectedItem = null;
                    MessageBox.Show("Елемент успішно видалено", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час видалення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var currentFileName = _menuFiles[SelectedMenuType];
                _menuService.SaveMenuItems(currentFileName);
                
                // Update the corresponding view model
                _menuViewModels[SelectedMenuType].MenuItems = new ObservableCollection<MenuItem>(MenuItems);

                MessageBox.Show("Зміни збережено успішно!", "Збереження", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час збереження: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Помилка під час виходу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
