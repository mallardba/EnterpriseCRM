# Enterprise CRM - Real-time Communication with SignalR

## 🔄 SignalR Overview

### **What is SignalR?**

SignalR is a real-time communication library for ASP.NET Core that enables server-to-client communication. It provides a simple API for creating real-time web applications, supporting WebSockets, Server-Sent Events, and Long Polling as fallback transports.

### **Why Use SignalR?**

**Real-time Communication:**
- Instant updates to connected clients
- Bidirectional communication
- Live data synchronization

**Transport Abstraction:**
- Automatic transport selection
- Fallback mechanisms
- Cross-platform compatibility

**Scalability:**
- Horizontal scaling support
- Connection management
- Performance optimization

**Integration:**
- Seamless ASP.NET Core integration
- Authentication support
- Dependency injection

### **SignalR Benefits**

- **Real-time Updates**: Instant data synchronization
- **Cross-platform**: Works across different platforms
- **Scalability**: Handles multiple connections
- **Reliability**: Automatic reconnection
- **Performance**: Optimized communication

### **When to Use SignalR**

**Good Use Cases:**
- Live notifications
- Real-time dashboards
- Collaborative features
- Live updates

**Avoid When:**
- Simple request-response scenarios
- One-time data fetching
- Over-engineering simple features
- Performance-critical applications

## 🎯 Core Concepts

### **1. Hub**
Central communication point for real-time messaging

```csharp
public class CustomerHub : Hub
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerHub> _logger;

    public CustomerHub(ICustomerService customerService, ILogger<CustomerHub> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    public async Task JoinCustomerGroup(int customerId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{customerId}");
        _logger.LogInformation("User {ConnectionId} joined customer group {CustomerId}", 
            Context.ConnectionId, customerId);
    }

    public async Task LeaveCustomerGroup(int customerId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Customer_{customerId}");
        _logger.LogInformation("User {ConnectionId} left customer group {CustomerId}", 
            Context.ConnectionId, customerId);
    }

    public async Task SendCustomerUpdate(int customerId, string message)
    {
        await Clients.Group($"Customer_{customerId}").SendAsync("CustomerUpdated", message);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
```

### **2. Hub Context Service**
Service for sending messages from outside the hub

```csharp
public interface IHubContextService
{
    Task SendToAllAsync(string method, object data);
    Task SendToGroupAsync(string groupName, string method, object data);
    Task SendToUserAsync(string userId, string method, object data);
    Task SendToConnectionAsync(string connectionId, string method, object data);
}

public class HubContextService : IHubContextService
{
    private readonly IHubContext<CustomerHub> _customerHub;
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly ILogger<HubContextService> _logger;

    public HubContextService(
        IHubContext<CustomerHub> customerHub,
        IHubContext<NotificationHub> notificationHub,
        ILogger<HubContextService> logger)
    {
        _customerHub = customerHub;
        _notificationHub = notificationHub;
        _logger = logger;
    }

    public async Task SendToAllAsync(string method, object data)
    {
        await _customerHub.Clients.All.SendAsync(method, data);
        _logger.LogInformation("Sent message to all clients: {Method}", method);
    }

    public async Task SendToGroupAsync(string groupName, string method, object data)
    {
        await _customerHub.Clients.Group(groupName).SendAsync(method, data);
        _logger.LogInformation("Sent message to group {GroupName}: {Method}", groupName, method);
    }

    public async Task SendToUserAsync(string userId, string method, object data)
    {
        await _customerHub.Clients.User(userId).SendAsync(method, data);
        _logger.LogInformation("Sent message to user {UserId}: {Method}", userId, method);
    }

    public async Task SendToConnectionAsync(string connectionId, string method, object data)
    {
        await _customerHub.Clients.Client(connectionId).SendAsync(method, data);
        _logger.LogInformation("Sent message to connection {ConnectionId}: {Method}", connectionId, method);
    }
}
```

## 🔧 Advanced SignalR Patterns

### **1. Typed Hubs**
Strongly-typed hub interfaces for better IntelliSense

```csharp
public interface ICustomerHubClient
{
    Task CustomerCreated(CustomerDto customer);
    Task CustomerUpdated(CustomerDto customer);
    Task CustomerDeleted(int customerId);
    Task CustomerStatusChanged(int customerId, CustomerStatus newStatus);
    Task NewTaskAssigned(TaskDto task);
    Task TaskCompleted(int taskId);
    Task OpportunityWon(int opportunityId, decimal amount);
    Task LeadConverted(int leadId, int customerId);
}

public class CustomerHub : Hub<ICustomerHubClient>
{
    private readonly ICustomerService _customerService;

    public CustomerHub(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task JoinCustomerGroup(int customerId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{customerId}");
    }

    public async Task LeaveCustomerGroup(int customerId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Customer_{customerId}");
    }

    public async Task JoinUserGroup(int userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
    }

    public async Task LeaveUserGroup(int userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
    }
}
```

### **2. Authentication Integration**
```csharp
public class AuthenticatedHub : Hub
{
    public string UserId => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public string UserName => Context.User?.FindFirst(ClaimTypes.Name)?.Value;
    public string UserRole => Context.User?.FindFirst(ClaimTypes.Role)?.Value;

    public override async Task OnConnectedAsync()
    {
        if (UserId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{UserId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{UserRole}");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        if (UserId != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{UserId}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Role_{UserRole}");
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
```

### **3. Real-time Notifications**
```csharp
public class NotificationHub : Hub<INotificationHubClient>
{
    private readonly IConnectionManager _connectionManager;

    public NotificationHub(IConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            await _connectionManager.AddConnectionAsync(userId, Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            await _connectionManager.RemoveConnectionAsync(userId, Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}

public interface INotificationHubClient
{
    Task NotificationReceived(NotificationDto notification);
    Task TaskReminder(TaskDto task);
    Task LeadFollowUp(LeadDto lead);
    Task OpportunityDeadline(OpportunityDto opportunity);
}
```

## 🎯 Real-time Features Implementation

### **1. Customer Updates**
```csharp
public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHubContextService _hubContextService;

    public CustomerService(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        IHubContextService hubContextService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _hubContextService = hubContextService;
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
    {
        var customer = _mapper.Map<Customer>(dto);
        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        var customerDto = _mapper.Map<CustomerDto>(customer);

        // Send real-time notification
        await _hubContextService.SendToAllAsync("CustomerCreated", customerDto);
        await _hubContextService.SendToGroupAsync("Managers", "NewCustomerNotification", 
            new { CustomerId = customer.Id, CompanyName = customer.CompanyName });

        return customerDto;
    }

    public async Task<CustomerDto> UpdateCustomerAsync(UpdateCustomerDto dto)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(dto.Id);
        if (customer == null)
            throw new NotFoundException($"Customer with ID {dto.Id} not found");

        _mapper.Map(dto, customer);
        customer.UpdatedAt = DateTime.UtcNow;
        
        await _unitOfWork.Customers.UpdateAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        var customerDto = _mapper.Map<CustomerDto>(customer);

        // Send real-time notification
        await _hubContextService.SendToGroupAsync($"Customer_{customer.Id}", "CustomerUpdated", customerDto);
        await _hubContextService.SendToUserAsync(customer.CreatedBy, "CustomerModified", 
            new { CustomerId = customer.Id, Action = "Updated" });

        return customerDto;
    }
}
```

### **2. Task Management**
```csharp
public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHubContextService _hubContextService;

    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto)
    {
        var task = _mapper.Map<Task>(dto);
        await _unitOfWork.Tasks.AddAsync(task);
        await _unitOfWork.SaveChangesAsync();

        var taskDto = _mapper.Map<TaskDto>(task);

        // Send real-time notification to assigned user
        await _hubContextService.SendToUserAsync(task.AssignedToUserId.ToString(), "NewTaskAssigned", taskDto);
        
        // Send notification to managers
        await _hubContextService.SendToGroupAsync("Managers", "TaskCreated", 
            new { TaskId = task.Id, Title = task.Title, AssignedTo = task.AssignedToUserId });

        return taskDto;
    }

    public async Task<TaskDto> CompleteTaskAsync(int taskId)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
        if (task == null)
            throw new NotFoundException($"Task with ID {taskId} not found");

        task.Status = TaskStatus.Completed;
        task.CompletedDate = DateTime.UtcNow;
        
        await _unitOfWork.Tasks.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        var taskDto = _mapper.Map<TaskDto>(task);

        // Send real-time notification
        await _hubContextService.SendToUserAsync(task.AssignedToUserId.ToString(), "TaskCompleted", taskDto);
        await _hubContextService.SendToGroupAsync("Managers", "TaskCompleted", 
            new { TaskId = task.Id, CompletedBy = task.AssignedToUserId });

        return taskDto;
    }
}
```

### **3. Lead and Opportunity Tracking**
```csharp
public class LeadService : ILeadService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHubContextService _hubContextService;

    public async Task<LeadDto> ConvertLeadToCustomerAsync(int leadId)
    {
        var lead = await _unitOfWork.Leads.GetByIdAsync(leadId);
        if (lead == null)
            throw new NotFoundException($"Lead with ID {leadId} not found");

        // Create customer from lead
        var customer = new Customer
        {
            CompanyName = lead.CompanyName,
            FirstName = lead.FirstName,
            LastName = lead.LastName,
            Email = lead.Email,
            Phone = lead.Phone,
            Industry = lead.Industry,
            Type = CustomerType.Company, // Default to company
            Status = CustomerStatus.Active
        };

        await _unitOfWork.Customers.AddAsync(customer);
        
        // Update lead status
        lead.Status = LeadStatus.ClosedWon;
        lead.CustomerId = customer.Id;
        
        await _unitOfWork.Leads.UpdateAsync(lead);
        await _unitOfWork.SaveChangesAsync();

        var leadDto = _mapper.Map<LeadDto>(lead);
        var customerDto = _mapper.Map<CustomerDto>(customer);

        // Send real-time notifications
        await _hubContextService.SendToUserAsync(lead.AssignedToUserId.ToString(), "LeadConverted", 
            new { LeadId = lead.Id, CustomerId = customer.Id });
        await _hubContextService.SendToGroupAsync("Sales", "LeadConverted", 
            new { LeadId = lead.Id, CustomerId = customer.Id, CompanyName = customer.CompanyName });

        return leadDto;
    }
}
```

## 🔄 Client-side Integration

### **1. JavaScript Client**
```javascript
// SignalR client setup
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/customerHub", {
        accessTokenFactory: () => {
            return localStorage.getItem("accessToken");
        }
    })
    .withAutomaticReconnect()
    .build();

// Connection events
connection.start().then(() => {
    console.log("Connected to SignalR hub");
    
    // Join customer group
    connection.invoke("JoinCustomerGroup", customerId);
}).catch(err => {
    console.error("SignalR connection error:", err);
});

// Reconnection handling
connection.onreconnecting(() => {
    console.log("Reconnecting to SignalR hub...");
});

connection.onreconnected(() => {
    console.log("Reconnected to SignalR hub");
    // Rejoin groups
    connection.invoke("JoinCustomerGroup", customerId);
});

// Hub method handlers
connection.on("CustomerCreated", (customer) => {
    console.log("New customer created:", customer);
    addCustomerToTable(customer);
    showNotification(`New customer: ${customer.companyName}`);
});

connection.on("CustomerUpdated", (customer) => {
    console.log("Customer updated:", customer);
    updateCustomerInTable(customer);
    showNotification(`Customer updated: ${customer.companyName}`);
});

connection.on("NewTaskAssigned", (task) => {
    console.log("New task assigned:", task);
    addTaskToDashboard(task);
    showNotification(`New task: ${task.title}`);
});

connection.on("TaskCompleted", (task) => {
    console.log("Task completed:", task);
    updateTaskStatus(task.id, "completed");
    showNotification(`Task completed: ${task.title}`);
});

// Blazor Server integration
window.signalRConnection = connection;
```

### **2. Blazor Server Integration**
```csharp
@page "/customers"
@inject IJSRuntime JSRuntime
@inject IHubContextService HubContextService

<div class="customers-container">
    <h2>Customers</h2>
    <div class="customers-list">
        @foreach (var customer in customers)
        {
            <CustomerCard Customer="customer" OnUpdate="HandleCustomerUpdate" />
        }
    </div>
</div>

@code {
    private List<CustomerDto> customers = new();
    private DotNetObjectReference<CustomersPage>? objRef;

    protected override async Task OnInitializedAsync()
    {
        customers = await CustomerService.GetCustomersAsync();
        objRef = DotNetObjectReference.Create(this);
        
        await JSRuntime.InvokeVoidAsync("setupSignalR", objRef);
    }

    [JSInvokable]
    public async Task HandleCustomerCreated(CustomerDto customer)
    {
        customers.Add(customer);
        StateHasChanged();
        
        // Show toast notification
        await ShowToastAsync($"New customer: {customer.CompanyName}");
    }

    [JSInvokable]
    public async Task HandleCustomerUpdated(CustomerDto customer)
    {
        var existingCustomer = customers.FirstOrDefault(c => c.Id == customer.Id);
        if (existingCustomer != null)
        {
            var index = customers.IndexOf(existingCustomer);
            customers[index] = customer;
            StateHasChanged();
            
            await ShowToastAsync($"Customer updated: {customer.CompanyName}");
        }
    }

    private async Task HandleCustomerUpdate(CustomerDto customer)
    {
        await CustomerService.UpdateCustomerAsync(customer);
        // Real-time update will be handled by SignalR
    }

    public void Dispose()
    {
        objRef?.Dispose();
    }
}
```

## 🧪 Testing SignalR

### **Unit Testing Hubs**
```csharp
public class CustomerHubTests
{
    private readonly Mock<ICustomerService> _customerServiceMock;
    private readonly Mock<IHubCallerClients<ICustomerHubClient>> _clientsMock;
    private readonly Mock<IGroupManager> _groupsMock;
    private readonly Mock<HubCallerContext> _contextMock;
    private readonly CustomerHub _hub;

    public CustomerHubTests()
    {
        _customerServiceMock = new Mock<ICustomerService>();
        _clientsMock = new Mock<IHubCallerClients<ICustomerHubClient>>();
        _groupsMock = new Mock<IGroupManager>();
        _contextMock = new Mock<HubCallerContext>();

        _hub = new CustomerHub(_customerServiceMock.Object)
        {
            Context = _contextMock.Object,
            Clients = _clientsMock.Object,
            Groups = _groupsMock.Object
        };
    }

    [Fact]
    public async Task JoinCustomerGroup_ShouldAddToGroup()
    {
        // Arrange
        var customerId = 1;
        var connectionId = "test-connection-id";
        _contextMock.Setup(c => c.ConnectionId).Returns(connectionId);

        // Act
        await _hub.JoinCustomerGroup(customerId);

        // Assert
        _groupsMock.Verify(g => g.AddToGroupAsync(connectionId, $"Customer_{customerId}", default), Times.Once);
    }

    [Fact]
    public async Task SendCustomerUpdate_ShouldSendToGroup()
    {
        // Arrange
        var customerId = 1;
        var message = "Customer updated";
        var groupClientsMock = new Mock<IClientProxy>();
        _clientsMock.Setup(c => c.Group($"Customer_{customerId}")).Returns(groupClientsMock.Object);

        // Act
        await _hub.SendCustomerUpdate(customerId, message);

        // Assert
        groupClientsMock.Verify(c => c.SendAsync("CustomerUpdated", message, default), Times.Once);
    }
}
```

### **Integration Testing**
```csharp
public class SignalRIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SignalRIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task SignalRHub_ShouldBeAccessible()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/customerHub");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.SwitchingProtocols);
    }
}
```

## 🚀 Performance Optimization

### **1. Connection Management**
```csharp
public class ConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();
    private readonly ILogger<ConnectionManager> _logger;

    public async Task AddConnectionAsync(string userId, string connectionId)
    {
        _userConnections.AddOrUpdate(userId, 
            new HashSet<string> { connectionId },
            (key, existing) =>
            {
                existing.Add(connectionId);
                return existing;
            });

        _logger.LogInformation("Added connection {ConnectionId} for user {UserId}", connectionId, userId);
    }

    public async Task RemoveConnectionAsync(string userId, string connectionId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            connections.Remove(connectionId);
            if (connections.Count == 0)
            {
                _userConnections.TryRemove(userId, out _);
            }
        }

        _logger.LogInformation("Removed connection {ConnectionId} for user {UserId}", connectionId, userId);
    }

    public IEnumerable<string> GetUserConnections(string userId)
    {
        return _userConnections.TryGetValue(userId, out var connections) 
            ? connections 
            : Enumerable.Empty<string>();
    }
}
```

### **2. Message Batching**
```csharp
public class BatchedNotificationService : IBatchedNotificationService
{
    private readonly IHubContextService _hubContextService;
    private readonly Timer _batchTimer;
    private readonly ConcurrentQueue<NotificationMessage> _messageQueue = new();

    public BatchedNotificationService(IHubContextService hubContextService)
    {
        _hubContextService = hubContextService;
        _batchTimer = new Timer(ProcessBatch, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public void QueueNotification(string userId, string method, object data)
    {
        _messageQueue.Enqueue(new NotificationMessage
        {
            UserId = userId,
            Method = method,
            Data = data,
            Timestamp = DateTime.UtcNow
        });
    }

    private async void ProcessBatch(object state)
    {
        var messages = new List<NotificationMessage>();
        
        while (_messageQueue.TryDequeue(out var message))
        {
            messages.Add(message);
        }

        if (messages.Any())
        {
            await ProcessMessagesAsync(messages);
        }
    }

    private async Task ProcessMessagesAsync(List<NotificationMessage> messages)
    {
        var groupedMessages = messages.GroupBy(m => m.UserId);
        
        foreach (var group in groupedMessages)
        {
            var batchData = group.Select(m => new { m.Method, m.Data, m.Timestamp });
            await _hubContextService.SendToUserAsync(group.Key, "BatchNotification", batchData);
        }
    }
}
```

## 📊 Best Practices

### **1. Use Groups for Targeted Messaging**
```csharp
// Good: Use groups for targeted messaging
await Clients.Group($"Customer_{customerId}").SendAsync("CustomerUpdated", customer);

// Avoid: Sending to all clients unnecessarily
await Clients.All.SendAsync("CustomerUpdated", customer);
```

### **2. Handle Connection Lifecycle**
```csharp
public override async Task OnConnectedAsync()
{
    var userId = GetUserId();
    if (userId != null)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
    }
    
    await base.OnConnectedAsync();
}

public override async Task OnDisconnectedAsync(Exception exception)
{
    var userId = GetUserId();
    if (userId != null)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
    }
    
    await base.OnDisconnectedAsync(exception);
}
```

### **3. Implement Reconnection Logic**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/customerHub")
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    .build();
```

### **4. Use Typed Hubs for Better IntelliSense**
```csharp
public interface ICustomerHubClient
{
    Task CustomerCreated(CustomerDto customer);
    Task CustomerUpdated(CustomerDto customer);
    Task CustomerDeleted(int customerId);
}

public class CustomerHub : Hub<ICustomerHubClient>
{
    // Strongly-typed hub implementation
}
```

SignalR provides powerful real-time communication capabilities for the Enterprise CRM system, enabling instant updates, notifications, and collaborative features that enhance user experience and productivity.
