using MemoryGameLab2.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MemoryGameLab2.Views
{
    public partial class CustomSettingsWindow : Window
    {
        public int SelectedRows { get; private set; }
        public int SelectedColumns { get; private set; }

        public CustomSettingsWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (RowsComboBox.SelectedItem == null || ColumnsComboBox.SelectedItem == null)
            {
                ErrorText.Text = "Please select both rows and columns";
                return;
            }

            SelectedRows = int.Parse((RowsComboBox.SelectedItem as ComboBoxItem).Content.ToString());
            SelectedColumns = int.Parse((ColumnsComboBox.SelectedItem as ComboBoxItem).Content.ToString());

            if (SelectedRows * SelectedColumns % 2 != 0)
            {
                ErrorText.Text = "Total cards must be even! (Rows × Columns must be even)";
                return;
            }

            var vm = Application.Current.MainWindow.DataContext as GameViewModel;
            if (vm != null)
            {
                vm.IsStandardMode = false;
            }

            this.DialogResult = true;
            this.Close();
        }
    }
}