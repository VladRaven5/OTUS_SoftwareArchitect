using OTUS_SoftwareArchitect_Client.Models;
using OTUS_SoftwareArchitect_Client.Networking.Misc;
using OTUS_SoftwareArchitect_Client.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.ViewModels
{
    public class ProjectsViewModel : BaseViewModel, IViewLoadingAware
    {
        private readonly ProjectsService _projectsService;
        private IEnumerable<ProjectModel> _projects;

        public ProjectsViewModel()
        {
            _projectsService = DependencyService.Resolve<ProjectsService>();

            RefreshCommand = new AsyncCommand(RefreshCollectionAsync);
            CreateProjectCommand = new Command(CreateProject);
        }

        public IEnumerable<ProjectModel> Projects
        {
            get => _projects;
            set
            {
                _projects = value;
                OnPropertyChanged();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand CreateProjectCommand { get; }

        public event EventHandler CreateProjectRequested;


        public void OnViewAppearing()
        {
            RefreshCommand.Execute(null);
        }

        private void CreateProject()
        {
            CreateProjectRequested?.Invoke(this, EventArgs.Empty);
        }

        private async Task RefreshCollectionAsync()
        {
            IsBusy = true;

            try
            {
                var myTasksRequest = await _projectsService.GetProjectsAsync();

                if (!myTasksRequest.IsSuccess)
                {
                    ShowToast(myTasksRequest.GetFullMessage());
                    return;
                }

                Projects = myTasksRequest.Result;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
