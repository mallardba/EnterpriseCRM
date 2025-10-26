using EnterpriseCRM.Core.Entities;
using EnterpriseCRM.Core.Interfaces;
using EnterpriseCRM.Application.Interfaces;
using EnterpriseCRM.Application.DTOs;
using AutoMapper;
using System.Linq;

namespace EnterpriseCRM.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        return product != null ? _mapper.Map<ProductDto>(product) : null;
    }

    public async Task<PagedResultDto<ProductDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        var totalCount = products.Count();

        var pagedProducts = products
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResultDto<ProductDto>
        {
            Data = _mapper.Map<IEnumerable<ProductDto>>(pagedProducts),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResultDto<ProductDto>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        var products = await _unitOfWork.Products.SearchAsync(searchTerm);
        var totalCount = products.Count();

        var pagedProducts = products
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResultDto<ProductDto>
        {
            Data = _mapper.Map<IEnumerable<ProductDto>>(pagedProducts),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto createDto, string currentUser)
    {
        var product = new Product
        {
            Name = createDto.Name,
            Description = createDto.Description,
            SKU = createDto.SKU,
            Price = createDto.Price,
            Cost = createDto.Cost,
            Category = createDto.Category,
            IsActive = createDto.IsActive,
            CreatedBy = currentUser,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(UpdateProductDto updateDto, string currentUser)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(updateDto.Id);
        if (product == null)
            throw new ArgumentException($"Product with ID {updateDto.Id} not found.");

        product.Name = updateDto.Name;
        product.Description = updateDto.Description;
        product.SKU = updateDto.SKU;
        product.Price = updateDto.Price;
        product.Cost = updateDto.Cost;
        product.Category = updateDto.Category;
        product.IsActive = updateDto.IsActive;
        product.UpdatedBy = currentUser;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(int id)
    {
        await _unitOfWork.Products.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category)
    {
        var products = await _unitOfWork.Products.GetByCategoryAsync(category);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
    {
        var products = await _unitOfWork.Products.GetActiveProductsAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetBySKUAsync(string sku)
    {
        var product = await _unitOfWork.Products.GetBySKUAsync(sku);
        return product != null ? _mapper.Map<ProductDto>(product) : null;
    }
}