# Enterprise CRM - Web API Documentation

## 🌐 API Overview

The Enterprise CRM Web API provides RESTful endpoints for managing customers, leads, opportunities, tasks, and users. Built with ASP.NET Core 8.0, it follows REST principles and provides comprehensive CRUD operations with advanced features like pagination, search, and filtering.

## 🔗 Base URL

```
Development: https://localhost:7001/api
Production: https://your-domain.com/api
```

## 🔐 Authentication

All API endpoints require authentication using JWT tokens:

```http
Authorization: Bearer <your-jwt-token>
```

## 📋 API Endpoints

### **Customers API** (`/api/customers`)

#### **GET /api/customers**
Get all customers with pagination

**Query Parameters:**
- `pageNumber` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Page size (default: 10)

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "companyName": "Acme Corporation",
      "firstName": "John",
      "lastName": "Smith",
      "email": "john.smith@acme.com",
      "phone": "555-0101",
      "address": "123 Main St",
      "city": "New York",
      "state": "NY",
      "postalCode": "10001",
      "country": "USA",
      "industry": "Technology",
      "type": 1,
      "status": 0,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": null,
      "createdBy": "admin",
      "updatedBy": null
    }
  ],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

#### **GET /api/customers/{id}**
Get customer by ID

**Path Parameters:**
- `id` (int): Customer ID

**Response:**
```json
{
  "id": 1,
  "companyName": "Acme Corporation",
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@acme.com",
  "phone": "555-0101",
  "address": "123 Main St",
  "city": "New York",
  "state": "NY",
  "postalCode": "10001",
  "country": "USA",
  "industry": "Technology",
  "type": 1,
  "status": 0,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null,
  "createdBy": "admin",
  "updatedBy": null
}
```

#### **GET /api/customers/search**
Search customers

**Query Parameters:**
- `searchTerm` (string, required): Search term
- `pageNumber` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Page size (default: 10)

**Response:** Same as GET /api/customers

#### **POST /api/customers**
Create a new customer

**Request Body:**
```json
{
  "companyName": "New Company",
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane.doe@newcompany.com",
  "phone": "555-0201",
  "address": "456 Oak Ave",
  "city": "San Francisco",
  "state": "CA",
  "postalCode": "94102",
  "country": "USA",
  "industry": "Software",
  "type": 1,
  "status": 0,
  "notes": "New customer from website"
}
```

**Response:** Created customer object (201 Created)

#### **PUT /api/customers/{id}**
Update an existing customer

**Path Parameters:**
- `id` (int): Customer ID

**Request Body:**
```json
{
  "id": 1,
  "companyName": "Updated Company",
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@updated.com",
  "phone": "555-0101",
  "address": "123 Main St",
  "city": "New York",
  "state": "NY",
  "postalCode": "10001",
  "country": "USA",
  "industry": "Technology",
  "type": 1,
  "status": 0,
  "notes": "Updated customer information"
}
```

**Response:** Updated customer object (200 OK)

#### **DELETE /api/customers/{id}**
Delete a customer (soft delete)

**Path Parameters:**
- `id` (int): Customer ID

**Response:** 204 No Content

#### **GET /api/customers/by-status/{status}**
Get customers by status

**Path Parameters:**
- `status` (int): Customer status (0=Active, 1=Inactive, 2=Suspended)

**Response:** Array of customer objects

#### **GET /api/customers/by-type/{type}**
Get customers by type

**Path Parameters:**
- `type` (int): Customer type (0=Individual, 1=Company)

**Response:** Array of customer objects

#### **GET /api/customers/recent**
Get recent customers

**Query Parameters:**
- `count` (int, optional): Number of recent customers (default: 10)

**Response:** Array of customer objects

### **Leads API** (`/api/leads`)

#### **GET /api/leads**
Get all leads with pagination

**Query Parameters:**
- `pageNumber` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Page size (default: 10)

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "companyName": "StartupXYZ",
      "firstName": "Alice",
      "lastName": "Brown",
      "email": "alice.brown@startupxyz.com",
      "phone": "555-0201",
      "jobTitle": "CEO",
      "industry": "Technology",
      "source": 0,
      "status": 0,
      "priority": 2,
      "estimatedValue": 50000.00,
      "expectedCloseDate": "2024-02-01T00:00:00Z",
      "assignedToUserId": 2,
      "customerId": null,
      "notes": "Interested in enterprise solution"
    }
  ],
  "totalCount": 50,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

#### **POST /api/leads**
Create a new lead

**Request Body:**
```json
{
  "companyName": "New Lead Company",
  "firstName": "Bob",
  "lastName": "Johnson",
  "email": "bob.johnson@newlead.com",
  "phone": "555-0301",
  "jobTitle": "CTO",
  "industry": "Software",
  "source": 1,
  "status": 0,
  "priority": 1,
  "estimatedValue": 75000.00,
  "expectedCloseDate": "2024-03-01T00:00:00Z",
  "assignedToUserId": 2,
  "notes": "Referred by existing customer"
}
```

### **Opportunities API** (`/api/opportunities`)

#### **GET /api/opportunities**
Get all opportunities with pagination

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "customerId": 1,
      "name": "Enterprise Software License",
      "stage": 2,
      "amount": 100000.00,
      "probability": 75.00,
      "expectedCloseDate": "2024-02-01T00:00:00Z",
      "actualCloseDate": null,
      "status": 0,
      "product": "CRM Software",
      "description": "Annual license for enterprise CRM solution",
      "assignedToUserId": 2,
      "notes": "High priority opportunity"
    }
  ],
  "totalCount": 25,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 3
}
```

#### **POST /api/opportunities**
Create a new opportunity

**Request Body:**
```json
{
  "customerId": 1,
  "name": "Custom Development Project",
  "stage": 3,
  "amount": 150000.00,
  "probability": 60.00,
  "expectedCloseDate": "2024-04-01T00:00:00Z",
  "product": "Custom Software",
  "description": "Custom web application development",
  "assignedToUserId": 3,
  "notes": "Complex project with multiple phases"
}
```

### **Tasks API** (`/api/tasks`)

#### **GET /api/tasks**
Get all tasks with pagination

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "title": "Follow up with Acme Corp",
      "description": "Call to discuss contract renewal",
      "type": 0,
      "priority": 2,
      "status": 0,
      "dueDate": "2024-01-15T00:00:00Z",
      "completedDate": null,
      "assignedToUserId": 2,
      "customerId": 1,
      "leadId": null,
      "opportunityId": null,
      "notes": "Important follow-up call"
    }
  ],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

#### **POST /api/tasks**
Create a new task

**Request Body:**
```json
{
  "title": "Prepare proposal for Tech Solutions",
  "description": "Create detailed proposal for software upgrade",
  "type": 5,
  "priority": 1,
  "status": 0,
  "dueDate": "2024-01-20T00:00:00Z",
  "assignedToUserId": 3,
  "customerId": 2,
  "leadId": null,
  "opportunityId": 1,
  "notes": "Include pricing and timeline"
}
```

### **Users API** (`/api/users`)

#### **GET /api/users**
Get all users

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "firstName": "Admin",
      "lastName": "User",
      "email": "admin@enterprisecrm.com",
      "username": "admin",
      "role": 0,
      "status": 0,
      "lastLoginDate": "2024-01-10T08:30:00Z",
      "phone": "555-0001",
      "jobTitle": "System Administrator",
      "department": "IT"
    }
  ],
  "totalCount": 10,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

## 📊 Data Transfer Objects (DTOs)

### **CustomerDto**
```json
{
  "id": 1,
  "companyName": "string",
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "phone": "string",
  "address": "string",
  "city": "string",
  "state": "string",
  "postalCode": "string",
  "country": "string",
  "industry": "string",
  "type": 0,
  "status": 0,
  "notes": "string",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z",
  "createdBy": "string",
  "updatedBy": "string"
}
```

### **CreateCustomerDto**
```json
{
  "companyName": "string",
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "phone": "string",
  "address": "string",
  "city": "string",
  "state": "string",
  "postalCode": "string",
  "country": "string",
  "industry": "string",
  "type": 0,
  "status": 0,
  "notes": "string"
}
```

### **UpdateCustomerDto**
```json
{
  "id": 1,
  "companyName": "string",
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "phone": "string",
  "address": "string",
  "city": "string",
  "state": "string",
  "postalCode": "string",
  "country": "string",
  "industry": "string",
  "type": 0,
  "status": 0,
  "notes": "string"
}
```

### **PagedResultDto<T>**
```json
{
  "data": [],
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 0
}
```

## 🔢 Status Codes

### **Success Responses**
- `200 OK` - Successful GET, PUT requests
- `201 Created` - Successful POST requests
- `204 No Content` - Successful DELETE requests

### **Error Responses**
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Missing or invalid authentication
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

## 📝 Error Response Format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "0HMQ8VJJA7U1G:00000001",
  "errors": {
    "Email": [
      "The Email field is required."
    ],
    "CompanyName": [
      "The CompanyName field is required."
    ]
  }
}
```

## 🔍 Search and Filtering

### **Customer Search**
Search customers by:
- Company name
- First name
- Last name
- Email address
- Phone number
- Industry

### **Lead Search**
Search leads by:
- Company name
- Contact name
- Email address
- Industry
- Source
- Status

### **Task Search**
Search tasks by:
- Title
- Description
- Assigned user
- Customer
- Lead
- Opportunity

## 📈 Pagination

All list endpoints support pagination:

**Query Parameters:**
- `pageNumber` (int): Page number (1-based)
- `pageSize` (int): Items per page (max 100)

**Response Headers:**
- `X-Total-Count`: Total number of items
- `X-Page-Number`: Current page number
- `X-Page-Size`: Items per page
- `X-Total-Pages`: Total number of pages

## 🔒 Authorization

### **Role-Based Access Control**
- **Admin**: Full access to all endpoints
- **Manager**: Access to assigned records and team records
- **User**: Access to own records only
- **ReadOnly**: Read-only access to assigned records

### **Resource-Level Permissions**
- Users can only access their assigned leads, opportunities, and tasks
- Managers can access their team's records
- Admins have full access to all records

## 🚀 Performance Features

### **Caching**
- Response caching for frequently accessed data
- ETags for conditional requests
- Cache invalidation on updates

### **Compression**
- Gzip compression for responses
- Brotli compression for modern clients

### **Rate Limiting**
- 1000 requests per hour per user
- 100 requests per minute per IP
- Burst allowance for legitimate usage

## 📚 API Documentation

### **Swagger/OpenAPI**
Interactive API documentation available at:
```
https://localhost:7001/swagger
```

### **Postman Collection**
Import the Postman collection for easy API testing:
```
https://your-domain.com/api/postman-collection.json
```

## 🧪 Testing the API

### **Using cURL**
```bash
# Get all customers
curl -H "Authorization: Bearer <token>" \
     https://localhost:7001/api/customers

# Create a new customer
curl -X POST \
     -H "Authorization: Bearer <token>" \
     -H "Content-Type: application/json" \
     -d '{"companyName":"Test Company","email":"test@test.com"}' \
     https://localhost:7001/api/customers
```

### **Using Postman**
1. Import the Postman collection
2. Set the base URL to your API endpoint
3. Add your JWT token to the Authorization header
4. Test the endpoints

This API provides a comprehensive interface for managing all aspects of the Enterprise CRM system with proper authentication, authorization, and error handling.
