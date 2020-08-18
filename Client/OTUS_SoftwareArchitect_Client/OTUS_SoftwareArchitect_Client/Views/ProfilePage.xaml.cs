using OTUS_SoftwareArchitect_Client.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OTUS_SoftwareArchitect_Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : ContentPageBase
    {
        public ProfilePage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var vm = BindingContext as ProfileViewModel;
            vm.LogoutCompleted += OnLogout;
        }

        private void OnLogout(object vmObject, EventArgs e)
        {
            var vm = vmObject as ProfileViewModel;
            vm.LogoutCompleted -= OnLogout;

            App.Current.MainPage = new NavigationPage(new AuthPage());
        }
    }
}