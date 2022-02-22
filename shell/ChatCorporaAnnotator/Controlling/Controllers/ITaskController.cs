using ChatCorporaAnnotator.Infrastructure.Enums;
using System.Threading.Tasks;

namespace ChatCorporaAnnotator.Controlling.Controllers
{
    internal interface ITaskController
    {
        void AddTask(Task task, TaskFeature taskFeature = TaskFeature.Default);
        bool RemoveTask(Task task);
    }
}
