using System.Windows;
using MemoryGameLab2.ViewModels;

namespace MemoryGameLab2.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
        }
    }
} 