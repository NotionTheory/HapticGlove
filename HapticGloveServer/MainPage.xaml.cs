using System;
using Windows.UI.Xaml.Controls;
using System.Threading;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HapticGloveServer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Timer t;

        public MainPage()
        {
            this.InitializeComponent();
            t = new Timer(Tick, this, 3000, 1000);
        }

        private HapticGlove.GloveState lastState = HapticGlove.GloveState.NotReady;

        private void Tick(object state)
        {
            var glove = HapticGlove.Glove.DEFAULT;
            if(glove != null)
            {
                if(!glove.State.HasFlag(HapticGlove.GloveState.Ready))
                {
                    if(glove.State != lastState)
                    {
                        lastState = glove.State;
                        WriteLine(glove.State.ToString());
                    }

                    if(!glove.State.HasFlag(HapticGlove.GloveState.Searching) && glove.State.HasFlag(HapticGlove.GloveState.DeviceFound))
                    {
                        glove.Connect();
                    }
                }
                else
                {
                    Write("Finger [Battery: {0}%, Fingers: ",
                        glove.Battery,
                        glove.State == HapticGlove.GloveState.Ready ? "Ready" : glove.State.ToString(),
                        glove.Fingers.Count);
                    for(int i = 0; i < glove.Fingers.Count; ++i)
                    {
                        Write(", {0}", glove.Fingers[i]);
                    }
                    Write(" Motors: {0}", glove.Motors.Count);
                    for(int i = 0; i < glove.Motors.Count; ++i)
                    {
                        Write(", {0}", glove.Motors[i]);
                    }
                    Write("]");

                    if(glove.Error != null)
                    {
                        Write(" ERROR: {0}", glove.Error.Message);
                        t.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                    WriteLine();
                }
            }
        }

        private void Write(string msg = "")
        {
            this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.Description += msg;
                this.Bindings.Update();
            }).AsTask().Wait();
        }

        private void WriteLine(string msg = "")
        {
            Write(msg + "\n");
        }

        private void Write(string format, params object[] args)
        {
            Write(string.Format(format, args));
        }

        private void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
        }

        public string Description
        {
            get; set;
        }
    }
}
