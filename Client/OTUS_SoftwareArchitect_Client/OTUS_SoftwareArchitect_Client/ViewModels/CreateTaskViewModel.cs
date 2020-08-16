using OTUS_SoftwareArchitect_Client.DTO.TaskDtos;
using OTUS_SoftwareArchitect_Client.Models;
using OTUS_SoftwareArchitect_Client.Models.BaseModels;
using OTUS_SoftwareArchitect_Client.Models.ProjectModels;
using OTUS_SoftwareArchitect_Client.Networking.Misc;
using OTUS_SoftwareArchitect_Client.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.ViewModels
{
    public class CreateTaskViewModel : BaseViewModel, IViewLoadingAware
    {
        private readonly TasksService _tasksService;
        private readonly ListsService _listsService;
        private readonly ProjectsService _projectsService;
        private readonly LabelsService _labelsService;
        private readonly string _requestId;

        private string _title;
        private string _description;
        private DateTime? _dueDate;
        private IEnumerable<ListModel> _lists;
        private List<SimpleUserModel> _allAvailableMembers;
        private Dictionary<string, List<SimpleUserModel>> _membersByProjectDict = new Dictionary<string, List<SimpleUserModel>>();
        private ObservableCollection<object> _selectedMembers = new ObservableCollection<object>();
        private ListModel _selectedList;
        private IEnumerable<LabelModel> _allLabels;
        private ObservableCollection<object> _selectedLabels = new ObservableCollection<object>();

        public CreateTaskViewModel()
        {
            _tasksService = DependencyService.Resolve<TasksService>();
            _listsService = DependencyService.Resolve<ListsService>();
            _projectsService = DependencyService.Resolve<ProjectsService>();
            _labelsService = DependencyService.Resolve<LabelsService>();

            _requestId = RequestIdProvider.GetRequestId();

            CreateCommand = new AsyncCommand(CreateTaskAsync);
            PickMembersCommand = new Command(OnPickMembersTapped);
            PickLabelsCommand = new Command(OnPickLabelsTapped);

            DueDate = DateTime.Now.AddDays(7);
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
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

        public DateTime? DueDate
        {
            get => _dueDate;
            set
            {
                _dueDate = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<ListModel> Lists
        {
            get => _lists;
            set
            {
                _lists = value;
                OnPropertyChanged();
            }
        }

        public ListModel SelectedList
        {
            get => _selectedList;
            set
            {
                _selectedList = value;
                OnPropertyChanged();
                UpdateAvailableMembers();
                Members?.Clear();
            }
        }

        public List<SimpleUserModel> AllUsers
        {
            get => _allAvailableMembers;
            set
            {
                _allAvailableMembers = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<LabelModel> AllLabels
        {
            get => _allLabels;
            set
            {
                _allLabels = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<object> SelectedLabels
        {
            get => _selectedLabels;
            set
            {
                _selectedLabels = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<object> Members
        {
            get => _selectedMembers;
            set
            {
                _selectedMembers = value;
                OnPropertyChanged();
            }
        }


        public ICommand CreateCommand { get; }
        public ICommand PickMembersCommand { get; }
        public ICommand PickLabelsCommand { get; }

        public event EventHandler TaskCreated;
        public event EventHandler PickMembersRequested;
        public event EventHandler PickLabelsRequested;



        protected async Task CreateTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                ShowToast("Title can't be empty");
                return;
            }

            if (SelectedList == null)
            {
                ShowToast("List must be specified");
                return;
            }

            IsBusy = true;

            try
            {
                var dueDate = DueDate.HasValue
                    ? new DateTimeOffset(DueDate.Value, DateTimeOffset.Now.Offset)
                    : (DateTimeOffset?)null;

                var dto = GetTaskDto();
                dto.Title = Title;
                dto.ListId = SelectedList.Id;
                dto.Description = Description;
                dto.DueDate = dueDate;
                dto.MembersIds = Members.Select(m => (m as BaseModel).Id).ToList();
                dto.LabelsIds = SelectedLabels?.Select(l => (l as BaseModel).Id).ToList() ?? new List<string>();

                await SendDtoAsync(dto);
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected virtual async Task SendDtoAsync(CreateTaskDto dto)
        {
            var creationResult = await _tasksService.CreateTaskAsync(_requestId, dto);
            if (!creationResult.IsSuccess)
            {
                ShowToast(creationResult.GetFullMessage());
                return;
            }

            TaskCreated?.Invoke(this, EventArgs.Empty);
        }

        protected virtual CreateTaskDto GetTaskDto()
        {
            return new CreateTaskDto();
        }

        protected virtual async Task InitializeTaskRelatedDataAsync()
        {
            IsBusy = true;

            try
            {
                var listsResultTask = _listsService.GetLists();
                var membersResultTask = _projectsService.GetProjectsMembersAsync();
                var labelsResultTask = _labelsService.GetLabels();

                await Task.WhenAll(listsResultTask, membersResultTask, labelsResultTask);

                var listsResult = listsResultTask.Result;
                var membersResult = membersResultTask.Result;
                var labelsResult = labelsResultTask.Result;


                if (!listsResult.IsSuccess)
                {
                    ShowToast(listsResult.GetFullMessage());
                    return;
                }

                if (!membersResult.IsSuccess)
                {
                    ShowToast(membersResult.GetFullMessage());
                    return;
                }

                if (!labelsResult.IsSuccess)
                {
                    ShowToast(labelsResult.GetFullMessage());
                    return;
                }

                Lists = listsResult.Result;
                SelectedList = Lists.FirstOrDefault();
                AllLabels = labelsResult.Result;
                SetMembers(membersResult.Result);
            }
            catch (Exception e)
            {

            }
            finally
            {
                IsBusy = false;
            }
        }

        private void SetMembers(IEnumerable<ProjectMemberModel> allMembers)
        {
            var membersGroupedByProject = allMembers.GroupBy(member => member.ProjectId);

            Dictionary<string, List<SimpleUserModel>> projectMemberMap = new Dictionary<string, List<SimpleUserModel>>();

            foreach (var projectMembers in membersGroupedByProject)
            {
                projectMemberMap.Add(
                    projectMembers.Key,
                    projectMembers.Select(m => new SimpleUserModel { Id = m.UserId, Username = m.Username }).ToList());
            }

            _membersByProjectDict = projectMemberMap;

            UpdateAvailableMembers();
        }

        private void UpdateAvailableMembers()
        {
            if (SelectedList != null)
            {
                if (_membersByProjectDict.TryGetValue(SelectedList.ProjectId, out List<SimpleUserModel> selectedProjectMembers))
                {
                    AllUsers = selectedProjectMembers;
                }
                else
                {
                    AllUsers = new List<SimpleUserModel>();
                }
            }
        }


        public void OnViewAppearing()
        {
            InitializeTaskRelatedDataAsync();
        }

        private void OnPickMembersTapped()
        {
            PickMembersRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnPickLabelsTapped()
        {
            PickLabelsRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
