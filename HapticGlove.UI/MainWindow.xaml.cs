using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace NotionTheory.HapticGlove
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = Application.Current;
        }
    }
}
