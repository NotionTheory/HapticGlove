using System.Windows;
using System.Windows.Controls;

namespace HapticGloveServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public HapticGlove.Glove glove
        {
            get; set;
        }

        public Server server
        {
            get; set;
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainPage_Loaded;
            this.Closing += MainWindow_Closing;
            this.glove = new HapticGlove.Glove();
            this.glove.PropertyChanged += Glove_PropertyChanged;
            this.server = new Server();
            this.server.PropertyChanged += Server_PropertyChanged;
            this.DataContext = this;
        }

        private void Glove_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(this.glove.Sensors.Ready)
            {
                this.glove.PropertyChanged -= Glove_PropertyChanged;
                foreach(var reader in this.glove.Sensors.Readers)
                {
                    reader.PropertyChanged += Reader_PropertyChanged;
                }
            }
        }

        private void Reader_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Value")
            {
                var reader = sender as HapticGlove.Sensor;
                if(reader != null)
                {
                    this.server.SetSensorState(reader.Index, reader.Value);
                }
            }
        }

        private void Server_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "MotorState")
            {
                this.glove.SetMotorState(this.server.MotorState);
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.glove.Search();
            this.server.Start();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.server.Stop();
        }

        private static int? GetIndex(Control ctrl)
        {
            var pan = ctrl?.Parent as StackPanel;
            var sensor = pan?.DataContext as HapticGlove.Sensor;
            return sensor?.Index;
        }


        private void motor_Toggled(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            var index = GetIndex(cb);
            if(0 <= index && index < this.glove.Sensors.Count)
            {
                this.glove[index.Value].Motor = cb.IsChecked.HasValue && cb.IsChecked.Value;
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
