// Services/IUserService.cs
using RaceDay.API.DTOs.User;

namespace RaceDay.API.Services;

public interface IUserService
{
    Task<UserProfileDto?> GetProfileAsync(int userId);
    Task<UserProfileDto?> UpdateProfileAsync(int userId, UpdateUserProfileDto dto);
}