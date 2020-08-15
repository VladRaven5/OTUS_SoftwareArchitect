
using OTUS_SoftwareArchitect_Client.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OTUS_SoftwareArchitect_Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var registerViewModel = BindingContext as RegisterViewModel;
            registerViewModel.Registered += OnRegistered;
        }

        private void OnRegistered(object sender, EventArgs e)
        {
            (sender as RegisterViewModel).Registered -= OnRegistered;
            Navigation.PopAsync();
        }
    }
}