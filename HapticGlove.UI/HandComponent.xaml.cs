using System.Windows;
using System.Windows.Controls;

namespace NotionTheory.HapticGlove
{
    /// <summary>
    /// Interaction logic for Hand.xaml
    /// </summary>
    public partial class HandComponent : UserControl
    {

        public Hand hand
        {
            get
            {
                return this.DataContext as Hand;
            }
        }

        public HandComponent()
        {
            InitializeComponent();
        }


        private void CalibrateMinAll_Click(object sender, RoutedEventArgs e)
        {
            this.hand.CalibrateMin();
        }

        private void CalibrateMaxAll_Click(object sender, RoutedEventArgs e)
        {
            this.hand.CalibrateMax();
        }

        private void CloseFist_Click(object sender, RoutedEventArgs e)
        {
            this.hand.CloseFistTest();
        }

        private void OpenFirst_Click(object sender, RoutedEventArgs e)
        {
            this.hand.OpenFistTest();
        }

        private void ClearMotors_Click(object sender, RoutedEventArgs e)
        {
            this.hand.ClearMotorState();
        }
    }
}
