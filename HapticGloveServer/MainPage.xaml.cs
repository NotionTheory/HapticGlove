using System;
using Windows.UI.Xaml.Controls;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HapticGloveServer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DispatcherTimer t;
        HapticGlove.Glove glove;
        public MainPage()
        {
            this.InitializeComponent();
            this.glove = new HapticGlove.Glove(CoreWindow.GetForCurrentThread().Dispatcher);
            DataContext = this.glove;

            if(true)
            {
                this.glove.Search();
            }
            else
            {
                t = new DispatcherTimer();
                t.Interval = new TimeSpan(0, 0, 0, 1);
                t.Tick += T_Tick;
                t.Start();
            }
        }

        private void T_Tick(object sender, object e)
        {
            if(this.glove.State == HapticGlove.GloveState.NotReady)
            {
                this.glove.Test();
            }
        }

        private void motor_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var cb = sender as ToggleSwitch;
            if(cb != null)
            {
                var index = motors.Children.IndexOf(cb);
                if(0 <= index && index < this.glove.Motors.Count)
                {
                    this.glove.Motors[index] = cb.IsOn;
                }
            }
        }
    }
}
