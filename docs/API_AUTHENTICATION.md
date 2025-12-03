# TimePE API Authentication & Security

## Overview
The TimePE API uses JWT (JSON Web Token) Bearer authentication to secure endpoints. All endpoints except `/api/v1/auth/login` and `/api/v1/auth/register` require authentication.

## Authentication Flow

### 1. User Registration
**Endpoint:** `POST /api/v1/auth/register`

**Request:**
```json
{
  "username": "john.doe",
  "password": "SecurePassword123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-12-02T18:30:00Z",
  "user": {
    "id": 1,
    "username": "john.doe",
    "isActive": true,
    "createdAt": "2025-12-02T17:30:00Z"
  }
}
```

### 2. User Login
**Endpoint:** `POST /api/v1/auth/login`

**Request:**
```json
{
  "username": "john.doe",
  "password": "SecurePassword123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-12-02T18:30:00Z",
  "user": {
    "id": 1,
    "username": "john.doe",
    "isActive": true,
    "lastLoginAt": "2025-12-02T17:30:00Z",
    "createdAt": "2025-12-02T16:00:00Z"
  }
}
```

### 3. Using the Token
Include the JWT token in the `Authorization` header for all authenticated requests:

```http
GET /api/v1/projects
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Configuration

### appsettings.json
```json
{
  "Jwt": {
    "Key": "your-super-secret-key-min-32-characters-long-for-production",
    "Issuer": "TimePE.Api",
    "Audience": "TimePE.WebApp",
    "ExpirationMinutes": 60
  }
}
```

**Important:** Change the `Key` value in production to a secure random string of at least 32 characters.

### Environment-Specific Configuration
- **Development:** Token expires in 120 minutes (`appsettings.Development.json`)
- **Production:** Token expires in 60 minutes (`appsettings.json`)

## Secured Endpoints

All the following endpoints require JWT authentication:

### Projects
- `GET /api/v1/projects` - List all projects
- `GET /api/v1/projects/{id}` - Get project by ID
- `POST /api/v1/projects` - Create new project
- `PUT /api/v1/projects/{id}` - Update project
- `DELETE /api/v1/projects/{id}` - Delete project

### Users
- `GET /api/v1/users` - List all users
- `GET /api/v1/users/{id}` - Get user by ID
- `POST /api/v1/users` - Create new user
- `PUT /api/v1/users/{id}` - Update user
- `DELETE /api/v1/users/{id}` - Delete user

### Time Entries
- `GET /api/v1/timeentries` - List all time entries
- `GET /api/v1/timeentries/{id}` - Get time entry by ID
- `POST /api/v1/timeentries` - Create new time entry
- `PUT /api/v1/timeentries/{id}` - Update time entry
- `DELETE /api/v1/timeentries/{id}` - Delete time entry

### Payments
- `GET /api/v1/payments` - List all payments
- `GET /api/v1/payments/{id}` - Get payment by ID
- `POST /api/v1/payments` - Create new payment
- `PUT /api/v1/payments/{id}` - Update payment
- `DELETE /api/v1/payments/{id}` - Delete payment

### Pay Rates
- `GET /api/v1/payrates` - List all pay rates
- `GET /api/v1/payrates/{id}` - Get pay rate by ID
- `POST /api/v1/payrates` - Create new pay rate
- `PUT /api/v1/payrates/{id}` - Update pay rate
- `DELETE /api/v1/payrates/{id}` - Delete pay rate

### Incidentals
- `GET /api/v1/incidentals` - List all incidentals
- `GET /api/v1/incidentals/{id}` - Get incidental by ID
- `POST /api/v1/incidentals` - Create new incidental
- `PUT /api/v1/incidentals/{id}` - Update incidental
- `DELETE /api/v1/incidentals/{id}` - Delete incidental

## Error Responses

### 401 Unauthorized
Returned when no token is provided or the token is invalid/expired.

```json
{
  "message": "Unauthorized",
  "statusCode": 401,
  "timestamp": "2025-12-02T17:30:00Z",
  "traceId": "00-abc123..."
}
```

### 403 Forbidden
Returned when the token is valid but the user doesn't have permission for the resource.

```json
{
  "message": "Forbidden",
  "statusCode": 403,
  "timestamp": "2025-12-02T17:30:00Z",
  "traceId": "00-abc123..."
}
```

## Security Best Practices

1. **Always use HTTPS** in production
2. **Rotate JWT secrets** regularly
3. **Keep tokens short-lived** (60-120 minutes)
4. **Store tokens securely** on the client (httpOnly cookies or secure storage)
5. **Never log tokens** or include them in error messages
6. **Validate all input** on both client and server
7. **Use strong passwords** (enforce password policies)
8. **Implement rate limiting** for authentication endpoints

## Testing with cURL

### Register a new user:
```bash
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"TestPass123"}'
```

### Login:
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"TestPass123"}'
```

### Access protected endpoint:
```bash
curl -X GET http://localhost:5000/api/v1/projects \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

## Token Claims

The JWT token contains the following claims:
- `nameid` - User ID (integer)
- `unique_name` - Username (string)
- `jti` - Token unique identifier (GUID)
- `exp` - Expiration timestamp
- `iss` - Issuer (TimePE.Api)
- `aud` - Audience (TimePE.WebApp)
