using OTUS_SoftwareArchitect_Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OTUS_SoftwareArchitect_Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyTasksPage : ContentPage
    {
        private bool _loadedOnce = false;

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


        protected override void OnAppearing()
        {
            base.OnAppearing();

            if(!_loadedOnce)
            {
                _loadedOnce = true;
                (BindingContext as IViewLoadingAware)?.OnViewAppearing();
            }            
        }
    }
}