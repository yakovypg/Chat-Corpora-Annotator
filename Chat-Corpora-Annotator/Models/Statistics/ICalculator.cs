using ChatCorporaAnnotator.Infrastructure.Enums;
using System.Threading.Tasks;

namespace ChatCorporaAnnotator.Models.Statistics
{
    internal interface ICalculator
    {
        long CurrentProgressValue { get; }
        long MaxProgressValue { get; }

        OperationState CalculatingState { get; }

        Task CalculateAsync();
    }
}
