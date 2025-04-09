using System.Windows.Media.Imaging;

namespace MemoryGameLab2.Models
{
    public class GameCard
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public bool IsFlipped { get; set; }
        public bool IsMatched { get; set; }
        public BitmapImage Image { get; set; }

        public GameCard(int id, string imagePath)
        {
            Id = id;
            ImagePath = imagePath;
            IsFlipped = false;
            IsMatched = false;
            Image = new BitmapImage(new System.Uri(imagePath));
        }
    }
} 