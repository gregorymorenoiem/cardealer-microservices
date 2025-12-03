# API Versioning Guide - ApiDocsService

## 📋 Overview

The **Version Management System** provides comprehensive API versioning capabilities including version tracking, comparison, deprecation management, and migration paths.

## 🏗️ Architecture

### Components

1. **VersionService**: Core service for version management
2. **VersionController**: REST API endpoints for version operations
3. **ApiVersion Models**: Data structures for version information

### Version Information

```csharp
public class ApiVersion
{
    public string Version { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsDeprecated { get; set; }
    public DateTime? DeprecationDate { get; set; }
    public DateTime? SunsetDate { get; set; }
    public string? MigrationGuideUrl { get; set; }
    public List<string> BreakingChanges { get; set; }
}
```

## 🚀 API Endpoints

### Get Service Versions

```http
GET /api/version/services/{serviceName}
```

**Response:**
```json
{
  "serviceName": "AuthService",
  "displayName": "Authentication Service",
  "versions": [
    {
      "version": "v1",
      "title": "Authentication Service v1",
      "description": "Initial release",
      "isDeprecated": false
    },
    {
      "version": "v2",
      "title": "Authentication Service v2",
      "description": "OAuth 2.0 support",
      "isDeprecated": false
    }
  ],
  "currentVersion": "v2",
  "latestVersion": "v2"
}
```

### Get All Versioned Services

```http
GET /api/version/services
```

Returns version information for all registered services.

### Compare Versions

```http
GET /api/version/compare/{serviceName}?fromVersion=v1&toVersion=v2
```

**Response:**
```json
{
  "serviceName": "AuthService",
  "fromVersion": "v1",
  "toVersion": "v2",
  "addedEndpoints": [
    "GET /api/auth/oauth/authorize",
    "POST /api/auth/oauth/token"
  ],
  "removedEndpoints": [
    "POST /api/auth/legacy-login"
  ],
  "modifiedEndpoints": [
    "POST /api/auth/login"
  ],
  "changes": [
    {
      "path": "/api/auth/oauth/authorize",
      "method": "GET",
      "type": "Added",
      "description": "New endpoint added in v2",
      "isBreaking": false
    },
    {
      "path": "/api/auth/legacy-login",
      "method": "POST",
      "type": "Removed",
      "description": "Endpoint removed in v2",
      "isBreaking": true
    }
  ]
}
```

### Get Deprecated APIs

```http
GET /api/version/deprecated
```

Returns all deprecated API versions across all services.

**Response:**
```json
[
  {
    "version": "v1",
    "title": "Auth Service v1",
    "description": "Legacy authentication",
    "isDeprecated": true,
    "deprecationDate": "2024-01-01T00:00:00Z",
    "sunsetDate": "2025-01-01T00:00:00Z",
    "migrationGuideUrl": "https://docs.cardealer.com/migration/auth-v1-to-v2",
    "breakingChanges": [
      "JWT token format changed",
      "Password policy updated"
    ]
  }
]
```

### Check Version Deprecation

```http
GET /api/version/deprecated/{serviceName}/{version}
```

**Response:**
```json
{
  "serviceName": "AuthService",
  "version": "v1",
  "isDeprecated": true
}
```

### Get Migration Path

```http
GET /api/version/migration/{serviceName}?fromVersion=v1&toVersion=v3
```

Returns the recommended migration path between versions.

**Response:**
```json
{
  "serviceName": "AuthService",
  "fromVersion": "v1",
  "toVersion": "v3",
  "migrationPath": ["v1", "v2", "v3"]
}
```

## 💡 Usage Examples

### Check if Service Version is Deprecated

```bash
curl -X GET "http://localhost:5320/api/version/deprecated/AuthService/v1"
```

### Compare Two Versions

```bash
curl -X GET "http://localhost:5320/api/version/compare/AuthService?fromVersion=v1&toVersion=v2"
```

### Get All Service Versions

```bash
curl -X GET "http://localhost:5320/api/version/services/AuthService"
```

## 🔄 Version Management Best Practices

### 1. Semantic Versioning

- **Major (v1, v2, v3)**: Breaking changes
- **Minor (v1.1, v1.2)**: New features, backward compatible
- **Patch (v1.1.1)**: Bug fixes

### 2. Deprecation Process

1. **Announcement**: Communicate deprecation 6+ months in advance
2. **Documentation**: Update API docs with deprecation notices
3. **Migration Guide**: Provide clear migration instructions
4. **Monitoring**: Track usage of deprecated endpoints
5. **Sunset**: Remove deprecated version after sunset date

### 3. Breaking Changes

Always document breaking changes:

```json
{
  "breakingChanges": [
    "Changed authentication header format from 'X-Auth-Token' to 'Authorization: Bearer'",
    "Removed deprecated endpoint /api/legacy/users",
    "Updated response format for /api/users - now returns paginated results"
  ]
}
```

### 4. Migration Paths

Provide clear migration paths:

```
v1 → v2: Update authentication to OAuth 2.0
v2 → v3: Migrate from REST to GraphQL endpoints
```

## 📊 Version Comparison

The comparison feature identifies:

- ✅ **Added Endpoints**: New APIs in the target version
- ❌ **Removed Endpoints**: APIs removed (breaking changes)
- 🔄 **Modified Endpoints**: APIs with changed behavior
- 📝 **Change Details**: Description of each change

## 🎯 Integration Example

```csharp
// In your client application
public async Task<bool> CheckApiCompatibilityAsync(string serviceName, string version)
{
    var isDeprecated = await _versionClient.IsVersionDeprecatedAsync(serviceName, version);
    
    if (isDeprecated)
    {
        var latestVersion = await _versionClient.GetServiceVersionsAsync(serviceName);
        var comparison = await _versionClient.CompareVersionsAsync(
            serviceName, 
            version, 
            latestVersion.LatestVersion);
            
        // Check for breaking changes
        if (comparison.Changes.Any(c => c.IsBreaking))
        {
            // Handle breaking changes
            Console.WriteLine($"⚠️ Breaking changes detected! Migration required.");
            return false;
        }
    }
    
    return true;
}
```

## 🔔 Notifications

Set up alerts for:

- New version releases
- Deprecation announcements
- Breaking changes
- Sunset dates approaching

## 📚 Additional Resources

- [API Versioning Strategy](https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design#versioning-a-restful-web-api)
- [Semantic Versioning](https://semver.org/)
- [API Deprecation Best Practices](https://swagger.io/specification/#operation-object)
