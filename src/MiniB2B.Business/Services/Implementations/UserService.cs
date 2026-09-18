using Microsoft.EntityFrameworkCore;
using MiniB2B.Business.Services.Interfaces;
using MiniB2B.Core.Entities;
using MiniB2B.Core.Enums;
using MiniB2B.Core.Models;
using MiniB2B.Core.Security;
using MiniB2B.DataAccess.Context;

namespace MiniB2B.Business.Services.Implementations;

public class UserService : IUserService
{
    private readonly MiniB2BDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(MiniB2BDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<ServiceResult<User>> AuthenticateAsync(string usernameOrEmail, string password)
    {
        var identifier = usernameOrEmail.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == identifier || u.Username.ToLower() == identifier);

        if (user == null)
        {
            return ServiceResult<User>.Failure("Kullanıcı adı/e-posta veya şifre hatalı.");
        }

        if (!user.IsActive)
        {
            return ServiceResult<User>.Failure("Kullanıcı hesabınız pasif durumdadır. Yönetici ile iletişime geçiniz.");
        }

        var isValid = _passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
        if (!isValid)
        {
            return ServiceResult<User>.Failure("Kullanıcı adı/e-posta veya şifre hatalı.");
        }

        return ServiceResult<User>.Success(user);
    }

    public async Task<ServiceResult<int>> RegisterAsync(RegisterViewModel model)
    {
        var email = model.Email.Trim().ToLowerInvariant();
        var username = model.Username.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
        {
            return ServiceResult<int>.Failure("Bu e-posta adresi zaten kullanımda.");
        }

        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == username))
        {
            return ServiceResult<int>.Failure("Bu kullanıcı adı zaten kullanımda.");
        }

        var (hash, salt) = _passwordHasher.HashPassword(model.Password);

        var user = new User
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Email = model.Email.Trim(),
            Username = model.Username.Trim(),
            PasswordHash = hash,
            PasswordSalt = salt,
            PhoneNumber = model.PhoneNumber?.Trim(),
            Role = UserRole.Customer,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return ServiceResult<int>.Success(user.Id, "Kayıt başarıyla tamamlandı.");
    }

    public async Task<PagedResult<UserListDto>> GetUsersAsync(UserFilterDto filter)
    {
        var query = _context.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var search = filter.SearchText.Trim();
            var pattern = $"%{search}%";

            query = query.Where(u =>
                EF.Functions.Like(u.FirstName, pattern) ||
                EF.Functions.Like(u.LastName, pattern) ||
                EF.Functions.Like(u.Email, pattern) ||
                EF.Functions.Like(u.Username, pattern) ||
                (u.PhoneNumber != null && EF.Functions.Like(u.PhoneNumber, pattern)));
        }

        if (filter.Role.HasValue)
        {
            query = query.Where(u => u.Role == filter.Role.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        var totalCount = await query.CountAsync();
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = await query
            .OrderByDescending(u => u.CreatedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Username = u.Username,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedDate = u.CreatedDate,
                OrderCount = u.Orders.Count
            })
            .ToListAsync();

        return new PagedResult<UserListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<List<UserListDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Select(u => new UserListDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Username = u.Username,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedDate = u.CreatedDate,
                OrderCount = u.Orders.Count
            })
            .OrderByDescending(u => u.CreatedDate)
            .ToListAsync();
    }

    public async Task<UserEditDto?> GetUserForEditAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;

        return new UserEditDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Username = user.Username,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }

    public async Task<ServiceResult> UpdateUserAsync(UserEditDto dto)
    {
        var user = await _context.Users.FindAsync(dto.Id);
        if (user == null)
        {
            return ServiceResult.Failure("Kullanıcı bulunamadı.");
        }

        var email = dto.Email.Trim().ToLowerInvariant();
        var username = dto.Username.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email && u.Id != dto.Id))
        {
            return ServiceResult.Failure("Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor.");
        }

        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == username && u.Id != dto.Id))
        {
            return ServiceResult.Failure("Bu kullanıcı adı başka bir kullanıcı tarafından kullanılıyor.");
        }

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.Email = dto.Email.Trim();
        user.Username = dto.Username.Trim();
        user.PhoneNumber = dto.PhoneNumber?.Trim();
        user.Role = dto.Role;
        user.IsActive = dto.IsActive;
        user.UpdatedDate = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            var (hash, salt) = _passwordHasher.HashPassword(dto.NewPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Success("Kullanıcı bilgileri güncellendi.");
    }

    public async Task<UserDetailDto?> GetUserDetailAsync(int id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Orders)
                .ThenInclude(o => o.Items)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return null;

        var recentOrders = user.Orders
            .OrderByDescending(o => o.OrderDate)
            .Take(10)
            .Select(o => new OrderListDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                UserId = o.UserId,
                CustomerName = user.FullName,
                CustomerEmail = user.Email,
                OrderDate = o.OrderDate,
                OrderStatus = o.OrderStatus,
                TotalAmount = o.TotalAmount,
                ItemCount = o.Items.Count
            })
            .ToList();

        return new UserDetailDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Username = user.Username,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedDate = user.CreatedDate,
            OrderCount = user.Orders.Count,
            TotalSpent = user.Orders.Where(o => o.OrderStatus != OrderStatus.Rejected).Sum(o => o.TotalAmount),
            RecentOrders = recentOrders
        };
    }

    public async Task<ServiceResult> ToggleUserStatusAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return ServiceResult.Failure("Kullanıcı bulunamadı.");
        }

        if (user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult.Failure("Ana yönetici hesabı pasife alınamaz.");
        }

        user.IsActive = !user.IsActive;
        user.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var statusText = user.IsActive ? "aktife" : "pasife";
        return ServiceResult.Success($"Kullanıcı hesabı başarıyla {statusText} alındı.");
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }
}
