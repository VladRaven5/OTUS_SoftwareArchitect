using OTUS_SoftwareArchitect_Client.ViewModels;
using System;

using Xamarin.Forms;
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
        }


        private void OnCreateTaskRequested(object sender, EventArgs e)
        {
            Navigation.PushAsync(new CreateTaskPage());
        }        
    }
}