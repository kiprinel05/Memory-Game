using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MemoryGameLab2.Models;
using MemoryGameLab2.Views;
using System.Timers;
using System.Windows;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Win32;

namespace MemoryGameLab2.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private ObservableCollection<GameCard> _cards;
        private GameCard _firstSelectedCard;
        private GameCard _secondSelectedCard;
        private int _timeRemaining;
        private bool _isGameActive;
        private string _selectedCategory;
        private GameSettings _settings;
        private User _currentUser;
        private List<GameCategory> _categories;
        private bool _isStandardMode;
        private int _customRows;
        private int _customColumns;

        public ObservableCollection<GameCard> Cards
        {
            get => _cards;
            set
            {
                _cards = value;
                OnPropertyChanged();
            }
        }

        public int TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                _timeRemaining = value;
                OnPropertyChanged();
            }
        }

        public bool IsGameActive
        {
            get => _isGameActive;
            set
            {
                _isGameActive = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }

        public GameSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public bool IsStandardMode
        {
            get => _isStandardMode;
            set
            {
                _isStandardMode = value;
                OnPropertyChanged();
            }
        }

        public int CustomRows
        {
            get => _customRows;
            set
            {
                _customRows = value;
                OnPropertyChanged();
            }
        }

        public int CustomColumns
        {
            get => _customColumns;
            set
            {
                _customColumns = value;
                OnPropertyChanged();
            }
        }

        public ICommand CardClickCommand { get; private set; }
        public ICommand NewGameCommand { get; private set; }
        public ICommand SaveGameCommand { get; private set; }
        public ICommand OpenGameCommand { get; private set; }
        public ICommand StatisticsCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand CategorySelectedCommand { get; private set; }
        public ICommand ModeSelectedCommand { get; private set; }
        public ICommand ShowAboutCommand { get; private set; }

        private System.Timers.Timer _gameTimer;
        private readonly string _savesDirectory;
        private readonly string _statisticsFilePath;

        public GameViewModel(User currentUser)
        {
            _currentUser = currentUser;
            Cards = new ObservableCollection<GameCard>();
            Settings = new GameSettings(4, 4, 300, "Obiecte");
            SelectedCategory = "Obiecte";
            IsStandardMode = true;
            CustomRows = 4;
            CustomColumns = 4;

            _savesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");
            _statisticsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statistics.json");

            if (!Directory.Exists(_savesDirectory))
            {
                Directory.CreateDirectory(_savesDirectory);
            }

            InitializeCategories();
            InitializeCommands();
            InitializeGame();
            StartNewGame(this);
        }

        private void InitializeCategories()
        {
            var imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Game");
            _categories = new List<GameCategory>
            {
                //new GameCategory("Animale", Directory.GetFiles(Path.Combine(imagesDirectory, "Animale"), "*.png").ToList()),
                //new GameCategory("Fructe", Directory.GetFiles(Path.Combine(imagesDirectory, "Fructe"), "*.png").ToList()),
                new GameCategory("Obiecte", Directory.GetFiles(Path.Combine(imagesDirectory, "Obiecte"), "*.png").ToList())
            };
        }

        private void InitializeCommands()
        {
            CardClickCommand = new RelayCommand(OnCardClick);
            NewGameCommand = new RelayCommand(StartNewGame);
            SaveGameCommand = new RelayCommand(SaveGame);
            OpenGameCommand = new RelayCommand(OpenGame);
            StatisticsCommand = new RelayCommand(ShowStatistics);
            ExitCommand = new RelayCommand(ExitGame);
            CategorySelectedCommand = new RelayCommand(OnCategorySelected);
            ModeSelectedCommand = new RelayCommand(OnModeSelected);
            ShowAboutCommand = new RelayCommand(ShowAbout);
        }

        private void InitializeGame()
        {
            _gameTimer = new System.Timers.Timer(1000);
            _gameTimer.Elapsed += GameTimer_Elapsed;
        }

        private void GameTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (TimeRemaining > 0)
            {
                TimeRemaining--;
                if (TimeRemaining == 0)
                {
                    _gameTimer.Stop();
                    IsGameActive = false;
                    MessageBox.Show("Timpul a expirat! Jocul s-a terminat.");
                }
            }
        }

        private void OnCardClick(object parameter)
        {
            if (!IsGameActive || parameter is not GameCard card || card.IsMatched || card.IsFlipped)
                return;

            if (_firstSelectedCard == null)
            {
                _firstSelectedCard = card;
                card.IsFlipped = true;
            }
            else if (_secondSelectedCard == null)
            {
                _secondSelectedCard = card;
                card.IsFlipped = true;

                if (_firstSelectedCard.ImagePath == _secondSelectedCard.ImagePath)
                {
                    _firstSelectedCard.IsMatched = true;
                    _secondSelectedCard.IsMatched = true;
                    _firstSelectedCard = null;
                    _secondSelectedCard = null;

                    if (Cards.All(c => c.IsMatched))
                    {
                        _gameTimer.Stop();
                        IsGameActive = false;
                        SaveStatistics(true);
                        MessageBox.Show("Felicitări! Ai câștigat jocul!");
                    }
                }
                else
                {
                    IsGameActive = false;
                    System.Threading.Thread.Sleep(1000);
                    _firstSelectedCard.IsFlipped = false;
                    _secondSelectedCard.IsFlipped = false;
                    _firstSelectedCard = null;
                    _secondSelectedCard = null;
                    IsGameActive = true;
                }
            }
        }

        private void StartNewGame(object parameter)
        {
            var rows = IsStandardMode ? 4 : CustomRows;
            var columns = IsStandardMode ? 4 : CustomColumns;

            if ((rows * columns) % 2 != 0)
            {
                MessageBox.Show("Numărul total de cărți trebuie să fie par!");
                return;
            }

            Settings = new GameSettings(rows, columns, 300, SelectedCategory);
            var category = _categories.FirstOrDefault(c => c.Name == SelectedCategory);
            if (category == null) return;

            var random = new Random();
            var selectedImages = category.ImagePaths.OrderBy(x => random.Next())
                                                 .Take((rows * columns) / 2)
                                                 .ToList();

            var cards = new List<GameCard>();
            var id = 0;
            foreach (var imagePath in selectedImages)
            {
                cards.Add(new GameCard(id++, imagePath));
                cards.Add(new GameCard(id++, imagePath));
            }

            Cards = new ObservableCollection<GameCard>(cards.OrderBy(x => random.Next()));
            TimeRemaining = Settings.TimeLimit;
            IsGameActive = true;
            _gameTimer.Start();
        }

        private void SaveGame(object parameter)
        {
            if (!IsGameActive) return;

            var saveData = new
            {
                User = _currentUser.Username,
                Settings = Settings,
                Cards = Cards.Select(c => new { c.Id, c.ImagePath, c.IsFlipped, c.IsMatched }).ToList(),
                TimeRemaining = TimeRemaining
            };

            var savePath = Path.Combine(_savesDirectory, $"{_currentUser.Username}_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            File.WriteAllText(savePath, JsonConvert.SerializeObject(saveData));
            MessageBox.Show("Joc salvat cu succes!");
        }

        private void OpenGame(object parameter)
        {
            var dialog = new OpenFileDialog
            {
                InitialDirectory = _savesDirectory,
                Filter = "Save files (*.json)|*.json",
                Title = "Selectează jocul salvat"
            };

            if (dialog.ShowDialog() == true)
            {
                var saveData = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(dialog.FileName));
                if (saveData.User != _currentUser.Username)
                {
                    MessageBox.Show("Acest joc nu aparține utilizatorului curent!");
                    return;
                }

                Settings = new GameSettings(
                    (int)saveData.Settings.Rows,
                    (int)saveData.Settings.Columns,
                    (int)saveData.TimeRemaining,
                    (string)saveData.Settings.Category
                );

                Cards.Clear();
                foreach (var cardData in saveData.Cards)
                {
                    Cards.Add(new GameCard((int)cardData.Id, (string)cardData.ImagePath)
                    {
                        IsFlipped = (bool)cardData.IsFlipped,
                        IsMatched = (bool)cardData.IsMatched
                    });
                }

                TimeRemaining = (int)saveData.TimeRemaining;
                IsGameActive = true;
                _gameTimer.Start();
            }
        }

        private void ShowStatistics(object parameter)
        {
            var statistics = LoadStatistics();
            var message = $"Statistici pentru {_currentUser.Username}:\n\n";
            message += $"Jocuri jucate: {statistics.GamesPlayed}\n";
            message += $"Jocuri câștigate: {statistics.GamesWon}\n";
            message += $"Rata de succes: {statistics.WinRate:P}";

            MessageBox.Show(message, "Statistici");
        }

        private void SaveStatistics(bool isWin)
        {
            var statistics = LoadStatistics();
            statistics.GamesPlayed++;
            if (isWin) statistics.GamesWon++;

            File.WriteAllText(_statisticsFilePath, JsonConvert.SerializeObject(statistics));
        }

        private GameStatistics LoadStatistics()
        {
            if (File.Exists(_statisticsFilePath))
            {
                return JsonConvert.DeserializeObject<GameStatistics>(File.ReadAllText(_statisticsFilePath))
                    ?? new GameStatistics();
            }
            return new GameStatistics();
        }

        private void OnCategorySelected(object parameter)
        {
            if (parameter is string category)
            {
                SelectedCategory = category;
            }
        }

        private void OnModeSelected(object parameter)
        {
            if (parameter is string mode)
            {
                IsStandardMode = mode == "Standard";
            }
        }

        private void ShowAbout(object parameter)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void ExitGame(object parameter)
        {
            _gameTimer.Stop();
            var loginWindow = new LoginView();
            loginWindow.Show();
            Application.Current.MainWindow.Close();
        }
    }

    public class GameStatistics
    {
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public double WinRate => GamesPlayed > 0 ? (double)GamesWon / GamesPlayed : 0;
    }
} 