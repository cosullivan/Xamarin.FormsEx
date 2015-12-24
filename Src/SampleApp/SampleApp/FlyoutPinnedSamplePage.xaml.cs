using System;
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
            await _bottom.FlyoutAsync();
        }

        async void OnDownButtonClicked(object sender, EventArgs e)
        {
            await _bottom.BackAsync();
        }
    }
}
