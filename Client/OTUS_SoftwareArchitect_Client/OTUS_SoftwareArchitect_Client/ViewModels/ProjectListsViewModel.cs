using OTUS_SoftwareArchitect_Client.DTO.ListDtos;
using OTUS_SoftwareArchitect_Client.Infrastructure;
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
    public class ProjectListsViewModel : BaseViewModel, IViewLoadingAware
    {
        private readonly ListsService _listsService;
        private readonly string _projectId;

        public ProjectListsViewModel(string projectId)
        {
            _listsService = DependencyService.Resolve<ListsService>();
            _projectId = projectId;

            RefreshCommand = new AsyncCommand(RefrechListsAsync);

            EditListCommand = new AsyncCommand<object>(EditListAsync);
            DeleteListCommand = new AsyncCommand<object>(DeleteListAsync);
            CreateListCommand = new AsyncCommand(CreateListAsync);
        }

        private IEnumerable<ListModel> _lists;

        public IEnumerable<ListModel> Lists
        {
            get => _lists;
            set 
            {
                _lists = value;
                OnPropertyChanged();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand CreateListCommand { get; set; }
        public ICommand DeleteListCommand { get; set; }
        public ICommand EditListCommand { get; set; }

        public Func<PromptConfig, Task<string>> DisplayPromptAsync;

        public void OnViewAppearing()
        {
            RefreshCommand?.Execute(null);
        }

        private async Task RefrechListsAsync()
        {
            IsBusy = true;

            try
            {
                var projectListsResult = await _listsService.GetProjectLists(_projectId);
                if(!projectListsResult.IsSuccess)
                {
                    ShowToast(projectListsResult.GetFullMessage());
                    return;
                }

                Lists = projectListsResult.Result;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CreateListAsync()
        {
            var title = await DisplayPromptAsync(new PromptConfig
            {
                Title = "New list",
                Message = "Enter title",
                Placeholder = "Title",
                OkText = "Create",
                CancelText = "Cancel",
            });

            if (title != null)
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    ShowToast("Canceled. Title can't be empty");
                    return;
                }

                var dto = new CreateListDto
                {
                    Title = title,
                    ProjectId = _projectId
                };

                var creationResult = await _listsService.CreateListAsync(dto);
                if (!creationResult.IsSuccess)
                {
                    ShowToast(creationResult.GetFullMessage());
                    return;
                }

                ShowToast($"List {title} created");
            }
        }

        private async Task EditListAsync(object listObject)
        {
            var list = listObject as ListModel;

            var newTitle = await DisplayPromptAsync(new PromptConfig
            {
                Title = "Rename list",
                Message = "Enter new title",
                Placeholder = "New Title",
                InitialText = list.Title,
                OkText = "Save",
                CancelText = "Cancel",
            });

            if (newTitle != null)
            {
                if (string.IsNullOrWhiteSpace(newTitle))
                {
                    ShowToast("Canceled. Title can't be empty");
                    return;
                }

                var updateResult = await _listsService.UpdateListAsync(list, newTitle);
                if (!updateResult.IsSuccess)
                {
                    ShowToast(updateResult.GetFullMessage());
                    return;
                }

                ShowToast($"List updated");
            }          
        }

        private async Task DeleteListAsync(object listObject)
        {
            var list = listObject as ListModel;
            var deletionResult = await _listsService.DeleteListAsync(list.Id);
            if(!deletionResult.IsSuccess)
            {
                ShowToast(deletionResult.GetFullMessage());
                return;
            }

            ShowToast($"Deleted");
        }                
    }
}
