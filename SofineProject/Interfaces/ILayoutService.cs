using SofineProject.ViewModels.BasketViewModels;

namespace SofineProject.Interfaces
{
    public interface ILayoutService
    {
        Task<IEnumerable<BasketVM>> GetBaskets();
    }
}
