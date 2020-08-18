using OTUS_SoftwareArchitect_Client.DTO.WorkingHours;
using OTUS_SoftwareArchitect_Client.Models.TaskModels;
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
    public class CreateWorkingHoursRecordViewModel : BaseViewModel, IViewLoadingAware
    {
        private readonly TasksService _tasksService;
        private readonly WorkingHoursService _workingHoursService;
        private readonly string _requestId;

        private IEnumerable<TaskModel> _availabelTasks;
        private TaskModel _selectedTask;
        private string _description;
        private string _hours;
        private string _projectTitle;

        public CreateWorkingHoursRecordViewModel()
        {
            _requestId = RequestIdProvider.GetRequestId();
            _tasksService = DependencyService.Resolve<TasksService>();
            _workingHoursService = DependencyService.Resolve<WorkingHoursService>();

            CreateCommand = new AsyncCommand(CreateRecordAsync);

            Hours = "1";
        }

        public IEnumerable<TaskModel> AvailableTasks
        {
            get => _availabelTasks;
            set
            {
                _availabelTasks = value;
                OnPropertyChanged();
            }
        }

        public TaskModel SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
                OnPropertyChanged();
                ProjectTitle = value?.ProjectTitle;
            }
        }

        public string ProjectTitle
        {
            get => _projectTitle;
            set
            {
                _projectTitle = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public string Hours
        {
            get => _hours;
            set
            {
                _hours = value;
                OnPropertyChanged();
            }
        }

        public ICommand CreateCommand { get; }

        public event EventHandler RecordCreated;

        private async Task RefreshAsync()
        {
            IsBusy = true;

            try
            {
                var taskResult = await _tasksService.GetMyTasksAsync();
                if (!taskResult.IsSuccess)
                {
                    ShowToast(taskResult.GetFullMessage());
                    return;
                }

                AvailableTasks = taskResult.Result;
                SelectedTask = AvailableTasks.FirstOrDefault();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnViewAppearing()
        {
            Task.Run(RefreshAsync);
        }

        private async Task CreateRecordAsync()
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                ShowToast("Description can't be null");
                return;
            }

            if (SelectedTask == null)
            {
                ShowToast("Task must be specified");
                return;
            }

            if (!double.TryParse(Hours, out double realHours))
            {
                ShowToast("Incorrect hours value");
                return;
            }

            if (realHours <= 0)
            {
                ShowToast("Hours must be positive number");
                return;
            }

            IsBusy = true;

            try
            {
                var dto = new CreateWorkingHoursRecordDto
                {
                    Hours = realHours,
                    TaskId = SelectedTask.Id,
                    Description = Description
                };
                var creationResult = await _workingHoursService.CreateWorkingHoursRecord(_requestId, dto);
                if (!creationResult.IsSuccess)
                {
                    ShowToast(creationResult.GetFullMessage());
                    return;
                }

                RecordCreated?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
