# Test Driven Development (TDD)

## 🧪 **Overview**

**Test Driven Development (TDD)** is a software development methodology where you write tests **before** writing the actual code. It's a cycle of Red-Green-Refactor that ensures your code is well-tested and meets requirements.

## 🔄 **The TDD Cycle: Red-Green-Refactor**

### **1. 🔴 RED - Write a Failing Test**
```csharp
[Fact]
public void GetCustomerById_WhenCustomerExists_ShouldReturnCustomer()
{
    // Arrange
    var customerId = 1;
    var expectedCustomer = new Customer { Id = customerId, Name = "John Doe" };
    
    // Act
    var result = _customerService.GetById(customerId);
    
    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be("John Doe");
}
```
**Result**: Test fails because `GetById` method doesn't exist yet ❌

### **2. 🟢 GREEN - Write Minimal Code to Pass**
```csharp
public CustomerDto GetById(int id)
{
    // Minimal implementation to make test pass
    return new CustomerDto { Id = id, Name = "John Doe" };
}
```
**Result**: Test now passes ✅

### **3. 🔵 REFACTOR - Improve the Code**
```csharp
public async Task<CustomerDto> GetByIdAsync(int id)
{
    var customer = await _repository.GetByIdAsync(id);
    return _mapper.Map<CustomerDto>(customer);
}
```
**Result**: Code is now production-ready while tests still pass ✅

## 🎯 **TDD Benefits**

### **1. Better Code Quality**
- **Forces Good Design**: Writing tests first makes you think about the interface
- **Reduces Bugs**: Tests catch issues early
- **Cleaner Code**: TDD leads to more modular, testable code

### **2. Living Documentation**
- **Tests as Specs**: Tests document how the code should behave
- **Examples**: Tests show real usage examples
- **Requirements**: Tests capture business requirements

### **3. Confidence in Changes**
- **Refactoring Safety**: Tests ensure changes don't break existing functionality
- **Regression Prevention**: New bugs are caught immediately
- **Fearless Development**: You can change code knowing tests will catch issues

## 🏗️ **TDD in Your Enterprise CRM Project**

### **Current State:**
Your project already follows some TDD principles:

```csharp
// You have tests that verify CustomerService behavior
[Fact]
public async System.Threading.Tasks.Task GetByIdAsync_WhenCustomerExists_ShouldReturnCustomerDto()
{
    // This test verifies the service works correctly
    // It uses mocks to isolate the unit under test
    // It follows Arrange-Act-Assert pattern
}
```

### **TDD Approach Would Be:**
```csharp
// 1. RED: Write test first (before implementing GetByIdAsync)
[Fact]
public async Task GetByIdAsync_WhenCustomerExists_ShouldReturnCustomerDto()
{
    // Test the behavior you want
}

// 2. GREEN: Implement minimal code to pass
public async Task<CustomerDto> GetByIdAsync(int id)
{
    // Minimal implementation
}

// 3. REFACTOR: Improve implementation
public async Task<CustomerDto> GetByIdAsync(int id)
{
    // Production-ready implementation
}
```

## 📋 **TDD Rules**

### **The Three Rules of TDD:**
1. **Don't write production code** until you have a failing test
2. **Don't write more test code** than necessary to fail
3. **Don't write more production code** than necessary to pass

### **The TDD Mantra:**
> "Red, Green, Refactor, Repeat"

## 🔍 **TDD vs Traditional Testing**

### **Traditional Approach:**
```
Write Code → Write Tests → Fix Bugs
```

### **TDD Approach:**
```
Write Test → Write Code → Refactor → Repeat
```

## 🎯 **TDD in Practice**

### **Example: Adding a New Feature**

#### **Step 1: Write Test (RED)**
```csharp
[Fact]
public void CalculateCustomerValue_WhenCustomerHasOrders_ShouldReturnTotalValue()
{
    // Arrange
    var customer = new Customer { Id = 1 };
    var orders = new List<Order>
    {
        new Order { Amount = 100 },
        new Order { Amount = 200 }
    };
    
    // Act
    var totalValue = _customerService.CalculateCustomerValue(customer, orders);
    
    // Assert
    totalValue.Should().Be(300);
}
```

#### **Step 2: Implement Code (GREEN)**
```csharp
public decimal CalculateCustomerValue(Customer customer, List<Order> orders)
{
    return orders.Sum(o => o.Amount);
}
```

#### **Step 3: Refactor (BLUE)**
```csharp
public decimal CalculateCustomerValue(Customer customer, List<Order> orders)
{
    if (orders == null || !orders.Any())
        return 0;
        
    return orders.Where(o => o.Status == OrderStatus.Completed)
                 .Sum(o => o.Amount);
}
```

## 🚀 **TDD Best Practices**

### **1. Start Small**
- Write the simplest test first
- Make it pass with minimal code
- Gradually add complexity

### **2. One Test at a Time**
- Focus on one behavior per test
- Don't try to test everything at once
- Keep tests focused and clear

### **3. Test Behavior, Not Implementation**
```csharp
// Good: Tests behavior
[Fact]
public void GetCustomer_ShouldReturnCustomerWithCorrectName()

// Bad: Tests implementation details
[Fact]
public void GetCustomer_ShouldCallRepositoryGetByIdMethod()
```

### **4. Use Descriptive Test Names**
```csharp
// Good: Describes what is being tested
GetByIdAsync_WhenCustomerExists_ShouldReturnCustomerDto()
GetByIdAsync_WhenCustomerDoesNotExist_ShouldReturnNull()

// Bad: Vague names
Test1()
GetCustomer()
```

## 🔧 **TDD Tools in Your Project**

### **xUnit (Test Framework)**
```csharp
[Fact]  // Marks a test method
public void TestMethod()
{
    // Test implementation
}
```

### **Moq (Mocking)**
```csharp
// Mock dependencies to isolate the unit under test
_mockRepository.Setup(r => r.GetById(1)).Returns(customer);
```

### **FluentAssertions (Assertions)**
```csharp
// Readable assertions
result.Should().NotBeNull();
result.Name.Should().Be("John Doe");
```

## 🎯 **TDD Challenges**

### **1. Learning Curve**
- **Initial Slowness**: Writing tests first feels slower initially
- **Mindset Change**: Requires different thinking approach
- **Practice Required**: Takes time to master

### **2. When TDD is Hard**
- **UI Development**: Hard to test UI interactions
- **Legacy Code**: Adding tests to existing code is challenging
- **Complex Integrations**: Some integrations are hard to mock

### **3. Common Pitfalls**
- **Over-Testing**: Testing implementation details instead of behavior
- **Under-Testing**: Not covering edge cases
- **Test Maintenance**: Tests become brittle if they test implementation

## 💡 **TDD in Your CRM Project**

### **Current Approach:**
Your project uses **test-after development**:
1. Write the `CustomerService`
2. Write tests to verify it works
3. Use mocks to isolate dependencies

### **TDD Approach Would Be:**
1. Write test for `CustomerService.GetByIdAsync`
2. Implement minimal `GetByIdAsync` to pass test
3. Refactor `GetByIdAsync` to production quality
4. Repeat for next method

## 🚀 **Getting Started with TDD**

### **1. Start with Simple Methods**
```csharp
// Begin with pure functions (no dependencies)
public int Add(int a, int b) => a + b;
```

### **2. Practice the Cycle**
- Write failing test
- Make it pass
- Refactor
- Repeat

### **3. Use Your Existing Tools**
- **xUnit**: For test framework
- **Moq**: For mocking dependencies
- **FluentAssertions**: For readable assertions

## 📊 **TDD Metrics and Benefits**

### **Code Quality Metrics:**
- **Bug Density**: TDD typically reduces bugs by 40-80%
- **Code Coverage**: TDD naturally achieves high test coverage
- **Maintainability**: TDD code is more maintainable and refactorable

### **Development Metrics:**
- **Initial Development Time**: 15-35% longer initially
- **Debugging Time**: 50-90% reduction in debugging time
- **Regression Bugs**: Significant reduction in production bugs

## 🔄 **TDD Workflow Example**

### **Scenario: Adding Customer Search Feature**

#### **Iteration 1: Basic Search**
```csharp
// 1. RED: Write test
[Fact]
public void SearchCustomers_WithValidTerm_ShouldReturnMatchingCustomers()
{
    var customers = new List<Customer>
    {
        new Customer { Name = "John Doe" },
        new Customer { Name = "Jane Smith" }
    };
    
    var result = _customerService.SearchCustomers("John", customers);
    
    result.Should().HaveCount(1);
    result.First().Name.Should().Be("John Doe");
}

// 2. GREEN: Implement
public List<Customer> SearchCustomers(string term, List<Customer> customers)
{
    return customers.Where(c => c.Name.Contains(term)).ToList();
}

// 3. REFACTOR: Improve
public List<Customer> SearchCustomers(string term, List<Customer> customers)
{
    if (string.IsNullOrWhiteSpace(term))
        return customers;
        
    return customers.Where(c => 
        c.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
        .ToList();
}
```

#### **Iteration 2: Case-Insensitive Search**
```csharp
// 1. RED: Write test
[Fact]
public void SearchCustomers_WithCaseInsensitiveTerm_ShouldReturnMatchingCustomers()
{
    var customers = new List<Customer>
    {
        new Customer { Name = "John Doe" }
    };
    
    var result = _customerService.SearchCustomers("john", customers);
    
    result.Should().HaveCount(1);
}

// 2. GREEN: Already implemented in refactor step ✅
// 3. REFACTOR: No changes needed
```

#### **Iteration 3: Multiple Field Search**
```csharp
// 1. RED: Write test
[Fact]
public void SearchCustomers_WithEmailTerm_ShouldReturnMatchingCustomers()
{
    var customers = new List<Customer>
    {
        new Customer { Name = "John Doe", Email = "john@example.com" }
    };
    
    var result = _customerService.SearchCustomers("john@example.com", customers);
    
    result.Should().HaveCount(1);
}

// 2. GREEN: Implement
public List<Customer> SearchCustomers(string term, List<Customer> customers)
{
    if (string.IsNullOrWhiteSpace(term))
        return customers;
        
    return customers.Where(c => 
        c.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
        c.Email.Contains(term, StringComparison.OrdinalIgnoreCase))
        .ToList();
}

// 3. REFACTOR: Extract to method
public List<Customer> SearchCustomers(string term, List<Customer> customers)
{
    if (string.IsNullOrWhiteSpace(term))
        return customers;
        
    return customers.Where(c => ContainsTerm(c, term)).ToList();
}

private bool ContainsTerm(Customer customer, string term)
{
    return customer.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
           customer.Email.Contains(term, StringComparison.OrdinalIgnoreCase);
}
```

## 🎯 **TDD Anti-Patterns to Avoid**

### **1. Testing Implementation Details**
```csharp
// Bad: Testing how the method works internally
[Fact]
public void GetCustomer_ShouldCallRepositoryGetById()
{
    _service.GetCustomer(1);
    _mockRepository.Verify(r => r.GetById(1), Times.Once);
}

// Good: Testing what the method returns
[Fact]
public void GetCustomer_WhenCustomerExists_ShouldReturnCustomer()
{
    var result = _service.GetCustomer(1);
    result.Should().NotBeNull();
    result.Id.Should().Be(1);
}
```

### **2. Over-Mocking**
```csharp
// Bad: Mocking everything
var mockCustomer = new Mock<Customer>();
var mockOrder = new Mock<Order>();

// Good: Mock only external dependencies
var mockRepository = new Mock<ICustomerRepository>();
var customer = new Customer { Id = 1, Name = "John" };
```

### **3. Brittle Tests**
```csharp
// Bad: Test breaks when implementation changes
[Fact]
public void ProcessOrder_ShouldCallValidateThenSave()
{
    _service.ProcessOrder(order);
    _mockValidator.Verify(v => v.Validate(order), Times.Once);
    _mockRepository.Verify(r => r.Save(order), Times.Once);
}

// Good: Test focuses on outcome
[Fact]
public void ProcessOrder_WhenValidOrder_ShouldReturnSuccess()
{
    var result = _service.ProcessOrder(order);
    result.IsSuccess.Should().BeTrue();
}
```

## 🔧 **TDD Tools and Extensions**

### **Visual Studio Extensions:**
- **NCrunch**: Continuous test runner
- **Test Explorer**: Built-in test runner
- **ReSharper**: Enhanced test running and refactoring

### **Command Line Tools:**
```bash
# Run tests
dotnet test

# Run specific test
dotnet test --filter "GetByIdAsync"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### **CI/CD Integration:**
```yaml
# GitHub Actions example
- name: Run Tests
  run: dotnet test --verbosity normal

- name: Generate Coverage Report
  run: dotnet test --collect:"XPlat Code Coverage"
```

## 📈 **Measuring TDD Success**

### **Key Metrics:**
- **Test Coverage**: Percentage of code covered by tests
- **Bug Escape Rate**: Bugs found in production vs development
- **Refactoring Frequency**: How often you can safely refactor
- **Development Velocity**: Speed of feature delivery over time

### **Quality Indicators:**
- **Test Readability**: Can new developers understand tests?
- **Test Maintainability**: How often do tests need updates?
- **Test Reliability**: Do tests fail for the right reasons?

## 🎯 **TDD in Different Scenarios**

### **1. New Feature Development**
- Start with acceptance criteria
- Write tests for each requirement
- Implement minimal code to pass
- Refactor for production quality

### **2. Bug Fixes**
- Write test that reproduces the bug
- Fix the code to make test pass
- Ensure no regressions

### **3. Refactoring**
- Ensure comprehensive test coverage
- Refactor code while keeping tests green
- Use tests as safety net

## 💡 **Key Takeaways**

1. **TDD is a cycle**: Red → Green → Refactor → Repeat
2. **Tests come first**: Write tests before production code
3. **Start small**: Begin with simple, focused tests
4. **Focus on behavior**: Test what the code does, not how it does it
5. **Use your tools**: xUnit, Moq, and FluentAssertions support TDD well
6. **Practice makes perfect**: TDD requires time and practice to master
7. **Quality over speed**: Initial development may be slower, but overall quality improves

## 🚀 **Next Steps for Your Project**

### **1. Practice TDD on New Features**
- Start with simple utility methods
- Gradually apply to service methods
- Use existing test infrastructure

### **2. Improve Existing Tests**
- Add more edge case tests
- Improve test readability
- Ensure comprehensive coverage

### **3. TDD Training**
- Practice the Red-Green-Refactor cycle
- Learn to write better test names
- Master the art of minimal implementation

TDD is a powerful methodology that leads to better code quality, fewer bugs, and more maintainable software. Your Enterprise CRM project already has the tools and structure to practice TDD effectively! 🎉
