using EnterpriseCRM.Application.DTOs;
using System.Net.Http.Json;

namespace EnterpriseCRM.BlazorServer.Services;

public interface IProductClientService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductDto product);
    Task<ProductDto> UpdateAsync(UpdateProductDto product);
    Task DeleteAsync(int id);
}

public class ProductClientService : IProductClientService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://localhost:5001/api/products";

    public ProductClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync(_baseUrl);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>() ?? Enumerable.Empty<ProductDto>();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/{id}");
        if (!response.IsSuccessStatusCode)
            return null;
        return await response.Content.ReadFromJsonAsync<ProductDto>();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto product)
    {
        var response = await _httpClient.PostAsJsonAsync(_baseUrl, product);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>() ?? throw new Exception("Failed to create product");
    }

    public async Task<ProductDto> UpdateAsync(UpdateProductDto product)
    {
        var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{product.Id}", product);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>() ?? throw new Exception("Failed to update product");
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
        response.EnsureSuccessStatusCode();
    }
}