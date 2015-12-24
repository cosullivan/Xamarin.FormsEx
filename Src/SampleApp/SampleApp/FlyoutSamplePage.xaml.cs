using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.FormsEx;

namespace SampleApp
{
    public partial class FlyoutSamplePage
    {
        public FlyoutSamplePage()
        {
            InitializeComponent();
        }

        async void OnButtonClicked(object sender, EventArgs e)
        {
            await _top.FlyoutAsync();
            await Task.Delay(500);

            await _left.FlyoutAsync();
            await Task.Delay(500);

            await _bottom.FlyoutAsync();
            await Task.Delay(500);

            await AnimateRightFlyout();
            await Task.Delay(500);

            await _bottom.BackAsync();
            await Task.Delay(500);

            await _left.BackAsync();
            await Task.Delay(500);

            await _top.BackAsync();
            await Task.Delay(500);
        }

        async Task AnimateRightFlyout()
        {
            var width = _right.WidthRequest;

            await _right.FlyoutAsync();
            await Task.Delay(500);

            _right.WidthRequest += 100;
            await _right.FlyoutAsync();
            await Task.Delay(500);

            _right.WidthRequest += 50;
            await _right.FlyoutAsync();
            await Task.Delay(500);

            await _right.BackAsync();
            await Task.Delay(500);

            await _right.BackAsync();
            await Task.Delay(500);

            await _right.BackAsync();

            _right.WidthRequest = width;
        }
    }
}
