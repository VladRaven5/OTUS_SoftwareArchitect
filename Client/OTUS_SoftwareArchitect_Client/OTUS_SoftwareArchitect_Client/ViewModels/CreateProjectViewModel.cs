using OTUS_SoftwareArchitect_Client.DTO.ProjectDtos;
using OTUS_SoftwareArchitect_Client.Networking.Misc;
using OTUS_SoftwareArchitect_Client.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.ViewModels
{
    public class CreateProjectViewModel : BaseViewModel
    {
        private readonly ProjectsService _projectsService;
        private readonly string _requestId;

        private string _title;
        private string _description;
        private DateTime? _beginDate;
        private DateTime? _endDate;

        public CreateProjectViewModel()
        {
            _projectsService = DependencyService.Resolve<ProjectsService>();
            _requestId = RequestIdProvider.GetRequestId();

            CreateCommand = new AsyncCommand(CreateAsync);

            BeginDate = DateTime.Now;
            EndDate = BeginDate + TimeSpan.FromDays(30);
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

        public ICommand CreateCommand { get; }

        public event EventHandler ProjectCreated;


        private async Task CreateAsync()
        {
            if(string.IsNullOrWhiteSpace(Title))
            {
                ShowToast("Project title can't be empty!");
                return;
            }

            IsBusy = true;

            try
            {
                DateTimeOffset? beginDate = null;
                if(BeginDate.HasValue)
                {
                    beginDate = new DateTimeOffset(BeginDate.Value, DateTimeOffset.Now.Offset);
                }

                DateTimeOffset? endDate = null;
                if (EndDate.HasValue)
                {
                    endDate = new DateTimeOffset(EndDate.Value, DateTimeOffset.Now.Offset);
                }

                var dto = new CreateProjectDto
                {
                    Title = Title,
                    Description = Description,
                    BeginDate = beginDate,
                    EndDate = endDate
                };

                var createdProjectResult = await _projectsService.CreateProjectAsync(_requestId, dto);
                
                if(!createdProjectResult.IsSuccess)
                {
                    ShowToast(createdProjectResult.GetFullMessage());
                    return;
                }

                ProjectCreated?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
