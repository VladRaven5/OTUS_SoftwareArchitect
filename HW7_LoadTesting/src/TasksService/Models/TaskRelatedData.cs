using System.Collections.Generic;
using System.Linq;

namespace TasksService
{
    public class TaskRelatedData
        {
            public ListModelAggregate List { get; set; }
            public IEnumerable<UserModel> Users { get; set; }
            public IEnumerable<LabelModel> Labels { get; set; }

            public TaskCollections ToTaskCollections()
            {          
                return new TaskCollections
                {
                    Members = Users.Select(u => u.Id).ToList(),
                    Labels = Labels.Select(l => l.Id).ToList()
                };
            }
        }
}