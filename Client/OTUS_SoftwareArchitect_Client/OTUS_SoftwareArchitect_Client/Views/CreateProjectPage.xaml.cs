using OTUS_SoftwareArchitect_Client.ViewModels;
using System;
using Xamarin.Forms.Xaml;

namespace OTUS_SoftwareArchitect_Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateProjectPage : ContentPageBase
    {
        public CreateProjectPage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var vm = BindingContext as CreateProjectViewModel;

            vm.ProjectCreated += OnProjectCreated;
        }

        private void OnProjectCreated(object sender, EventArgs e)
        {
            var vm = sender as CreateProjectViewModel;
            vm.ProjectCreated -= OnProjectCreated;

            Navigation.PopAsync();
        }
    }
}