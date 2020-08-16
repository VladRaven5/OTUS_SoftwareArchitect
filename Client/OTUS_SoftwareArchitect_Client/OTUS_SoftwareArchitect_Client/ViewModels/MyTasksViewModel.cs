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
    public class MyTasksViewModel : BaseViewModel, IViewLoadingAware
    {
        private readonly TasksService _tasksService;
        private readonly IComparer<TaskState> _taskByStateComparer = new TaskByStateComparer();

        private List<TasksGroup> _tasks;

        public MyTasksViewModel()
        {
            _tasksService = DependencyService.Resolve<TasksService>();

            RefreshCommand = new AsyncCommand(RefreshCollectionAsync);
            CreateTaskCommand = new Command(CreateTask);
            TaskSelectedCommand = new Command<object>(OnTaskTapped);
        }

        public List<TasksGroup> Tasks 
        { 
            get => _tasks;
            private set
            {
                _tasks = value;
                OnPropertyChanged();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand CreateTaskCommand { get; }
        public ICommand TaskSelectedCommand { get; }

        public event EventHandler CreateTaskRequested;
        public event EventHandler<ItemSelectedEventArgs> TaskSelected;


        public void OnViewAppearing()
        {
            RefreshCommand.Execute(null);
        }

        private async Task RefreshCollectionAsync()
        {
            IsBusy = true;

            try
            {
                var myTasksRequest = await _tasksService.GetMyTasksAsync();

                if (!myTasksRequest.IsSuccess)
                {
                    ShowToast(myTasksRequest.GetFullMessage());
                    return;
                }

                PrepareTasksCollection(myTasksRequest.Result);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void PrepareTasksCollection(IEnumerable<TaskModel> tasks)
        {
            List<TasksGroup> taskGroups = new List<TasksGroup>();

            foreach(var listTasksGroup in tasks.OrderBy(task => task.ProjectId).GroupBy(task => task.ListId))
            {
                var firstTask = listTasksGroup.First();
                string groupTitle = $"{firstTask.ListTitle} (in {firstTask.ProjectTitle})";

                var group = new TasksGroup(groupTitle, listTasksGroup.OrderBy(task => task.State, _taskByStateComparer));
                
                taskGroups.Add(group);
            }

            Tasks = taskGroups;
        }

        private void CreateTask()
        {
            CreateTaskRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnTaskTapped(object taskObject)
        {
            var task = taskObject as TaskModel;
            TaskSelected?.Invoke(this, new ItemSelectedEventArgs(task.Id));
        }
    }

    public class TasksGroup : List<TaskModel>
    {
        public string Title { get; }

        public TasksGroup(string title, IEnumerable<TaskModel> tasks) : base(tasks)
        {
            Title = title;
        }

        public override string ToString()
        {
            return Title;
        }
    }

    public class TaskByStateComparer : IComparer<TaskState>
    {
        private static Dictionary<TaskState, int> _stateOrderDict = new Dictionary<TaskState, int>
        {
            { TaskState.Active, 0 },
            { TaskState.Proposed, 1 },
            { TaskState.Resolved, 2 },
            { TaskState.Closed, 3 }
        };


        public int Compare(TaskState x, TaskState y)
        {
            return _stateOrderDict[x] - _stateOrderDict[y];
        }
    }
}
