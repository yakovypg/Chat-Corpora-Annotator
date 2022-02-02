using ChatCorporaAnnotator.Infrastructure.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatCorporaAnnotator.Controlling.Controllers
{
    internal class TaskController : ITaskController
    {
        private readonly Dictionary<Task, TaskFeature> _tasks;

        public IReadOnlyDictionary<Task, TaskFeature> Tasks => _tasks;  
        public bool IsWindowReadOnly => _tasks.ContainsValue(TaskFeature.MakeWindowReadOnly);

        public TaskController()
        {
            _tasks = new Dictionary<Task, TaskFeature>();
        }

        public void AddTask(Task task, TaskFeature taskFeature = TaskFeature.Default)
        {
            _tasks.Add(task, taskFeature);
        }

        public bool RemoveTask(Task task)
        {
            return _tasks.Remove(task);
        }
    }
}
