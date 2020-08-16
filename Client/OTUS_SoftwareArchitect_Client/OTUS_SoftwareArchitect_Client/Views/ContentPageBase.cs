using OTUS_SoftwareArchitect_Client.Infrastructure;
using OTUS_SoftwareArchitect_Client.ViewModels;
using System.Threading.Tasks;
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

        protected Task<string> DisplayCustomPromptAsync(PromptConfig config)
        {
            //return null if pressed Cancel, Returns empty string if nothis was enter
            return this.DisplayPromptAsync(title: config.Title,
                message: config.Message,
                initialValue: config.InitialText,
                accept: config.OkText,
                cancel: config.CancelText,
                placeholder: config.Placeholder);
        }
    }
}
