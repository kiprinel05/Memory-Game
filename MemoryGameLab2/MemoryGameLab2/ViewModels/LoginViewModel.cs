using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MemoryGameLab2.Models;
using MemoryGameLab2.Views;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Windows;

namespace MemoryGameLab2.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private ObservableCollection<User> _users;
        private User _selectedUser;
        private string _newUsername;
        private string _newUserImagePath;
        private bool _isDeleteEnabled;
        private bool _isPlayEnabled;

        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                IsDeleteEnabled = value != null;
                IsPlayEnabled = value != null;
                OnPropertyChanged();
            }
        }

        public string NewUsername
        {
            get => _newUsername;
            set
            {
                _newUsername = value;
                OnPropertyChanged();
            }
        }

        public string NewUserImagePath
        {
            get => _newUserImagePath;
            set
            {
                _newUserImagePath = value;
                OnPropertyChanged();
            }
        }

        public bool IsDeleteEnabled
        {
            get => _isDeleteEnabled;
            set
            {
                _isDeleteEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsPlayEnabled
        {
            get => _isPlayEnabled;
            set
            {
                _isPlayEnabled = value;
                OnPropertyChanged();
            }
        }

        public ICommand CreateUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand SelectImageCommand { get; }

        private readonly string _usersFilePath;
        private readonly string _imagesDirectory;

        public LoginViewModel()
        {
            _usersFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.json");
            _imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Users");
            
            if (!Directory.Exists(_imagesDirectory))
            {
                Directory.CreateDirectory(_imagesDirectory);
            }

            Users = new ObservableCollection<User>();
            LoadUsers();

            CreateUserCommand = new RelayCommand(CreateUser, CanCreateUser);
            DeleteUserCommand = new RelayCommand(DeleteUser, CanDeleteUser);
            PlayCommand = new RelayCommand(Play, CanPlay);
            SelectImageCommand = new RelayCommand(SelectImage);
        }

        private void LoadUsers()
        {
            if (File.Exists(_usersFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_usersFilePath);
                    var users = JsonConvert.DeserializeObject<ObservableCollection<User>>(json);
                    Users = users ?? new ObservableCollection<User>();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la încărcarea utilizatorilor: {ex.Message}");
                    Users = new ObservableCollection<User>();
                }
            }
        }

        private void SaveUsers()
        {
            try
            {
                var json = JsonConvert.SerializeObject(Users);
                File.WriteAllText(_usersFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la salvarea utilizatorilor: {ex.Message}");
            }
        }

        private void CreateUser(object parameter)
        {
            MessageBox.Show($"Nume: {NewUsername}, Imagine: {NewUserImagePath}");

            if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewUserImagePath))
            {
                MessageBox.Show("Te rog completează toate câmpurile!");
                return;
            }

            if (Users.Any(u => u.Username.Equals(NewUsername, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Acest nume de utilizator există deja!");
                return;
            }

            try
            {
                var imageFileName = $"{NewUsername}_{Path.GetFileName(NewUserImagePath)}";
                var destinationPath = Path.Combine(_imagesDirectory, imageFileName);
                File.Copy(NewUserImagePath, destinationPath, true);

                var user = new User(NewUsername, destinationPath);
                Users.Add(user);
                SaveUsers();

                NewUsername = string.Empty;
                NewUserImagePath = string.Empty;
                MessageBox.Show("Utilizator creat cu succes!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la crearea utilizatorului: {ex.Message}");
            }
        }

        private bool CanCreateUser(object parameter)
        {
            return !string.IsNullOrWhiteSpace(NewUsername) && 
                   !string.IsNullOrWhiteSpace(NewUserImagePath) &&
                   !Users.Any(u => u.Username.Equals(NewUsername, StringComparison.OrdinalIgnoreCase));
        }

        private void DeleteUser(object parameter)
        {
            if (SelectedUser == null) return;

            var result = MessageBox.Show($"Sigur doriți să ștergeți utilizatorul {SelectedUser.Username}?",
                                      "Confirmare ștergere",
                                      MessageBoxButton.YesNo,
                                      MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Ștergem doar din listă, fără a șterge fișierul imagine
                    Users.Remove(SelectedUser);
                    SaveUsers();
                    SelectedUser = null; // Resetăm selecția

                    MessageBox.Show("Utilizator șters cu succes din listă!",
                                  "Succes",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la ștergerea utilizatorului: {ex.Message}",
                                  "Eroare",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        private bool CanDeleteUser(object parameter)
        {
            return SelectedUser != null;
        }

        private void Play(object parameter)
        {
            var gameWindow = new GameWindow(SelectedUser);
            gameWindow.Show();
            Application.Current.MainWindow.Close();
        }

        private bool CanPlay(object parameter)
        {
            return SelectedUser != null;
        }

        private void SelectImage(object parameter)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png, *.gif)|*.jpg;*.jpeg;*.png;*.gif",
                Title = "Selectează imaginea utilizatorului"
            };

            if (dialog.ShowDialog() == true)
            {
                NewUserImagePath = dialog.FileName;
            }
        }
    }
} 