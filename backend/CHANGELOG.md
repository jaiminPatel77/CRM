# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Rate limiting with AspNetCoreRateLimit for auth endpoints
- Input sanitization middleware for XSS prevention
- Repository abstraction layer (IRepository<T>)
- Unit of Work pattern implementation
- MediatR notifications for cross-cutting concerns
- Pagination defaults and max page size enforcement
- Query result metadata (PagedResult with TotalPages, HasNextPage, etc.)
- OpenTelemetry distributed tracing
- Health check details (SMTP, internet, Hangfire)
- API response caching with OutputCache
- HATEOAS support (LinkedResource, Link classes)
- Response compression (gzip)
- EF Core query optimization with AsNoTracking()
- Connection pooling configuration

### Changed
- GenericBaseFeatures now returns PagedResult<T> with rich metadata
- List queries use AsNoTracking() for better performance

### Security
- Added rate limiting to prevent brute force attacks
- Added input sanitization to prevent XSS attacks

## [1.0.0] - Initial Release

### Features
- JWT Authentication
- User Management
- Role Management
- Project & Task Management
- Settings Management
- Audit Logging
- File Storage (Local, S3, Azure)
- Background Jobs with Hangfire
- SignalR Hubs
- Health Checks
- API Versioning
- OpenAPI/Scalar Documentation
