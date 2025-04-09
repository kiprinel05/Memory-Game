using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MemoryGameLab2.Models
{
    public class GameCard : INotifyPropertyChanged
    {
        private int _id;
        private string _imagePath;
        private bool _isFlipped;
        private bool _isMatched;
        private BitmapImage _displayImage;
        private static readonly BitmapImage _backImage = new BitmapImage(new Uri("/Images/CardBack.png", UriKind.Relative));

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged();
                UpdateDisplayImage();
            }
        }

        public bool IsFlipped
        {
            get => _isFlipped;
            set
            {
                _isFlipped = value;
                OnPropertyChanged();
                UpdateDisplayImage();
            }
        }

        public bool IsMatched
        {
            get => _isMatched;
            set
            {
                _isMatched = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage DisplayImage
        {
            get => _displayImage;
            private set
            {
                _displayImage = value;
                OnPropertyChanged();
            }
        }

        public GameCard(int id, string imagePath)
        {
            Id = id;
            ImagePath = imagePath;
            IsFlipped = false;
            IsMatched = false;
            UpdateDisplayImage();
        }

        private void UpdateDisplayImage()
        {
            if (IsFlipped)
            {
                // Afișează imaginea cărții când este întoarsă
                DisplayImage = new BitmapImage(new Uri(ImagePath, UriKind.Absolute));
            }
            else
            {
                // Afișează spatele cărții când nu este întoarsă
                DisplayImage = null; // Butonul va rămâne albastru (Background="Blue")
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}