# Code Standards

This document captures the coding standards and conventions used across the TimePE project. It is intentionally concise and pragmatic so contributors can easily follow a consistent style.

Goals
- Make code easy to read and maintain
- Keep a consistent project structure so it's easy to locate types, services, and tests
- Reduce friction in code reviews by documenting common preferences

Table of contents
- General principles
- Repository layout and filenames
- C# style and conventions (examples)
- Interfaces and services guidance
- Tests and naming
- Commit and PR guidance
- Examples
- Quick reference checklist

General principles
- Prefer clarity over cleverness. Simple, readable code wins.
- Small files and single responsibility: one primary responsibility per class/file.
- Keep public APIs stable — prefer additive changes.
- Use meaningful names for types, methods, properties, and parameters.

Repository layout and filenames
- Use PascalCase for file names that contain a single public type (e.g., `OrderProcessor.cs`).
- For small, closely related types (for example an interface and a single concrete service that implements it), co-locate them in the same file only when it improves discoverability and the file remains small. See "Interfaces and services guidance" below.
- Keep folders focused by responsibility: e.g., `Services/`, `Models/`, `Tests/`, `Controllers/`.

C# style and conventions
- Use PascalCase for public types and members; camelCase for private fields (prefix private fields with `_` for clarity, e.g. `_logger`).
- Use expression-bodied members for trivial getters or methods when it improves readability.
- Prefer immutable DTOs where practical. Mark properties as init-only (`{ get; init; }`) when the object is only populated at creation.
- Use async/await for asynchronous operations. Name asynchronous methods with the `Async` suffix.
- Avoid dynamic and weakly typed constructs unless strictly necessary.
- All class parameters should have either defaults, or are required.

Interfaces and services guidance
- The guideline in the repository: Place interfaces and associated service files in the same file when it improves readability and the file is small. Use this sparingly — a file that grows beyond ~200 lines should probably be split.
- Example patterns:
  - Small, single-use service + interface: keep `IMyService` and `MyService` in `MyService.cs` if both are small and used together.
  - Shared interfaces used across the codebase: place each in its own file under `Interfaces/` or next to the primary consumer to make it easier to find.
- Always make the interface name clear (`I` prefix for C#) and keep the implementation name descriptive (e.g., `EmailSender` implements `IEmailSender`).

Tests and naming
- Tests live under a `Tests/` project or `*.Tests` project per conventional .NET layouts.
- Name test classes after the class under test and test methods in the pattern: MethodUnderTest_StateUnderTest_ExpectedBehavior. Example: `Calculate_WhenInputIsEmpty_ReturnsZero`.

Commit and PR guidance
- Keep commits small and focused; one logical change per commit.
- Write clear commit messages: a short summary line (50 chars or less) and an optional longer body describing the why.
- Open PRs against `main` (or the team's primary integration branch). Include a description of what changed and why, plus any migration or configuration steps.

Examples

Small service and interface co-located in one file (allowed):

// File: Services/TimeReporter.cs
public interface ITimeReporter
{
	Task ReportAsync(DateTime timestamp, TimeSpan duration);
}

public class TimeReporter : ITimeReporter
{
	private readonly ILogger<TimeReporter> _logger;

	public TimeReporter(ILogger<TimeReporter> logger) => _logger = logger;

	public async Task ReportAsync(DateTime timestamp, TimeSpan duration)
	{
		// minimal example implementation
		_logger.LogInformation("Reported {Duration} at {Timestamp}", duration, timestamp);
		await Task.CompletedTask;
	}
}

Separate files example (preferred for larger/shared types):

// File: Services/IEmailSender.cs
public interface IEmailSender
{
	Task SendAsync(string to, string subject, string body);
}

// File: Services/EmailSender.cs
public class EmailSender : IEmailSender
{
	public Task SendAsync(string to, string subject, string body)
	{
		// implementation omitted
		return Task.CompletedTask;
	}
}

Quick reference checklist
- Prefer small, focused commits and PRs
- Use PascalCase file/type names
- Co-locate interface + implementation only when small and cohesive
- Name async methods with `Async` suffix
- Tests follow Method_State_Expected pattern

Notes and assumptions
- These guidelines are intentionally lightweight. If your team already uses a stricter style guide (for example, EditorConfig or StyleCop rules), prefer those automated rules as the source of truth.
- The one-line rule in the original file had a typo and lacked detail — this file expands that guidance and provides examples.

Contributing
- If you'd like to suggest changes to these standards, open a PR with concrete examples and rationale.

License
- Follow the repository LICENSE for distribution and contribution terms.