using SofineProject.ViewModels.BasketViewModels;
using SofineProject.ViewModels.WishlistViewModels;

namespace SofineProject.Interfaces
{
    public interface ILayoutService
    {
        Task<IEnumerable<BasketVM>> GetBaskets();

        Task<IEnumerable<WishlistVM>> GetWishlists();
    }
}
