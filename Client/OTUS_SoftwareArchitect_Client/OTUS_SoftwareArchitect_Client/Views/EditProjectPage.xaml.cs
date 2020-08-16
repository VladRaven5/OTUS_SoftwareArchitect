using OTUS_SoftwareArchitect_Client.ViewModels;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OTUS_SoftwareArchitect_Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditProjectPage : ContentPage
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
        }

        private void OnProjectMemberPickRequested(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ProjectMembersPickPage() { BindingContext = this.BindingContext });
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
        }
    }
}