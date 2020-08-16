using OTUS_SoftwareArchitect_Client.DTO.TaskDtos;
using OTUS_SoftwareArchitect_Client.Networking.Misc;
using OTUS_SoftwareArchitect_Client.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.ViewModels
{
    public class EditTaskViewModel : CreateTaskViewModel
    {
        private readonly string _taskId;
        private readonly TasksService _tasksService;
        
        private int _version;

        public EditTaskViewModel(string taskId)
        {
            _taskId = taskId;
            _tasksService = DependencyService.Resolve<TasksService>();

            SaveTaskCommand = new AsyncCommand(CreateTaskAsync);
            DeleteTaskCommand = new AsyncCommand(DeleteTaskAsync);
        }

        protected override async Task InitializeTaskRelatedDataAsync()
        {
            await base.InitializeTaskRelatedDataAsync();

            await SetTaskDataAsync();

        }

        private async Task SetTaskDataAsync()
        {
            IsBusy = true;

            try
            {
                var taskRequestResult = await _tasksService.GetTaskAsync(_taskId);
                if(!taskRequestResult.IsSuccess)
                {
                    ShowToast(taskRequestResult.GetFullMessage());
                    return;
                }

                var task = taskRequestResult.Result;
                
                _version = task.Version;
                Title = task.Title;
                Description = task.Description;
                DueDate = task.DueDate?.LocalDateTime;
                SelectedList = Lists.FirstOrDefault(l => l.Id == task.ListId);

                if(task.Labels.Any())
                {
                    var labels = AllLabels.Where(l => task.Labels.Any(tl => tl.Id == l.Id));
                    SelectedLabels = new ObservableCollection<object>(labels);
                }

                if (task.Members.Any())
                {
                    var members = AllUsers.Where(u => task.Members.Any(m => m.Id == u.Id));
                    Members = new ObservableCollection<object>(members);
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand SaveTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }


        public event EventHandler TaskSaved;
        public event EventHandler TaskDeleted;

        protected override CreateTaskDto GetTaskDto()
        {
            return new UpdateTaskDto
            {
                Id = _taskId,
                Version = _version
            };
        }

        protected override async Task SendDtoAsync(CreateTaskDto dto)
        {
            var updateResult = await _tasksService.UpdateTaskAsync(dto as UpdateTaskDto);
            if(!updateResult.IsSuccess)
            {
                ShowToast(updateResult.GetFullMessage());
                return;
            }

            TaskSaved?.Invoke(this, EventArgs.Empty);           
        }



        private async Task DeleteTaskAsync()
        {
            IsBusy = true;

            try
            {
                var deletionResult = await _tasksService.DeleteTaskAsync(_taskId);
                if(!deletionResult.IsSuccess)
                {
                    ShowToast(deletionResult.GetFullMessage());
                    return;
                }

                TaskDeleted?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
