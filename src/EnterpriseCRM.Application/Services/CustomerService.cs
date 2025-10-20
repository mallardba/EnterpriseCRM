using EnterpriseCRM.Core.Entities;
using EnterpriseCRM.Core.Interfaces;
using EnterpriseCRM.Application.Interfaces;
using EnterpriseCRM.Application.DTOs;
using System.Threading.Tasks;
using AutoMapper;

namespace EnterpriseCRM.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
    }

    public async Task<PagedResultDto<CustomerDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        var customers = await _unitOfWork.Customers.GetAllAsync();
        var totalCount = customers.Count();

        var pagedCustomers = customers
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResultDto<CustomerDto>
        {
            Data = _mapper.Map<IEnumerable<CustomerDto>>(pagedCustomers),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto createDto, string currentUser)
    {
        var customer = new Customer
        {
            CompanyName = createDto.CompanyName,
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            Email = createDto.Email,
            Phone = createDto.Phone,
            Address = createDto.Address,
            City = createDto.City,
            State = createDto.State,
            PostalCode = createDto.PostalCode,
            Country = createDto.Country,
            Industry = createDto.Industry,
            Type = createDto.Type,
            Status = createDto.Status,
            CreatedBy = currentUser,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto> UpdateAsync(UpdateCustomerDto updateDto, string currentUser)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(updateDto.Id);
        if (customer == null)
            throw new ArgumentException($"Customer with ID {updateDto.Id} not found.");

        customer.CompanyName = updateDto.CompanyName;
        customer.FirstName = updateDto.FirstName;
        customer.LastName = updateDto.LastName;
        customer.Email = updateDto.Email;
        customer.Phone = updateDto.Phone;
        customer.Address = updateDto.Address;
        customer.City = updateDto.City;
        customer.State = updateDto.State;
        customer.PostalCode = updateDto.PostalCode;
        customer.Country = updateDto.Country;
        customer.Industry = updateDto.Industry;
        customer.Type = updateDto.Type;
        customer.Status = updateDto.Status;
        customer.UpdatedBy = currentUser;
        customer.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Customers.UpdateAsync(customer);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer == null)
            throw new ArgumentException($"Customer with ID {id} not found.");

        customer.IsDeleted = true;
        customer.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Customers.UpdateAsync(customer);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PagedResultDto<CustomerDto>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        var customers = await _unitOfWork.Customers.SearchAsync(searchTerm);
        var totalCount = customers.Count();

        var pagedCustomers = customers
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResultDto<CustomerDto>
        {
            Data = _mapper.Map<IEnumerable<CustomerDto>>(pagedCustomers),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<CustomerDto>> GetByStatusAsync(CustomerStatus status)
    {
        var customers = await _unitOfWork.Customers.GetByStatusAsync(status);
        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }

    public async Task<IEnumerable<CustomerDto>> GetByTypeAsync(CustomerType type)
    {
        var customers = await _unitOfWork.Customers.GetByTypeAsync(type);
        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }

    public async Task<CustomerDto?> GetByEmailAsync(string email)
    {
        var customer = await _unitOfWork.Customers.GetByEmailAsync(email);
        return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
    }

    public async Task<IEnumerable<CustomerDto>> GetRecentAsync(int count)
    {
        var customers = await _unitOfWork.Customers.GetRecentAsync(count);
        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }
}
