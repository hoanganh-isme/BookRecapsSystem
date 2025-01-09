using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.ViewModels.Categories;
using Services.Responses;

namespace Services.Interface
{
    public interface ICategoryService
    {
        Task<ApiResponse<Category>> CreateCategory(CategoryCreateRequest category);
        Task<ApiResponse<Category>> UpdateCategory(CategoryUpdateRequest category);
        Task<ApiResponse<bool>> DeleteCategory(Guid categoryId);
        Task<ApiResponse<bool>> SoftDeleteCategory(Guid categoryId);
        Task<ApiResponse<Category>> GetCategoryById(Guid categoryId);
        Task<ApiResponse<List<Category>>> GetAllCategory();

    }
}
