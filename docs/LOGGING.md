# TimePE Logging System

## Overview
TimePE uses **Serilog** as its logging framework, providing structured logging with multiple output sinks and configurable log levels.

## Log Outputs

### 1. Console Output
- **Format**: `[HH:mm:ss LEVEL] Message`
- **Purpose**: Real-time monitoring during development and debugging
- **Example**: `[10:08:51 INF] Starting TimePE application`

### 2. Daily Rolling Log Files
- **Location**: `logs/timepe-YYYYMMDD.log`
- **Rotation**: Daily (one file per day)
- **Retention**: 30 days
- **Size Limit**: 10 MB per file (rolls to new file when exceeded)
- **Format**: `YYYY-MM-DD HH:mm:ss.fff zzz [LEVEL] Message`
- **Purpose**: General application logging and audit trail

### 3. Error Log Files
- **Location**: `logs/errors/timepe-errors-YYYYMMDD.log`
- **Rotation**: Daily
- **Retention**: 90 days
- **Level**: Error and Fatal only
- **Purpose**: Quick access to application errors for troubleshooting

## Log Levels

### Production (appsettings.json)
- **Default**: Information
- **Microsoft**: Warning
- **System**: Warning

### Development (appsettings.Development.json)
- **Default**: Debug
- **Microsoft**: Information
- **System**: Information

## Log Enrichment

All log entries include:
- **Timestamp**: UTC with timezone offset
- **Application**: "TimePE"
- **MachineName**: Host machine identifier
- **ProcessId**: Process ID of the application

For HTTP requests:
- **RequestMethod**: GET, POST, etc.
- **RequestPath**: URL path
- **StatusCode**: HTTP response code
- **Elapsed**: Request processing time in milliseconds
- **UserName**: Authenticated user (if logged in)
- **UserAgent**: Client browser/application
- **RequestHost**: Request host header
- **RequestScheme**: http/https

## Log Examples

### Application Startup
```
2025-12-01 10:08:51.796 -05:00 [INF] Starting TimePE application
2025-12-01 10:08:51.796 -05:00 [DBG] Environment: Development
2025-12-01 10:08:51.796 -05:00 [INF] User account(s) already exist, skipping default user creation
2025-12-01 10:08:51.796 -05:00 [INF] Default 'General' project already exists
2025-12-01 10:08:51.796 -05:00 [INF] TimePE application configured successfully
```

### HTTP Requests
```
2025-12-01 10:08:53.549 -05:00 [INF] HTTP GET / responded 200 in 109.2662 ms
2025-12-01 10:08:56.421 -05:00 [INF] HTTP GET /Reports responded 200 in 42.4276 ms
```

### Authentication Events
```
2025-12-01 10:15:23.123 -05:00 [INF] User admin logged in successfully
2025-12-01 10:45:12.456 -05:00 [INF] User admin logged out
```

### Application Shutdown
```
2025-12-01 10:09:14.824 -05:00 [INF] Application is shutting down...
2025-12-01 10:09:14.836 -05:00 [INF] TimePE application stopped cleanly
2025-12-01 10:09:14.836 -05:00 [INF] Shutting down TimePE application
```

## Configuration

### Changing Log Levels
Edit `appsettings.json` or `appsettings.Development.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "TimePE": "Debug"
      }
    }
  }
}
```

### Adding Custom Loggers in Code

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void DoWork()
    {
        _logger.LogDebug("Starting work with parameter: {Param}", param);
        _logger.LogInformation("Work completed successfully");
        _logger.LogWarning("Potential issue detected: {Issue}", issue);
        _logger.LogError(ex, "An error occurred while processing: {Details}", details);
    }
}
```

## Maintenance

### Log Rotation
- Logs automatically rotate daily at midnight
- Old logs are retained according to retention policy
- Manual cleanup is not required

### Storage Considerations
- Each day generates approximately 1-5 MB of logs (depends on traffic)
- Error logs are typically much smaller (< 100 KB/day in normal operation)
- Monitor disk space if retention periods are extended

### Troubleshooting

**No logs appearing:**
1. Check `logs` directory exists (created automatically)
2. Verify permissions on logs directory
3. Check Serilog configuration in `appsettings.json`

**Logs too verbose:**
1. Increase minimum log level in configuration
2. Add overrides for specific namespaces

**Need more detailed logs:**
1. Change level to `Debug` or `Verbose`
2. Add custom enrichers for additional context

## Security Considerations

- Log files may contain sensitive information
- Ensure `logs/` directory is in `.gitignore`
- Do not log passwords, tokens, or sensitive user data
- Use structured logging with placeholders: `{Parameter}` instead of string concatenation
- Review log retention policies for compliance requirements

## Performance

Serilog is optimized for minimal performance impact:
- Asynchronous file writing
- Efficient structured logging
- Conditional logging based on level (debug logs skip when level is higher)

## Best Practices

1. **Use structured logging**: `_logger.LogInformation("User {Username} performed {Action}", username, action)`
2. **Choose appropriate levels**:
   - `Debug`: Detailed diagnostic information
   - `Information`: General application flow
   - `Warning`: Unexpected but handled situations
   - `Error`: Errors and exceptions
   - `Fatal`: Critical failures requiring immediate attention

3. **Include context**: Add relevant data to understand the log entry
4. **Be concise**: Log messages should be clear and actionable
5. **Avoid sensitive data**: Never log passwords, tokens, or PII
