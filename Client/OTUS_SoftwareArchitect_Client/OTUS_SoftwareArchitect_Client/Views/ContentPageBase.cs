using OTUS_SoftwareArchitect_Client.ViewModels;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.Views
{
    public abstract class ContentPageBase : ContentPage
    {
        private bool _loadedOnce = false;

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!_loadedOnce)
            {
                _loadedOnce = true;
                (BindingContext as IViewLoadingAware)?.OnViewAppearing();
            }
        }
    }
}
