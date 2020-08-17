using OTUS_SoftwareArchitect_Client.ViewModels;
using System;

using Xamarin.Forms.Xaml;

namespace OTUS_SoftwareArchitect_Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditProjectPage : ContentPageBase
    {
        public EditProjectPage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var vm = BindingContext as EditProjectViewModel;

            vm.ProjectDeleted += OnProjectDeleted;
            vm.ProjectSaved += OnProjectSaved;
            vm.UpdateMembersRequested += OnProjectMemberPickRequested;
            vm.ListsNavigationRequested += OnListsNavigationRequested;
        }

        private void OnListsNavigationRequested(object sender, EventArgs e)
        {
            var vm = sender as EditProjectViewModel;

            Navigation.PushAsync(new ProjectListsPage
            {
                Title = $"Lists of {vm.Title}",
                BindingContext = new ProjectListsViewModel(vm.ProjectId)
            });
        }

        private void OnProjectMemberPickRequested(object sender, EventArgs e)
        {
            Navigation.PushAsync(new PickMembersPage() { BindingContext = this.BindingContext });
        }

        private void OnProjectSaved(object sender, EventArgs e)
        {
            Unsubsribe(sender);
            Navigation.PopAsync();
        }

        private void OnProjectDeleted(object sender, EventArgs e)
        {
            Unsubsribe(sender);
            Navigation.PopAsync();
        }

        private void Unsubsribe(object viewModelObject)
        {
            var vm = viewModelObject as EditProjectViewModel;

            vm.ProjectDeleted -= OnProjectDeleted;
            vm.ProjectSaved -= OnProjectSaved;
            vm.UpdateMembersRequested -= OnProjectMemberPickRequested;
            vm.ListsNavigationRequested -= OnListsNavigationRequested;
        }
    }
}