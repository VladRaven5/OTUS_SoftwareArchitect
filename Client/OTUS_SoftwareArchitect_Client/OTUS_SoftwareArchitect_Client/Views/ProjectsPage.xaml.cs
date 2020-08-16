using OTUS_SoftwareArchitect_Client.ViewModels;
using System;
using Xamarin.Forms.Xaml;

namespace OTUS_SoftwareArchitect_Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProjectsPage : ContentPageBase
    {
        public ProjectsPage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var vm = BindingContext as ProjectsViewModel;
            vm.CreateProjectRequested += OnCreateProjectRequested;
            vm.ProjectSelected += OnProjectSelected;
        }

        private void OnProjectSelected(object sender, ItemSelectedEventArgs e)
        {
            var vm = new EditProjectViewModel(e.ItemId);
            Navigation.PushAsync(new EditProjectPage { BindingContext = vm } );
        }

        private void OnCreateProjectRequested(object sender, EventArgs e)
        {
            Navigation.PushAsync(new CreateProjectPage());
        }
    }
}