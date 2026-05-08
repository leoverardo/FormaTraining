using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Auth;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;

    public AuthService(AppDbContext db, IJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterTrainerAsync(RegisterTrainerRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email.ToLower()))
            return ApiResponse<AuthResponse>.Fail("E-mail já está em uso.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Trainer,
            IsActive = true
        };
        _db.Users.Add(user);

        var trainer = new Trainer
        {
            UserId = user.Id,
            BrandName = request.BrandName,
            Phone = request.Phone
        };
        _db.Trainers.Add(trainer);

        await _db.SaveChangesAsync();

        var token = _jwt.GenerateToken(user, trainer.Id, null);
        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            User = MapUserDto(user, trainer.Id, null)
        });
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users
            .Include(u => u.Trainer)
            .Include(u => u.Student)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ApiResponse<AuthResponse>.Fail("E-mail ou senha incorretos.");

        if (!user.IsActive)
            return ApiResponse<AuthResponse>.Fail("Conta inativa. Entre em contato com o suporte.");

        var trainerId = user.Trainer?.Id;
        var studentId = user.Student?.Id;

        var token = _jwt.GenerateToken(user, trainerId, studentId);
        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            User = MapUserDto(user, trainerId, studentId)
        });
    }

    public async Task<ApiResponse<UserDto>> MeAsync(Guid userId)
    {
        var user = await _db.Users
            .Include(u => u.Trainer)
            .Include(u => u.Student)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return ApiResponse<UserDto>.Fail("Usuário não encontrado.");

        return ApiResponse<UserDto>.Ok(MapUserDto(user, user.Trainer?.Id, user.Student?.Id));
    }

    private static UserDto MapUserDto(User user, Guid? trainerId, Guid? studentId) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role.ToString(),
        TrainerId = trainerId,
        StudentId = studentId,
        MustChangePassword = user.MustChangePassword
    };
}
