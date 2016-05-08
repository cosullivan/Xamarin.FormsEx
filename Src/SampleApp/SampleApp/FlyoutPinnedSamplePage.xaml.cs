using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.FormsEx;

namespace SampleApp
{
    public partial class FlyoutPinnedSamplePage : ContentPage
    {
        public FlyoutPinnedSamplePage()
        {
            InitializeComponent();
        }

        async void OnUpButtonClicked(object sender, EventArgs e)
        {
            if (_bottom1.Children.Count == 0)
            {
                _bottom1.Children.Add(new BoxView { HeightRequest = 100, BackgroundColor = Color.White });
                await _bottom1.FlyoutAsync();

                return;
            }

            if (_bottom1.Children.Count == 1)
            {
                _bottom1.Children.Add(new BoxView { HeightRequest = 75, BackgroundColor = Color.Red });
                await _bottom1.FlyoutAsync();

                //var view = new StackLayout { HeightRequest = 75, BackgroundColor = Color.Red };

                //var label = new Label { Text = "1" };
                //var button = new Button { Text = "Click" };
                //button.Clicked += (o, args) => label.Text += "2";
                
                //view.Children.Add(label);
                //view.Children.Add(button);

                //_bottom1.Children.Add(view);
                //await _bottom1.FlyoutAsync();
                
                return;
            }

            if (_bottom2.Children.Count == 0)
            {
                _bottom2.Children.Add(new BoxView { HeightRequest = 50, BackgroundColor = Color.Blue });
                await _bottom2.FlyoutAsync();

                return;
            }

            if (_bottom2.Children.Count == 1)
            {
                _bottom2.Children.Add(new BoxView { HeightRequest = 25, BackgroundColor = Color.Yellow });
                await _bottom2.FlyoutAsync();

                return;
            }
        }

        async void OnDownButtonClicked(object sender, EventArgs e)
        {
            if (_bottom1.Children.Count == 2)
            {
                await _bottom1.BackAsync();
                _bottom1.Children.RemoveAt(1);

                return;
            }

            if (_bottom1.Children.Count == 1)
            {
                await _bottom1.BackAsync();
                _bottom1.Children.RemoveAt(0);

                return;
            }

            if (_bottom2.Children.Count == 2)
            {
                await _bottom2.BackAsync();
                _bottom2.Children.RemoveAt(1);

                return;
            }

            if (_bottom2.Children.Count == 1)
            {
                await _bottom2.BackAsync();
                _bottom2.Children.RemoveAt(0);

                return;
            }
            //await _bottom1.CloseFlyoutAsync();
            //await _bottom2.CloseFlyoutAsync();
        }
    }
}
