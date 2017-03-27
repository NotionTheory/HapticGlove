using System.Windows;
using System.Windows.Controls;

namespace NotionTheory.HapticGlove
{
    /// <summary>
    /// Interaction logic for Finger.xaml
    /// </summary>
    public partial class FingerComponent : UserControl
    {

        public FingerComponent()
        {
            InitializeComponent();
        }

        Finger finger
        {
            get
            {
                return this.DataContext as Finger;
            }
        }

        private void CalibrateMaxFinger_Click(object sender, RoutedEventArgs e)
        {
            this.finger.CalibrateMax();
        }

        private void CalibrateMinFinger_Click(object sender, RoutedEventArgs e)
        {
            this.finger.CalibrateMin();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            this.finger.OpenFingerTest();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.finger.CloseFingerTest();
        }
    }
}
