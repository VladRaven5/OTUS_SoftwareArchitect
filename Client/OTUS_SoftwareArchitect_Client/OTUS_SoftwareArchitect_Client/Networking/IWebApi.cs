﻿using OTUS_SoftwareArchitect_Client.DTO.AuthDtos;
using OTUS_SoftwareArchitect_Client.DTO.ListDtos;
using OTUS_SoftwareArchitect_Client.DTO.ProjectDtos;
using OTUS_SoftwareArchitect_Client.DTO.TaskDtos;
using OTUS_SoftwareArchitect_Client.DTO.WorkingHours;
using OTUS_SoftwareArchitect_Client.Models;
using OTUS_SoftwareArchitect_Client.Models.ProjectModels;
using OTUS_SoftwareArchitect_Client.Models.TaskModels;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OTUS_SoftwareArchitect_Client.Networking
{
    public interface IWebApi
    {      
        #region Auth
        [Post("/login")]
        Task<string> Login([Body] LoginDto loginDto);

        [Post("/register")]
        Task<string> Register([Body] RegisterDto dto);

        [Get("/logout")]
        Task<string> Logout();

        [Get("/auth")]
        Task<string> Auth();

        #endregion Auth

        #region Users

        [Get("/users")]
        Task<IEnumerable<UserModel>> GetUsers();

        [Get("/users/{userId}")]
        Task<UserModel> GetUser(string userId);

        [Put("/users")]
        Task<UserModel> UpdateUser(UpdateUserDto dto);


        #endregion Users

        #region Tasks

        [Get("/tasks/my")]
        Task<IEnumerable<TaskModel>> GetMyTasks();

        [Get("/tasks/{taskId}")]
        Task<TaskModel> GetTask(string taskId);

        [Post("/tasks")]
        Task<TaskModel> CreateTask(
            [Header(Constants.RequestIdHeaderName)] string requestId,
            [Body] CreateTaskDto dto);

        [Put("/tasks")]
        Task<TaskModel> UpdateTask([Body] UpdateTaskDto dto);

        [Delete("/tasks/{taskId}")]
        Task<string> DeleteTask(string taskId);

        #endregion Tasks

        #region Projects

        [Get("/projects")]
        Task<IEnumerable<ProjectModel>> GetProjects();

        [Get("/projects/{projectId}")]
        Task<ProjectModel> GetProject(string projectId);

        [Post("/projects")]
        Task<ProjectModel> CreateProject(
            [Header(Constants.RequestIdHeaderName)] string requestId,
            [Body] CreateProjectDto dto);

        [Put("/projects")]
        Task<ProjectModel> UpdateProject([Body] UpdateProjectDto dto);


        [Delete("/projects/{projectId}")]
        Task DeleteProject(string projectId);

        #endregion Projects

        #region Project members

        [Get("/project-members")]
        Task<IEnumerable<ProjectMemberModel>> GetProjectMembers(string projectId);

        [Get("/project-members")]
        Task<IEnumerable<ProjectMemberModel>> GetProjectsMembers();

        [Post("/project-members")]
        Task<string> AddMemberToProject(
            [Header(Constants.RequestIdHeaderName)] string requestId, 
            [Body] CreateUpdateProjectMemberDto dto);

        [Delete("/project-members")]
        Task RemoveMemberFromProject([Query] DeleteProjectMemberDto dto);


        #endregion Project members

        #region Labels

        [Get("/labels")]
        Task<IEnumerable<LabelModel>> GetLabels();

        #endregion Labels

        #region Lists

        [Get("/lists/of-project/{projectId}")]
        Task<IEnumerable<ListModel>> GetProjectLists(string projectId);

        [Get("/lists")]
        Task<IEnumerable<ListModel>> GetLists();

        [Post("/lists")]
        Task<ListModel> CreateList(
            [Header(Constants.RequestIdHeaderName)] string requestId,
            [Body] CreateListDto dto);

        [Put("/lists")]
        Task<ListModel> UpdateList([Body] UpdateListDto dto);

        [Delete("/lists/{listId}")]
        Task DeleteList(string listId);


        #endregion Lists

        #region Notifications

        [Get("/notifications/my")]
        Task<IEnumerable<NotificationModel>> GetMyNotifications();

        #endregion Notifications

        #region Working Hours

        [Get("/working-hours/my")]
        Task<IEnumerable<WorkingHoursRecordModel>> GetMyWorkingHours();

        [Post("/working-hours")]
        Task<WorkingHoursRecordModel> CreateWorkingHoursRecord(
            [Header(Constants.RequestIdHeaderName)] string requestId,
            [Body] CreateWorkingHoursRecordDto dto);

        [Put("/working-hours")]
        Task<WorkingHoursRecordModel> UpdateWorkingHoursRecord(UpdateWorkingHoursRecordDto dto);

        [Delete("/working-hours/{recordId}")]
        Task DeleteWorkingHoursRecord(string recordId);


        #endregion Working Hours
    }

    public interface ILocalWebApi
    {
        #region Auth
        [Post("/login")]
        Task<string> Login([Body] LoginDto loginDto);

        [Post("/register")]
        Task<string> Register([Body] RegisterDto dto);

        [Get("/logout")]
        Task<string> Logout();

        [Get("/auth")]
        Task<string> Auth();

        #endregion Auth

        #region Tasks

        [Get("/my")]
        //[Headers("X-UserId: 2                                   ")]
        Task<IEnumerable<TaskModel>> GetMyTasks([Header("X-UserId")] string userId);

        #endregion Tasks




    }
}
