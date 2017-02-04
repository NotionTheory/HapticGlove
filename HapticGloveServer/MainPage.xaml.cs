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
            this.Loaded += MainPage_Loaded;
            this.glove = new HapticGlove.Glove();
            DataContext = this.glove;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if(true)
            {
                this.glove.Search();
            }
            else
            {
                t = new DispatcherTimer();
                t.Interval = new TimeSpan(0, 0, 0, 1);
                t.Tick += (a, b) =>
                {
                    this.glove.Test();
                };

                t.Start();
            }
        }

        private static int? GetIndex(Control ctrl)
        {
            var pan = ctrl?.Parent as StackPanel;
            var sensor = pan?.DataContext as HapticGlove.Sensor;
            return sensor?.Index;
        }

        private void motor_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var ts = sender as ToggleSwitch;
            var index = GetIndex(ts);
            if(0 <= index && index < this.glove.Sensors.Count)
            {
                this.glove[index.Value].Motor = ts.IsOn;
            }
        }

        private void CalibrateMinFinger_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var index = GetIndex(btn);
            if(index.HasValue)
            {
                this.glove.CalibrateMin(index.Value);
            }
        }

        private void CalibrateMaxFinger_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var index = GetIndex(btn);
            if(index.HasValue)
            {
                this.glove.CalibrateMax(index.Value);
            }
        }

        private void CalibrateMinAll_Click(object sender, RoutedEventArgs e)
        {
            this.glove.CalibrateMin();
        }

        private void CalibrateMaxAll_Click(object sender, RoutedEventArgs e)
        {
            this.glove.CalibrateMax();
        }

        private void Min_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            var index = GetIndex(tb);
            if(index.HasValue)
            {
                var s = tb.Text;
                byte v = 0;
                if(byte.TryParse(s, out v))
                {
                    this.glove.CalibrateMin(index.Value, v);
                }
            }
        }

        private void Max_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            var index = GetIndex(tb);
            if(index.HasValue)
            {
                var s = tb.Text;
                byte v = 0;
                if(byte.TryParse(s, out v))
                {
                    this.glove.CalibrateMax(index.Value, v);
                }
            }
        }
    }
}
