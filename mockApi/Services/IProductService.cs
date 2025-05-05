using mockApi.Models.Dtos;

namespace mockApi.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
    Task<IReadOnlyCollection<CategoryDTO>> GetCategoryInfoAsync();
}