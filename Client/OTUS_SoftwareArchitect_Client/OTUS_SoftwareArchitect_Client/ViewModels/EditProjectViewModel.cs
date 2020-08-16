using OTUS_SoftwareArchitect_Client.DTO.ProjectDtos;
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
    public class EditProjectViewModel : BaseViewModel
    {
        private readonly string _projectId;
        private readonly ProjectsService _projectsService;
        private readonly UsersService _usersService;

        private string _title;
        private string _description;
        private int _projectVersion;
        private DateTime? _beginDate;
        private DateTime? _endDate;
        private List<SimpleUserModel> _initialMembers;
        private ObservableCollection<object> _projectMembers;
        private List<SimpleUserModel> _allUsers;

        public EditProjectViewModel(string projectId)
        {
            _projectId = projectId;

            _projectsService = DependencyService.Resolve<ProjectsService>();
            _usersService = DependencyService.Resolve<UsersService>();

            SaveCommand = new AsyncCommand(SaveAsync);
            DeleteCommand = new AsyncCommand(DeleteAsync);
            UpdateMembersCommand = new Command(OnUpdateMembersClicked);

            Task.Run(async () => await InitializeAsync());
        }


        private async Task InitializeAsync()
        {
            IsBusy = true;

            try
            {
                var projectResultTask = _projectsService.GetProjectAsync(_projectId);
                var membersResultTask = _projectsService.GetProjectMembers(_projectId);
                var usersResultTask = _usersService.GetUsersAsync();

                await Task.WhenAll(projectResultTask, membersResultTask, usersResultTask);

                var projectResult = projectResultTask.Result;
                var membersResult = membersResultTask.Result;
                var usersResult = usersResultTask.Result;


                if (!projectResult.IsSuccess)
                {
                    ShowToast(projectResult.GetFullMessage());
                    return;
                }

                if (!membersResult.IsSuccess)
                {
                    ShowToast(membersResult.GetFullMessage());
                    return;
                }

                if (!usersResult.IsSuccess)
                {
                    ShowToast(usersResult.GetFullMessage());
                    return;
                }


                SetMembers(usersResult.Result, membersResult.Result);
                SetProjectProperties(projectResult.Result);
            }
            finally
            {
                IsBusy = false;
            }          
        }

        private void SetMembers(IEnumerable<UserModel> users, IEnumerable<ProjectMemberModel> members)
        {
            var simpleUsers = users.Select(u => new SimpleUserModel { Id = u.Id, Username = u.Username }).ToList();

            var simpleUsersDict = simpleUsers.ToDictionary(su => su.Id);

            var simpleMembers = new List<SimpleUserModel>();

            foreach (var member in members)
            {
                simpleMembers.Add(simpleUsersDict[member.UserId]);
            }

            _initialMembers = simpleMembers.ToList();
            ProjectMembers = new ObservableCollection<object>(simpleMembers.Cast<object>());
            AllUsers = simpleUsers;
        }

        private void SetProjectProperties(ProjectModel project)
        {
            Title = project.Title;
            Description = project.Description;
            _projectVersion = project.Version;
            BeginDate = project.BeginDate?.LocalDateTime;
            EndDate = project.EndDate?.LocalDateTime;
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

        public DateTime? BeginDate
        {
            get => _beginDate;
            set
            {
                _beginDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<object> ProjectMembers
        {
            get => _projectMembers;
            private set
            {
                _projectMembers = value;
                OnPropertyChanged();
            }
        }

        public List<SimpleUserModel> AllUsers
        { 
            get => _allUsers; 
            private set 
            {
                _allUsers = value;
                OnPropertyChanged();
            }
        }

        public ICommand UpdateMembersCommand { get; set; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }

        public event EventHandler UpdateMembersRequested;
        public event EventHandler ProjectSaved;
        public event EventHandler ProjectDeleted;


        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                ShowToast("Project title can't be empty!");
                return;
            }

            IsBusy = true;

            try
            {
                DateTimeOffset? beginDate = null;
                if (BeginDate.HasValue)
                {
                    beginDate = new DateTimeOffset(BeginDate.Value, DateTimeOffset.Now.Offset);
                }

                DateTimeOffset? endDate = null;
                if (EndDate.HasValue)
                {
                    endDate = new DateTimeOffset(EndDate.Value, DateTimeOffset.Now.Offset);
                }

                var dto = new UpdateProjectDto
                {
                    Id = _projectId,
                    Title = Title,
                    Description = Description,
                    BeginDate = beginDate,
                    EndDate = endDate,
                    Version = _projectVersion,
                };

                var updatedProjectResult = await _projectsService.UdpateProjectAsync(dto, 
                    _initialMembers,
                    ProjectMembers.Cast<SimpleUserModel>().ToList());

                if (!updatedProjectResult.IsSuccess)
                {
                    ShowToast(updatedProjectResult.GetFullMessage());
                    return;
                }

                ProjectSaved?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnUpdateMembersClicked()
        {
            UpdateMembersRequested?.Invoke(this, EventArgs.Empty);
        }

        private async Task DeleteAsync()
        {
            IsBusy = true;
            try
            {
                var deletionResult = await _projectsService.DeleteProjectAsync(_projectId);
                if(!deletionResult.IsSuccess)
                {
                    ShowToast(deletionResult.GetFullMessage());
                    return;
                }

                ProjectDeleted?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsBusy = false;
            }            
        }
    }

    public class SimpleUserModel : BaseModel
    {
        public string Username { get; set; }
    }
}
