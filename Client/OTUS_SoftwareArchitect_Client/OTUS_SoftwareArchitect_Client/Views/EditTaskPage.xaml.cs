using OTUS_SoftwareArchitect_Client.ViewModels;
using System;
using Xamarin.Forms.Xaml;

namespace OTUS_SoftwareArchitect_Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditTaskPage : ContentPageBase
    {
        public EditTaskPage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var vm = BindingContext as EditTaskViewModel;
            vm.TaskSaved += OnTaskSaved;
            vm.TaskDeleted += OnTaskDeleted;
            vm.PickMembersRequested += OnPickMembersRequested;
            vm.PickLabelsRequested += OnPickLabelsRequested;
        }

        private void OnTaskDeleted(object sender, EventArgs e)
        {
            Unsubscribe(sender);
            Navigation.PopAsync();
        }

        private void OnTaskSaved(object sender, EventArgs e)
        {
            Unsubscribe(sender);
            Navigation.PopAsync();
        }

        private void OnPickLabelsRequested(object sender, EventArgs e)
        {
            Navigation.PushAsync(new PickLabelsPage { BindingContext = this.BindingContext });
        }

        private void OnPickMembersRequested(object sender, EventArgs e)
        {
            Navigation.PushAsync(new PickMembersPage { BindingContext = this.BindingContext });
        }

        private void Unsubscribe(object vmObject)
        {
            var vm = vmObject as EditTaskViewModel;
            vm.TaskSaved -= OnTaskSaved;
            vm.TaskDeleted -= OnTaskDeleted;
            vm.PickMembersRequested -= OnPickMembersRequested;
            vm.PickLabelsRequested -= OnPickLabelsRequested;
        }
    }
}