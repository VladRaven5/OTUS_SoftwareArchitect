
using OTUS_SoftwareArchitect_Client.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OTUS_SoftwareArchitect_Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AuthPage : ContentPage
    {
        public AuthPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var authViewModel = BindingContext as AuthViewModel;
            authViewModel.SuccessLogin += NavigateToApp;
            authViewModel.RegistrationRequested += OpenRegisterPage;
        }

        private void NavigateToApp(object sender, EventArgs e)
        {
            var vm = sender as AuthViewModel;
            vm.SuccessLogin -= NavigateToApp;
            vm.RegistrationRequested -= OpenRegisterPage;

            App.Current.MainPage = new MainPage();
        }

        private void OpenRegisterPage()
        {
            Navigation.PushAsync(new RegisterPage());            
        }
    }
}