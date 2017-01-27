using System;
using Windows.UI.Xaml.Controls;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HapticGloveServer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DispatcherTimer t;
        public MainPage()
        {
            this.InitializeComponent();
            var glove = HapticGlove.Glove.DEFAULT;
            DataContext = glove;
            t = new DispatcherTimer();
            t.Interval = new TimeSpan(0, 0, 0, 1);
            t.Tick += T_Tick;
            t.Start();
        }

        private void T_Tick(object sender, object e)
        {
            var glove = HapticGlove.Glove.DEFAULT;
            if(glove.State == HapticGlove.GloveState.NotReady)
            {
                glove.Test();
            }
        }

        private void motor_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            if(cb != null)
            {
                var glove = HapticGlove.Glove.DEFAULT;
                var index = motors.Children.IndexOf(cb);
                if(0 <= index && index < glove.Motors.Count)
                {
                    glove.Motors[index] = cb.IsChecked.HasValue && cb.IsChecked.Value;
                }
            }
        }
    }
}
