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
            await _bottom1.FlyoutAsync();

            //if (Flyout.GetIsShowing(_bottom1) == false)
            //{
            //    await _bottom1.FlyoutAsync();
            //    return;
            //}

            //if (Flyout.GetIsShowing(_bottom2) == false)
            //{
            //    await _bottom2.FlyoutAsync();
            //}
        }

        async void OnDownButtonClicked(object sender, EventArgs e)
        {
            //if (Flyout.GetIsShowing(_bottom2))
            //{
            //    await _bottom2.BackAsync();
            //}

            //await _bottom1.BackAsync();
        }
    }
}
