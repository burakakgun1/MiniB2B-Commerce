using MiniB2B.Core.Entities;
using MiniB2B.Core.Models;

namespace MiniB2B.Business.Services.Interfaces;

public interface IUserService
{
    Task<ServiceResult<User>> AuthenticateAsync(string usernameOrEmail, string password);
    Task<ServiceResult<int>> RegisterAsync(RegisterViewModel model);
    Task<PagedResult<UserListDto>> GetUsersAsync(UserFilterDto filter);
    Task<List<UserListDto>> GetAllUsersAsync();
    Task<UserEditDto?> GetUserForEditAsync(int id);
    Task<UserDetailDto?> GetUserDetailAsync(int id);
    Task<ServiceResult> UpdateUserAsync(UserEditDto dto);
    Task<ServiceResult> ToggleUserStatusAsync(int id);
    Task<User?> GetUserByIdAsync(int id);
}
