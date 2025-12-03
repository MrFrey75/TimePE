using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TimePE.Core.Models;
using TimePE.Core.Services;
using TimePE.Api.DTOs;

namespace TimePE.Api.Controllers
{
    [ApiController]
    [Route("api/v1/projects")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly ProjectService _projectService;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(ProjectService projectService, ILogger<ProjectsController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        // GET: api/v1/projects
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            try
            {
                var projects = await _projectService.GetAllProjectsAsync();
                var projectDtos = projects.Select(p => new ProjectDto
                {
                    Id = p.Oid,
                    Name = p.Name,
                    Description = p.Description,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });
                return Ok(projectDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching projects");
                throw;
            }
        }

        // GET: api/v1/projects/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Message = $"Project with ID {id} not found",
                        StatusCode = 404,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                var projectDto = new ProjectDto
                {
                    Id = project.Oid,
                    Name = project.Name,
                    Description = project.Description,
                    IsActive = project.IsActive,
                    CreatedAt = project.CreatedAt,
                    UpdatedAt = project.UpdatedAt
                };
                return Ok(projectDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching project {ProjectId}", id);
                throw;
            }
        }

        // POST: api/v1/projects
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
        {
            try
            {
                var project = await _projectService.CreateProjectAsync(dto.Name, dto.Description);
                var projectDto = new ProjectDto
                {
                    Id = project.Oid,
                    Name = project.Name,
                    Description = project.Description,
                    IsActive = project.IsActive,
                    CreatedAt = project.CreatedAt,
                    UpdatedAt = project.UpdatedAt
                };
                return CreatedAtAction(nameof(GetProject), new { id = project.Oid }, projectDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                throw;
            }
        }

        // PUT: api/v1/projects/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectDto dto)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Message = $"Project with ID {id} not found",
                        StatusCode = 404,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                if (dto.Name != null) project.Name = dto.Name;
                if (dto.Description != null) project.Description = dto.Description;
                if (dto.IsActive.HasValue) project.IsActive = dto.IsActive.Value;

                await _projectService.UpdateProjectAsync(project);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project {ProjectId}", id);
                throw;
            }
        }

        // DELETE: api/v1/projects/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Message = $"Project with ID {id} not found",
                        StatusCode = 404,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                await _projectService.DeleteProjectAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project {ProjectId}", id);
                throw;
            }
        }
    }
}
