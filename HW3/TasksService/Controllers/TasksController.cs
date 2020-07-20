using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TasksService
{
    [ApiController]
    [Route("[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TasksRepository _repository;

        public TasksController()
        {
            _repository = new TasksRepository();
        }

        [HttpPost]
        public Task<TaskModel> CreateTask(string title, string assignedTo)
        {
            return _repository.CreateTask(title, assignedTo);
        }

        [HttpGet]
        public Task<IEnumerable<TaskModel>> GetTasks()
        {
            return _repository.GetTasks();
        }

        [HttpGet("{id}")]
        public Task<TaskModel> GetTask(string id)
        {
            return _repository.GetTask(id);
        }

        [HttpPut]
        public Task<TaskModel> UpdateTask(TaskModel model)
        {
            return _repository.UpdateTask(model);
        }


        [HttpDelete("{id}")]
        public Task<bool> DeleteTask(string id)
        {
            return _repository.DeleteTask(id);
        }

        [HttpDelete("clear")]
        public Task<bool> ClearTasks()
        {
            return _repository.ClearTasks();
        }
    }
}
