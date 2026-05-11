using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Explore;
using FitPlatform.Application.DTOs.Feed;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using FitPlatform.Infrastructure.Services;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/explore")]
public class ExploreController : ControllerBase
{
    private readonly ExploreService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext _db;

    public ExploreController(ExploreService service, ICurrentUserService currentUser, AppDbContext db)
    {
        _service = service;
        _currentUser = currentUser;
        _db = db;
    }

    private static bool IsSchemaSqlException(Exception ex)
    {
        if (ex is SqlException sqlEx)
            return sqlEx.Number is 207 or 208;

        if (ex is DbUpdateException dbUpdateEx && dbUpdateEx.InnerException is SqlException innerSql)
            return innerSql.Number is 207 or 208;

        return false;
    }

    private async Task<Guid?> ResolveStudentProfileIdAsync(bool autoCreate = false)
    {
        if (_currentUser.StudentProfileId.HasValue)
            return _currentUser.StudentProfileId.Value;

        if (_currentUser.UserId == Guid.Empty)
            return null;

        if (!await StudentProfilesTableExistsAsync())
            return null;

        var profile = await _db.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == _currentUser.UserId);
        if (profile != null)
            return profile.Id;

        if (!autoCreate)
            return null;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId);
        if (user == null || user.Role != UserRole.Student)
            return null;

        var created = new StudentProfile
        {
            UserId = user.Id,
            FullName = string.IsNullOrWhiteSpace(user.Name) ? user.Email : user.Name,
            AccountStatus = StudentAccountStatus.Explorer
        };
        _db.StudentProfiles.Add(created);
        await _db.SaveChangesAsync();
        return created.Id;
    }

    private async Task<bool> StudentProfilesTableExistsAsync()
    {
        try
        {
            await using var connection = _db.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT TOP(1) 1 FROM sys.tables WHERE name = 'StudentProfiles'";
            var result = await command.ExecuteScalarAsync();
            return result != null && result != DBNull.Value;
        }
        catch
        {
            return false;
        }
    }

    [HttpGet("feed")]
    [AllowAnonymous]
    public async Task<IActionResult> Feed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        Guid? userId = User.Identity?.IsAuthenticated == true ? _currentUser.UserId : null;
        var result = await _service.GetExploreFeedAsync(userId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("trainers")]
    [AllowAnonymous]
    public async Task<IActionResult> Trainers([FromQuery] ExploreTrainerQuery query)
    {
        try
        {
            Guid? studentProfileId = null;
            if (User.Identity?.IsAuthenticated == true)
                studentProfileId = await ResolveStudentProfileIdAsync();

            var result = await _service.SearchTrainersAsync(query, studentProfileId);
            return Ok(result);
        }
        catch (Exception ex) when (IsSchemaSqlException(ex))
        {
            return Ok(ApiResponse<TrainerSearchResponseDto>.Ok(new(), "Explore schema not applied yet."));
        }
        catch
        {
            return Ok(ApiResponse<TrainerSearchResponseDto>.Ok(new(), "Explore unavailable right now."));
        }
    }

    [HttpGet("trainers/recommended")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Recommended([FromQuery] decimal? latitude, [FromQuery] decimal? longitude, [FromQuery] string? city, [FromQuery] string? state, [FromQuery] string? goal, [FromQuery] string? interests)
    {
        try
        {
            if (!await StudentProfilesTableExistsAsync())
                return Ok(ApiResponse<TrainerSearchResponseDto>.Ok(new(), "Explore schema not applied yet."));

            var studentProfileId = await ResolveStudentProfileIdAsync();
            var profile = studentProfileId.HasValue
                ? await _db.StudentProfiles.AsNoTracking().FirstOrDefaultAsync(s => s.Id == studentProfileId.Value)
                : null;

            var query = new ExploreTrainerQuery
            {
                Latitude = latitude,
                Longitude = longitude,
                RadiusKm = 50,
                Page = 1,
                PageSize = 12
            };

            if (!latitude.HasValue || !longitude.HasValue)
            {
                query.City = city ?? profile?.City;
                query.State = state ?? profile?.State;
                query.Specialty = string.Join(", ", new[] { interests, goal, profile?.Interests, profile?.Goal }.Where(x => !string.IsNullOrWhiteSpace(x)));
            }

            var result = await _service.SearchTrainersAsync(query, studentProfileId);
            return Ok(result);
        }
        catch (Exception ex) when (IsSchemaSqlException(ex))
        {
            return Ok(ApiResponse<TrainerSearchResponseDto>.Ok(new(), "Explore schema not applied yet."));
        }
        catch
        {
            return Ok(ApiResponse<TrainerSearchResponseDto>.Ok(new(), "Explore unavailable right now."));
        }
    }

    [HttpPost("trainers/{trainerId:guid}/follow")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Follow(Guid trainerId)
    {
        try
        {
            var studentProfileId = await ResolveStudentProfileIdAsync(autoCreate: true);
            if (!studentProfileId.HasValue)
                return BadRequest(ApiResponse.Fail("Student profile not found for current user."));

            var trainer = await _db.Trainers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == trainerId);
            if (trainer == null || !trainer.PublicPageEnabled)
                return NotFound(ApiResponse.Fail("Trainer not found."));

            var exists = await _db.TrainerFollowers.AnyAsync(f => f.TrainerId == trainerId && f.StudentProfileId == studentProfileId.Value);
            if (!exists)
                _db.TrainerFollowers.Add(new TrainerFollower { TrainerId = trainerId, StudentProfileId = studentProfileId.Value });

            await _db.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { trainerId, isFollowed = true }));
        }
        catch (Exception ex) when (IsSchemaSqlException(ex))
        {
            return BadRequest(ApiResponse.Fail("Explore schema not applied yet."));
        }
        catch
        {
            return BadRequest(ApiResponse.Fail("Could not follow trainer right now."));
        }
    }

    [HttpDelete("trainers/{trainerId:guid}/follow")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Unfollow(Guid trainerId)
    {
        var studentProfileId = await ResolveStudentProfileIdAsync(autoCreate: true);
        if (!studentProfileId.HasValue)
            return BadRequest(ApiResponse.Fail("Student profile not found for current user."));

        var entity = await _db.TrainerFollowers.FirstOrDefaultAsync(f => f.TrainerId == trainerId && f.StudentProfileId == studentProfileId.Value);
        if (entity != null)
        {
            _db.TrainerFollowers.Remove(entity);
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpPost("trainers/{trainerId:guid}/save")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Save(Guid trainerId)
    {
        try
        {
            var studentProfileId = await ResolveStudentProfileIdAsync(autoCreate: true);
            if (!studentProfileId.HasValue)
                return BadRequest(ApiResponse.Fail("Student profile not found for current user."));

            var trainer = await _db.Trainers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == trainerId);
            if (trainer == null || !trainer.PublicPageEnabled)
                return NotFound(ApiResponse.Fail("Trainer not found."));

            var exists = await _db.SavedTrainers.AnyAsync(f => f.TrainerId == trainerId && f.StudentProfileId == studentProfileId.Value);
            if (!exists)
                _db.SavedTrainers.Add(new SavedTrainer { TrainerId = trainerId, StudentProfileId = studentProfileId.Value });

            await _db.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { trainerId, isSaved = true }));
        }
        catch (Exception ex) when (IsSchemaSqlException(ex))
        {
            return BadRequest(ApiResponse.Fail("Explore schema not applied yet."));
        }
        catch
        {
            return BadRequest(ApiResponse.Fail("Could not save trainer right now."));
        }
    }

    [HttpDelete("trainers/{trainerId:guid}/save")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Unsave(Guid trainerId)
    {
        var studentProfileId = await ResolveStudentProfileIdAsync(autoCreate: true);
        if (!studentProfileId.HasValue)
            return BadRequest(ApiResponse.Fail("Student profile not found for current user."));

        var entity = await _db.SavedTrainers.FirstOrDefaultAsync(f => f.TrainerId == trainerId && f.StudentProfileId == studentProfileId.Value);
        if (entity != null)
        {
            _db.SavedTrainers.Remove(entity);
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }
}

[ApiController]
[Route("api/student")]
[Authorize(Roles = "Student")]
public class StudentExploreListsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public StudentExploreListsController(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private static bool IsSchemaSqlException(Exception ex)
    {
        if (ex is SqlException sqlEx)
            return sqlEx.Number is 207 or 208;

        if (ex is DbUpdateException dbUpdateEx && dbUpdateEx.InnerException is SqlException innerSql)
            return innerSql.Number is 207 or 208;

        return false;
    }

    private async Task<Guid?> ResolveStudentProfileIdAsync()
    {
        if (_currentUser.StudentProfileId.HasValue)
            return _currentUser.StudentProfileId.Value;

        if (_currentUser.UserId == Guid.Empty)
            return null;

        if (!await StudentProfilesTableExistsAsync())
            return null;

        return await _db.StudentProfiles
            .AsNoTracking()
            .Where(s => s.UserId == _currentUser.UserId)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync();
    }

    private async Task<bool> StudentProfilesTableExistsAsync()
    {
        try
        {
            await using var connection = _db.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT TOP(1) 1 FROM sys.tables WHERE name = 'StudentProfiles'";
            var result = await command.ExecuteScalarAsync();
            return result != null && result != DBNull.Value;
        }
        catch
        {
            return false;
        }
    }

    [HttpGet("following-trainers")]
    public async Task<IActionResult> Following()
    {
        try
        {
            var studentProfileId = await ResolveStudentProfileIdAsync();
            if (!studentProfileId.HasValue)
                return Ok(ApiResponse<List<TrainerSearchResultDto>>.Ok(new()));

            var trainerIds = await _db.TrainerFollowers.Where(f => f.StudentProfileId == studentProfileId.Value).Select(f => f.TrainerId).ToListAsync();
            var trainers = await _db.Trainers.Include(t => t.User).Where(t => trainerIds.Contains(t.Id)).ToListAsync();
            var data = trainers.Select(t => new TrainerSearchResultDto
            {
                TrainerId = t.Id,
                Slug = t.PublicSlug,
                FullName = t.User.Name,
                BrandName = t.BrandName,
                Headline = t.PublicHeadline,
                Bio = t.Bio,
                City = t.City,
                State = t.State,
                Neighborhood = t.Neighborhood,
                Specialties = string.IsNullOrWhiteSpace(t.Specialties)
                    ? new List<string>()
                    : t.Specialties.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.Length > 0).ToList(),
                ProfilePhotoUrl = t.ProfilePhotoUrl,
                BannerUrl = t.BannerUrl,
                IsFollowedByCurrentUser = true
            }).ToList();
            return Ok(ApiResponse<List<TrainerSearchResultDto>>.Ok(data));
        }
        catch (Exception ex) when (IsSchemaSqlException(ex))
        {
            return Ok(ApiResponse<List<TrainerSearchResultDto>>.Ok(new(), "Explore schema not applied yet."));
        }
        catch
        {
            return Ok(ApiResponse<List<TrainerSearchResultDto>>.Ok(new(), "Explore unavailable right now."));
        }
    }

    [HttpGet("saved-trainers")]
    public async Task<IActionResult> Saved()
    {
        try
        {
            var studentProfileId = await ResolveStudentProfileIdAsync();
            if (!studentProfileId.HasValue)
                return Ok(ApiResponse<List<TrainerSearchResultDto>>.Ok(new()));

            var trainerIds = await _db.SavedTrainers.Where(f => f.StudentProfileId == studentProfileId.Value).Select(f => f.TrainerId).ToListAsync();
            var trainers = await _db.Trainers.Include(t => t.User).Where(t => trainerIds.Contains(t.Id)).ToListAsync();
            var data = trainers.Select(t => new TrainerSearchResultDto
            {
                TrainerId = t.Id,
                Slug = t.PublicSlug,
                FullName = t.User.Name,
                BrandName = t.BrandName,
                Headline = t.PublicHeadline,
                Bio = t.Bio,
                City = t.City,
                State = t.State,
                Neighborhood = t.Neighborhood,
                Specialties = string.IsNullOrWhiteSpace(t.Specialties)
                    ? new List<string>()
                    : t.Specialties.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.Length > 0).ToList(),
                ProfilePhotoUrl = t.ProfilePhotoUrl,
                BannerUrl = t.BannerUrl,
                IsSavedByCurrentUser = true
            }).ToList();
            return Ok(ApiResponse<List<TrainerSearchResultDto>>.Ok(data));
        }
        catch (Exception ex) when (IsSchemaSqlException(ex))
        {
            return Ok(ApiResponse<List<TrainerSearchResultDto>>.Ok(new(), "Explore schema not applied yet."));
        }
        catch
        {
            return Ok(ApiResponse<List<TrainerSearchResultDto>>.Ok(new(), "Explore unavailable right now."));
        }
    }
}
