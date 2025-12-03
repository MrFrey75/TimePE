
# Dev-ApiMigration Branch Instructions

## Overview

This document outlines the steps and best practices for migrating the TimePE application to a dedicated API architecture. The goal is to create a robust, testable, and maintainable API layer for TimePE.Core, integrate comprehensive testing, and refactor the TimePE.WebApp to consume the new API.

---

## 1. Create New API Project

- **Project Naming:** Choose a clear, descriptive name for the new API project (e.g., `TimePE.Api`).
- **Location:** Place the new project under the `src/` directory for consistency.
- **Framework:** Use the latest supported .NET version (e.g., .NET 9) for long-term support and new features.
- **Initial Setup:**
	- Scaffold the project using `dotnet new webapi`.
	- Configure solution references to `TimePE.Core`.
	- Set up basic folder structure: `Controllers/`, `Models/`, `DTOs/`, `Services/`, `Middleware/`.

---

## 2. Develop API for TimePE.Core

- **API Design:**
	- Expose endpoints for all major entities (Projects, Users, Payments, TimeEntries, etc.).
	- Use RESTful conventions for routes and HTTP verbs.
	- Implement versioning (e.g., `/api/v1/`).
- **Data Transfer Objects (DTOs):**
	- Create DTOs for request/response models to decouple API from internal models.
- **Dependency Injection:**
	- Register services from `TimePE.Core` in the API’s DI container.
- **Error Handling:**
	- Implement global exception handling middleware.
	- Return standardized error responses.
- **Security:**
	- Add authentication and authorization (JWT recommended).
	- Secure sensitive endpoints.

---

## 3. Create Testing Project for API

- **Project Setup:**
	- Name the test project `TimePE.Api.Tests` and place it under `src/`.
	- Reference the API project and use xUnit or NUnit for testing.
- **Test Coverage:**
	- Write unit tests for controllers, services, and middleware.
	- Add integration tests for API endpoints using in-memory test server (e.g., `WebApplicationFactory`).
- **Mocking:**
	- Use mocking frameworks (e.g., Moq) for dependencies.
- **Continuous Integration:**
	- Integrate tests into CI pipeline for automated validation.

---

## 4. Implement Comprehensive Tests

- **Test Types:**
	- Unit tests for business logic and controllers.
	- Integration tests for end-to-end scenarios.
	- Security tests for authentication/authorization.
	- Performance tests for critical endpoints.
- **Best Practices:**
	- Aim for high code coverage, but prioritize meaningful tests.
	- Use Arrange-Act-Assert pattern for clarity.
	- Document test cases and expected outcomes.

---

## 5. Refactor TimePE.WebApp to Use API

- **API Integration:**
	- Replace direct calls to `TimePE.Core` with HTTP requests to the new API.
	- Use HttpClient or a typed client for API communication.
- **Error Handling:**
	- Handle API errors gracefully in the UI.
- **Authentication:**
	- Update authentication flows to use API tokens.
- **Testing:**
	- Test all UI features to ensure seamless integration with the API.
- **Deprecation:**
	- Remove obsolete code and direct references to `TimePE.Core` from the web app.

---

## Additional Recommendations

- **Documentation:** Update README and relevant docs to reflect the new architecture.
- **Migration Plan:** Create a phased rollout plan to minimize disruption.
- **Monitoring:** Set up logging and monitoring for the API.
- **Feedback:** Collect feedback from stakeholders and iterate as needed.

---

## References

- [.NET Web API Documentation](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
- [xUnit Testing Guide](https://xunit.net/docs/getting-started/netcore/cmdline)
- [Best Practices for RESTful APIs](https://restfulapi.net/)

---

This enhanced guide should help ensure a smooth and maintainable migration to an API-driven architecture for TimePE.