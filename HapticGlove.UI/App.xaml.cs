using System.Windows;
using System.Timers;

namespace NotionTheory.HapticGlove
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        Timer t;
        public App()
        {
            this.Body = new Body();
            this.Body.PropertyChanged += Body_PropertyChanged;

            this.Server = new Server();
            this.Server.PropertyChanged += Server_PropertyChanged;
            this.Server.ClientDisconnected += Server_ClientDisconnected;

            this.Exit += App_Exit;

            this.t = new Timer(10);
            this.t.Elapsed += T_Elapsed;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            this.Server.DisconnectFromClient();
        }

        public Body Body
        {
            get; set;
        }

        public Server Server
        {
            get; set;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.t.Start();
            this.Body.Search();
        }

        private void Body_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "SensorValue")
            {
                this.Server.SetFinger(sender as Finger);
            }
        }


        private void Server_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Motors")
            {
                this.Body.SetMotors(this.Server.motors);
            }
        }

        private void Server_ClientDisconnected(object sender, System.EventArgs e)
        {
            if(!this.Server.IsConnected)
            {
                this.Body.ClearMotorState();
            }
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Body.Update(this.Server.IsConnected);
            this.Server.Update();
        }
    }
}
