using OTUS_SoftwareArchitect_Client.ViewModels;
using System;
using Xamarin.Forms.Xaml;

namespace OTUS_SoftwareArchitect_Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateWorkingHoursRecordPage : ContentPageBase
    {
        public CreateWorkingHoursRecordPage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var vm = BindingContext as CreateWorkingHoursRecordViewModel;

            vm.RecordCreated += OnRecordCreated;
        }

        private void OnRecordCreated(object vmObject, EventArgs e)
        {
            var vm = vmObject as CreateWorkingHoursRecordViewModel;
            vm.RecordCreated -= OnRecordCreated;
            Navigation.PopAsync();
        }
    }
}