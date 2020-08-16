using OTUS_SoftwareArchitect_Client.ViewModels;
using System;
using Xamarin.Forms.Xaml;

namespace OTUS_SoftwareArchitect_Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateTaskPage : ContentPageBase
    {
        public CreateTaskPage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var vm = BindingContext as CreateTaskViewModel;
            vm.TaskCreated += OnTaskCreated;
            vm.PickMembersRequested += OnPickMembersRequested;
        }

        private void OnPickMembersRequested(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ProjectMembersPickPage { BindingContext = this.BindingContext });
        }

        private void OnTaskCreated(object sender, EventArgs e)
        {
            var vm = sender as CreateTaskViewModel;
            vm.TaskCreated -= OnTaskCreated;
            vm.PickMembersRequested -= OnPickMembersRequested;

            Navigation.PopAsync();
        }
    }
}