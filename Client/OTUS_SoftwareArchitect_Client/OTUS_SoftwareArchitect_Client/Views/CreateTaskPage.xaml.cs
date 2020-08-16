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
            vm.PickLabelsRequested += OnPickLabelsRequested;
        }

        private void OnPickLabelsRequested(object sender, EventArgs e)
        {
            Navigation.PushAsync(new PickLabelsPage { BindingContext = this.BindingContext });
        }

        private void OnPickMembersRequested(object sender, EventArgs e)
        {
            Navigation.PushAsync(new PickMembersPage { BindingContext = this.BindingContext });
        }

        private void OnTaskCreated(object sender, EventArgs e)
        {
            var vm = sender as CreateTaskViewModel;
            vm.TaskCreated -= OnTaskCreated;
            vm.PickMembersRequested -= OnPickMembersRequested;
            vm.PickLabelsRequested -= OnPickLabelsRequested;

            Navigation.PopAsync();
        }
    }
}