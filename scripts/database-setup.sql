-- Enterprise CRM Database Scripts
-- This file contains SQL Server scripts for database setup and stored procedures

-- Create Database (run this first)
-- CREATE DATABASE EnterpriseCRM;
-- GO

-- Use Database
-- USE EnterpriseCRM;
-- GO

-- Create stored procedure for customer metrics
CREATE OR ALTER PROCEDURE sp_GetCustomerMetrics
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        COUNT(*) as TotalCustomers,
        COUNT(CASE WHEN Status = 0 THEN 1 END) as ActiveCustomers,
        COUNT(CASE WHEN Status = 1 THEN 1 END) as InactiveCustomers,
        COUNT(CASE WHEN Status = 2 THEN 1 END) as SuspendedCustomers,
        COUNT(CASE WHEN Type = 0 THEN 1 END) as IndividualCustomers,
        COUNT(CASE WHEN Type = 1 THEN 1 END) as CompanyCustomers,
        COUNT(CASE WHEN CreatedAt >= DATEADD(month, -1, GETDATE()) THEN 1 END) as NewCustomersThisMonth
    FROM Customers
    WHERE IsDeleted = 0;
END
GO

-- Create stored procedure for sales forecast
CREATE OR ALTER PROCEDURE sp_CalculateSalesForecast
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        SUM(Amount) as TotalPipelineValue,
        SUM(Amount * Probability / 100) as ForecastedRevenue,
        COUNT(*) as TotalOpportunities,
        COUNT(CASE WHEN Stage = 0 THEN 1 END) as ProspectingCount,
        COUNT(CASE WHEN Stage = 1 THEN 1 END) as QualificationCount,
        COUNT(CASE WHEN Stage = 2 THEN 1 END) as ProposalCount,
        COUNT(CASE WHEN Stage = 3 THEN 1 END) as NegotiationCount,
        COUNT(CASE WHEN Stage = 4 THEN 1 END) as ClosedWonCount,
        COUNT(CASE WHEN Stage = 5 THEN 1 END) as ClosedLostCount
    FROM Opportunities
    WHERE IsDeleted = 0 AND Status = 0;
END
GO

-- Create stored procedure for monthly report
CREATE OR ALTER PROCEDURE sp_GenerateMonthlyReport
    @Year INT,
    @Month INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @StartDate DATETIME = DATEFROMPARTS(@Year, @Month, 1);
    DECLARE @EndDate DATETIME = EOMONTH(@StartDate);
    
    -- Customer metrics
    SELECT 
        'CustomerMetrics' as ReportType,
        COUNT(*) as TotalCustomers,
        COUNT(CASE WHEN Status = 0 THEN 1 END) as ActiveCustomers,
        COUNT(CASE WHEN CreatedAt >= @StartDate AND CreatedAt <= @EndDate THEN 1 END) as NewCustomers
    FROM Customers
    WHERE IsDeleted = 0
    
    UNION ALL
    
    -- Lead metrics
    SELECT 
        'LeadMetrics' as ReportType,
        COUNT(*) as TotalLeads,
        COUNT(CASE WHEN Status = 0 THEN 1 END) as NewLeads,
        COUNT(CASE WHEN Status = 6 THEN 1 END) as ClosedWonLeads
    FROM Leads
    WHERE IsDeleted = 0 AND CreatedAt >= @StartDate AND CreatedAt <= @EndDate
    
    UNION ALL
    
    -- Opportunity metrics
    SELECT 
        'OpportunityMetrics' as ReportType,
        COUNT(*) as TotalOpportunities,
        SUM(Amount) as TotalValue,
        SUM(Amount * Probability / 100) as ForecastedValue
    FROM Opportunities
    WHERE IsDeleted = 0 AND CreatedAt >= @StartDate AND CreatedAt <= @EndDate;
END
GO

-- Create stored procedure for task analytics
CREATE OR ALTER PROCEDURE sp_GetTaskAnalytics
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        COUNT(*) as TotalTasks,
        COUNT(CASE WHEN Status = 0 THEN 1 END) as PendingTasks,
        COUNT(CASE WHEN Status = 1 THEN 1 END) as InProgressTasks,
        COUNT(CASE WHEN Status = 2 THEN 1 END) as CompletedTasks,
        COUNT(CASE WHEN Status = 3 THEN 1 END) as CancelledTasks,
        COUNT(CASE WHEN Status = 4 THEN 1 END) as OnHoldTasks,
        COUNT(CASE WHEN DueDate < GETDATE() AND Status != 2 THEN 1 END) as OverdueTasks,
        COUNT(CASE WHEN DueDate = CAST(GETDATE() AS DATE) AND Status != 2 THEN 1 END) as DueTodayTasks
    FROM Tasks
    WHERE IsDeleted = 0;
END
GO

-- Create stored procedure for user performance
CREATE OR ALTER PROCEDURE sp_GetUserPerformance
    @UserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.Id,
        u.FirstName + ' ' + u.LastName as FullName,
        u.Email,
        u.JobTitle,
        u.Department,
        COUNT(DISTINCT t.Id) as TotalTasks,
        COUNT(DISTINCT CASE WHEN t.Status = 2 THEN t.Id END) as CompletedTasks,
        COUNT(DISTINCT CASE WHEN t.DueDate < GETDATE() AND t.Status != 2 THEN t.Id END) as OverdueTasks,
        COUNT(DISTINCT l.Id) as AssignedLeads,
        COUNT(DISTINCT o.Id) as AssignedOpportunities,
        ISNULL(SUM(o.Amount), 0) as TotalOpportunityValue
    FROM Users u
    LEFT JOIN Tasks t ON u.Id = t.AssignedToUserId AND t.IsDeleted = 0
    LEFT JOIN Leads l ON u.Id = l.AssignedToUserId AND l.IsDeleted = 0
    LEFT JOIN Opportunities o ON u.Id = o.AssignedToUserId AND o.IsDeleted = 0
    WHERE u.IsDeleted = 0
    AND (@UserId IS NULL OR u.Id = @UserId)
    GROUP BY u.Id, u.FirstName, u.LastName, u.Email, u.JobTitle, u.Department
    ORDER BY TotalTasks DESC;
END
GO

-- Create indexes for better performance
CREATE NONCLUSTERED INDEX IX_Customers_Email ON Customers(Email);
CREATE NONCLUSTERED INDEX IX_Customers_Status ON Customers(Status);
CREATE NONCLUSTERED INDEX IX_Customers_Type ON Customers(Type);
CREATE NONCLUSTERED INDEX IX_Customers_CreatedAt ON Customers(CreatedAt);

CREATE NONCLUSTERED INDEX IX_Contacts_CustomerId ON Contacts(CustomerId);
CREATE NONCLUSTERED INDEX IX_Contacts_Email ON Contacts(Email);
CREATE NONCLUSTERED INDEX IX_Contacts_Role ON Contacts(Role);

CREATE NONCLUSTERED INDEX IX_Leads_Status ON Leads(Status);
CREATE NONCLUSTERED INDEX IX_Leads_Source ON Leads(Source);
CREATE NONCLUSTERED INDEX IX_Leads_AssignedToUserId ON Leads(AssignedToUserId);
CREATE NONCLUSTERED INDEX IX_Leads_CreatedAt ON Leads(CreatedAt);

CREATE NONCLUSTERED INDEX IX_Opportunities_CustomerId ON Opportunities(CustomerId);
CREATE NONCLUSTERED INDEX IX_Opportunities_Stage ON Opportunities(Stage);
CREATE NONCLUSTERED INDEX IX_Opportunities_Status ON Opportunities(Status);
CREATE NONCLUSTERED INDEX IX_Opportunities_AssignedToUserId ON Opportunities(AssignedToUserId);

CREATE NONCLUSTERED INDEX IX_Tasks_AssignedToUserId ON Tasks(AssignedToUserId);
CREATE NONCLUSTERED INDEX IX_Tasks_Status ON Tasks(Status);
CREATE NONCLUSTERED INDEX IX_Tasks_DueDate ON Tasks(DueDate);
CREATE NONCLUSTERED INDEX IX_Tasks_CustomerId ON Tasks(CustomerId);
CREATE NONCLUSTERED INDEX IX_Tasks_LeadId ON Tasks(LeadId);
CREATE NONCLUSTERED INDEX IX_Tasks_OpportunityId ON Tasks(OpportunityId);

CREATE NONCLUSTERED INDEX IX_Users_Email ON Users(Email);
CREATE NONCLUSTERED INDEX IX_Users_Username ON Users(Username);
CREATE NONCLUSTERED INDEX IX_Users_Role ON Users(Role);
CREATE NONCLUSTERED INDEX IX_Users_Status ON Users(Status);

-- Insert sample data for testing
INSERT INTO Users (FirstName, LastName, Email, Username, PasswordHash, Role, Status, CreatedBy, CreatedAt)
VALUES 
('Admin', 'User', 'admin@enterprisecrm.com', 'admin', 'admin123', 0, 0, 'System', GETDATE()),
('John', 'Doe', 'john.doe@enterprisecrm.com', 'john.doe', 'password123', 1, 0, 'System', GETDATE()),
('Jane', 'Smith', 'jane.smith@enterprisecrm.com', 'jane.smith', 'password123', 2, 0, 'System', GETDATE());

INSERT INTO Customers (CompanyName, FirstName, LastName, Email, Phone, Address, City, State, PostalCode, Country, Industry, Type, Status, CreatedBy, CreatedAt)
VALUES 
('Acme Corporation', 'John', 'Smith', 'john.smith@acme.com', '555-0101', '123 Main St', 'New York', 'NY', '10001', 'USA', 'Technology', 1, 0, 'admin', GETDATE()),
('Tech Solutions Inc', 'Sarah', 'Johnson', 'sarah.johnson@techsolutions.com', '555-0102', '456 Oak Ave', 'San Francisco', 'CA', '94102', 'USA', 'Software', 1, 0, 'admin', GETDATE()),
('Global Industries', 'Mike', 'Wilson', 'mike.wilson@global.com', '555-0103', '789 Pine St', 'Chicago', 'IL', '60601', 'USA', 'Manufacturing', 1, 0, 'admin', GETDATE());

INSERT INTO Leads (CompanyName, FirstName, LastName, Email, Phone, JobTitle, Industry, Source, Status, Priority, EstimatedValue, ExpectedCloseDate, AssignedToUserId, CreatedBy, CreatedAt)
VALUES 
('StartupXYZ', 'Alice', 'Brown', 'alice.brown@startupxyz.com', '555-0201', 'CEO', 'Technology', 0, 0, 2, 50000.00, DATEADD(month, 1, GETDATE()), 2, 'admin', GETDATE()),
('Innovation Labs', 'Bob', 'Davis', 'bob.davis@innovationlabs.com', '555-0202', 'CTO', 'Software', 1, 1, 1, 75000.00, DATEADD(month, 2, GETDATE()), 3, 'admin', GETDATE());

INSERT INTO Opportunities (CustomerId, Name, Stage, Amount, Probability, ExpectedCloseDate, Status, Product, Description, AssignedToUserId, CreatedBy, CreatedAt)
VALUES 
(1, 'Enterprise Software License', 2, 100000.00, 75, DATEADD(month, 1, GETDATE()), 0, 'CRM Software', 'Annual license for enterprise CRM solution', 2, 'admin', GETDATE()),
(2, 'Custom Development Project', 3, 150000.00, 60, DATEADD(month, 2, GETDATE()), 0, 'Custom Software', 'Custom web application development', 3, 'admin', GETDATE());

INSERT INTO Tasks (Title, Description, Type, Priority, Status, DueDate, AssignedToUserId, CustomerId, CreatedBy, CreatedAt)
VALUES 
('Follow up with Acme Corp', 'Call to discuss contract renewal', 0, 2, 0, DATEADD(day, 3, GETDATE()), 2, 1, 'admin', GETDATE()),
('Prepare proposal for Tech Solutions', 'Create detailed proposal for software upgrade', 5, 1, 1, DATEADD(day, 5, GETDATE()), 3, 2, 'admin', GETDATE()),
('Demo presentation for Global Industries', 'Prepare and deliver product demo', 6, 1, 0, DATEADD(day, 7, GETDATE()), 2, 3, 'admin', GETDATE());
