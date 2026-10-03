# API Versioning Documentation

## Overview

This API uses URL path versioning with header fallback support.

## Versioning Strategy

- **Default Version**: v1
- **URL Path Format**: `/api/v1/{controller}`
- **Header Support**: `X-Api-Version: 1`

## Breaking Change Policy

### What Constitutes a Breaking Change

- Removing or renaming API endpoints
- Changing response body structure
- Changing parameter types or names
- Changing authentication requirements
- Removing or renaming response fields

### What Is NOT a Breaking Change

- Adding new optional parameters
- Adding new response fields
- Adding new endpoints
- Changing field order in responses

## Migration Path

1. New versions are introduced when breaking changes are required
2. Old versions remain available for at least 3 months after a new version release
3. Deprecation warnings are returned in response headers for deprecated versions
4. Clients should specify the API version in requests

## Version Lifecycle

| Version | Release Date | Status    | Sunset Date |
|---------|--------------|-----------|-------------|
| v1      | Current      | Active    | -           |

## Request Examples

```bash
# URL path versioning
GET /api/v1/users

# Header versioning  
GET /api/users
X-Api-Version: 1
```

## Response Headers

- `api-supported-versions`: List of supported versions
- `api-deprecated-versions`: List of deprecated versions
- `X-Api-Version`: The version that was used to process the request
