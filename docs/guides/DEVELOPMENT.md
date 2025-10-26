# Development Setup

## Database Configuration
- **Environment:** Development
- **Server:** localhost\SQLEXPRESS_Dev
- **Database:** EnterpriseCRM
- **Authentication:** Windows Authentication
- **Encryption:** Optional (dev only)
- **Trust Certificate:** Yes (dev only)

⚠️ **WARNING:** These settings are for development only!

## Connection String
```
Server=localhost\SQLEXPRESS_Dev;Database=EnterpriseCRM;Trusted_Connection=true;TrustServerCertificate=true;
```

## Environment Variables
- **Environment:** Development
- **IsDevelopment:** true

## Production Differences
- **Database:** EnterpriseCRM (not _DEV)
- **Encryption:** Mandatory
- **Trust Certificate:** No
- **Authentication:** SQL Authentication
- **Server:** Production server name
