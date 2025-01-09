using BusinessObject.ViewModels.Categories;
using BusinessObject.Models;
using Repository;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using AutoMapper;
using Services.Responses;

namespace Services.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<Category>> CreateCategory(CategoryCreateRequest categoryRequest)
        {
            var category = _mapper.Map<Category>(categoryRequest);

            
            await _unitOfWork.CategoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<Category>
            {
                Succeeded = true,
                Message = "Tạo thành công.",
                Errors = new[] { "No data available." },
                Data = category
            };
        }

        public async Task<ApiResponse<bool>> DeleteCategory(Guid categoryId)
        {
            
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);

            
            if (category == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy thể loại.",
                    Errors = new[] { "The specified category does not exist." },
                    Data = false
                };
            }

            // Check if category use by one or more books
            var hasBooks = await _unitOfWork.BookRepository
                .AnyAsync(book => book.Categories.Any(category => category.Id == categoryId) );
            
            if (hasBooks)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Xóa thất bại.",
                    Errors = new[] { "The category is in use and cannot be deleted." },
                    Data = false
                };
            }

            _unitOfWork.CategoryRepository.Delete(category);

            
            var result = await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = result,
                Message = result ? "Xóa thành công." : "Xóa thất bại.",
                Data = result
            };
        }
        public async Task<ApiResponse<bool>> SoftDeleteCategory(Guid categoryId)
        {

            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);


            if (category == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy thể loại.",
                    Errors = new[] { "The specified category does not exist." },
                    Data = false
                };
            }

            _unitOfWork.CategoryRepository.SoftDelete(category);
            var result = await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = result,
                Message = result ? "Xóa thành công." : "Xóa thất bại.",
                Data = result
            };
        }


        public async Task<ApiResponse<List<Category>>> GetAllCategory()
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();

            if (categories == null || !categories.Any())
            {
                return new ApiResponse<List<Category>>
                {
                    Succeeded = false,
                    Message = "No categories found.",
                    Errors = new[] { "No data available." },
                    Data = null
                };
            }

            return new ApiResponse<List<Category>>(categories);
        }

        public async Task<ApiResponse<Category>> GetCategoryById(Guid categoryId)
        {
            
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
      
            if (category == null)
            {
                return new ApiResponse<Category>
                {
                    Succeeded = false,
                    Message = $"Category with ID {categoryId} not found.",
                    Errors = new[] { "Category not found." },
                    Data = null
                };
            }

            // Trả về response với dữ liệu category
            return new ApiResponse<Category>(category);
        }

        public async Task<ApiResponse<Category>> UpdateCategory(CategoryUpdateRequest categoryRequest)
        {
            var existingCategory = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryRequest.Id);

            if (existingCategory == null)
            {
                return new ApiResponse<Category>
                {
                    Succeeded = false,
                    Message = $"Category with ID {categoryRequest.Id} not found.",
                    Errors = new[] { "Category not found." },
                    Data = null
                };
            }

            // Check if category is used by one or more books
            var hasBooks = await _unitOfWork.BookRepository
                .AnyAsync(book => book.Categories.Any(c => c.Id == categoryRequest.Id));

            if (hasBooks)
            {
                return new ApiResponse<Category>
                {
                    Succeeded = false,
                    Message = "Category cannot be updated because it is being used by one or more books.",
                    Errors = new[] { "Category is in use." },
                    Data = null
                };
            }

            _mapper.Map(categoryRequest, existingCategory);

            _unitOfWork.CategoryRepository.Update(existingCategory);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<Category>(existingCategory);
        }



    }
}
