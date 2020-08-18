using OTUS_SoftwareArchitect_Client.Models;
using OTUS_SoftwareArchitect_Client.Networking.Misc;
using OTUS_SoftwareArchitect_Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.ViewModels
{
    public class WorkingHoursViewModel : BaseViewModel, IViewLoadingAware
    {
        private readonly WorkingHoursService _workingHoursService;
        private IEnumerable<WorkingHoursRecordModel> _workingHoursRecords;

        public WorkingHoursViewModel()
        {
            _workingHoursService = DependencyService.Resolve<WorkingHoursService>();

            CreateRecordCommand = new Command(OnCreateRecordTapped);
            RefreshCommand = new AsyncCommand(RefreshAsync);
            DeleteRecordCommand = new AsyncCommand<object>(DeleteRecordAsync);
        }

        public IEnumerable<WorkingHoursRecordModel> WorkingHoursRecords
        { 
            get => _workingHoursRecords;
            set
            {
                _workingHoursRecords = value;
                OnPropertyChanged();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand DeleteRecordCommand { get; }
        public ICommand CreateRecordCommand { get; }
        public ICommand SelectedCommand { get; }

        public event EventHandler CreateRecordRequested; 

        public void OnViewAppearing()
        {
            RefreshCommand?.Execute(null);
        }

        private async Task RefreshAsync()
        {
            IsBusy = true;

            try
            {
                var recordsResut = await _workingHoursService.GetMyWorkingHoursAsync();
                if (!recordsResut.IsSuccess)
                {
                    ShowToast(recordsResut.GetFullMessage());
                    return;
                }

                WorkingHoursRecords = recordsResut.Result.ToList();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteRecordAsync(object recordObject)
        {
            var record = recordObject as WorkingHoursRecordModel;

            var deletionResult = await _workingHoursService.DeleteWorkingHoursRecord(record.Id);
            if (!deletionResult.IsSuccess)
            {
                ShowToast(deletionResult.GetFullMessage());
                return;
            }

            ShowToast("Record deleted");
        }

        private void OnCreateRecordTapped()
        {
            CreateRecordRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
