using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Emmellsoft.IoT.Rpi.SenseHat;
using System.Threading.Tasks;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RaspberryPi3SenseHat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ISenseHat SenseHat { get; set; }

        public MainPage()
        {
            this.InitializeComponent();


            TemperaturTest();
            //DisplayTest();
            //JoystickTest();
            //CompasTest();
        }

        private async void SetScreenText(string text)
        {
            // Need to invoke UI updates on the UI thread because this event handler gets invoked on a separate thread.
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    textBlock.Text = text;
                });
        }

        private async void TemperaturTest()
        {
            decimal min = 29;
            decimal max = 33;

            SenseHat = await SenseHatFactory.GetSenseHat().ConfigureAwait(false);

            while (true)
            {
                SenseHat.Sensors.HumiditySensor.Update();

                if (SenseHat.Sensors.Temperature.HasValue)
                {
                    var temperatur = SenseHat.Sensors.Temperature.Value;
                    var temperaturColor = HeatMap((decimal)temperatur, min, max);

                    SetScreenText($"Temp: {temperatur.ToString()}");


                    SenseHat.Display.Clear();
                    SenseHat.Display.Fill(temperaturColor);
                    SenseHat.Display.Update();

                    await Task.Delay(1000);
                }
                else
                {
                    await Task.Delay(500);
                }
            }
        }

        private async void DisplayTest()
        {
            SenseHat = await SenseHatFactory.GetSenseHat().ConfigureAwait(false);

            while (true)
            {
                var rand = new Random();

                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        Color pixel = Color.FromArgb(
                            255,
                            (byte)rand.Next(256),
                            (byte)rand.Next(256),
                            (byte)rand.Next(256));

                        SenseHat.Display.Screen[x, y] = pixel;
                    }
                }

                SenseHat.Display.Update();
                await Task.Delay(500);
            }
        }

        private async void JoystickTest()
        {
            SenseHat = await SenseHatFactory.GetSenseHat().ConfigureAwait(false);

            while (true)
            {
                if (SenseHat.Joystick.Update())
                {
                    switch (SenseHat.Joystick.EnterKey)
                    {
                        case KeyState.Released:
                            SenseHat.Display.Fill(Colors.Blue);
                            SenseHat.Display.Update();
                            break;
                        case KeyState.Pressing:
                            SenseHat.Display.Fill(Colors.Red);
                            SenseHat.Display.Update();
                            break;
                        case KeyState.Pressed:
                            SenseHat.Display.Fill(Colors.Green);
                            SenseHat.Display.Update();
                            break;
                        case KeyState.Releasing:
                            SenseHat.Display.Fill(Colors.Yellow);
                            SenseHat.Display.Update();
                            break;
                        default:
                            break;
                    }
                }

                await Task.Delay(50);
            }
        }

        private async void CompasTest()
        {
            SenseHat = await SenseHatFactory.GetSenseHat().ConfigureAwait(false);


            while (true)
            {
                SenseHat.Sensors.ImuSensor.Update();

                if (SenseHat.Sensors.Pose.HasValue)
                {
                    double northAngle = SenseHat.Sensors.Pose.Value.Z;

                    SetScreenText($"Pos: {northAngle.ToString()}");
                }

                await Task.Delay(2000);
            }
        }


        public Color HeatMap(decimal value, decimal min, decimal max)
        {
            decimal val = (value - min) / (max - min);
            return new Color
            {
                A = 255,
                R = Convert.ToByte(255 * val),
                G = Convert.ToByte(255 * (1 - val)),
                B = 0
            };
        }
    }
}
