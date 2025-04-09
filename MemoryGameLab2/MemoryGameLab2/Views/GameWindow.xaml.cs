using System.Windows;
using MemoryGameLab2.Models;
using MemoryGameLab2.ViewModels;

namespace MemoryGameLab2.Views
{
    public partial class GameWindow : Window
    {
        public GameWindow(User currentUser)
        {
            InitializeComponent();
            DataContext = new GameViewModel(currentUser);
        }
    }
} 