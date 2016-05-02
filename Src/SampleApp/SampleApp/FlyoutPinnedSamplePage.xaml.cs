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
                _bottom1.Children.AddVertical(new BoxView { HeightRequest = 100, BackgroundColor = Color.White });
                await _bottom1.FlyoutAsync();

                return;
            }

            if (_bottom1.Children.Count == 1)
            {
                _bottom1.Children.AddVertical(new BoxView { HeightRequest = 100, BackgroundColor = Color.Red });
                await _bottom1.FlyoutAsync();

                return;
            }
        }

        async void OnDownButtonClicked(object sender, EventArgs e)
        {
            //await _bottom2.BackAsync();
            //await _bottom1.BackAsync();

            await Task.WhenAll(_bottom2.BackAsync(), _bottom1.BackAsync());
            //if (Flyout.GetIsShowing(_bottom2))
            //{
            //    await _bottom2.BackAsync();
            //}

            //await _bottom1.BackAsync();
        }
    }
}
