using OTUS_SoftwareArchitect_Client.ViewModels;
using System;
using Xamarin.Forms.Xaml;

namespace OTUS_SoftwareArchitect_Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyTasksPage : ContentPageBase
    {       
        public MyTasksPage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var vm = BindingContext as MyTasksViewModel;
            vm.CreateTaskRequested += OnCreateTaskRequested;
            vm.TaskSelected += OnTaskSelected;
        }

        private void OnTaskSelected(object sender, ItemSelectedEventArgs e)
        {
            Navigation.PushAsync(new EditTaskPage { BindingContext = new EditTaskViewModel(e.ItemId) });
        }

        private void OnCreateTaskRequested(object sender, EventArgs e)
        {
            Navigation.PushAsync(new CreateTaskPage());
        }        
    }
}