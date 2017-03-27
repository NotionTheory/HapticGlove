using System.Windows;
using System.Windows.Controls;

namespace NotionTheory.HapticGlove
{
    /// <summary>
    /// Interaction logic for BodyComponent.xaml
    /// </summary>
    public partial class BodyComponent : UserControl
    {
        public BodyComponent()
        {
            InitializeComponent();
        }

        Body body
        {
            get
            {
                return this.DataContext as Body;
            }
        }

        private void TestFingers_Checked(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            if(cb != null)
            {
                this.body.TestFingers = cb.IsChecked.HasValue && cb.IsChecked.Value;
            }
        }

        private void SwapHands_Click(object sender, RoutedEventArgs e)
        {
            this.body.SwapHands();
        }

        private void TestMotors_Checked(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            if(cb != null)
            {
                this.body.TestMotors = cb.IsChecked.HasValue && cb.IsChecked.Value;
            }
        }
    }
}
