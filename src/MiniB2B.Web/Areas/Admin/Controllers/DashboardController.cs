using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniB2B.Core.Enums;
using MiniB2B.Core.Models;
using MiniB2B.DataAccess.Context;

namespace MiniB2B.Web.Areas.Admin.Controllers;

public class DashboardController : AdminBaseController
{
    private readonly MiniB2BDbContext _context;

    public DashboardController(MiniB2BDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalOrders = await _context.Orders.CountAsync();
        var totalRevenue = await _context.Orders.Where(o => o.OrderStatus != OrderStatus.Rejected).SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
        var totalUsers = await _context.Users.CountAsync(u => u.Role == UserRole.Customer);
        var criticalStockCount = await _context.Products.CountAsync(p => p.IsActive && p.StockQuantity <= p.CriticalStockLevel);

        var recentOrders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .Select(o => new OrderListDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                UserId = o.UserId,
                CustomerName = o.User != null ? $"{o.User.FirstName} {o.User.LastName}".Trim() : string.Empty,
                CustomerEmail = o.User != null ? o.User.Email : string.Empty,
                OrderDate = o.OrderDate,
                OrderStatus = o.OrderStatus,
                TotalAmount = o.TotalAmount,
                ItemCount = o.Items.Count
            })
            .ToListAsync();

        var lowStockProducts = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.StockQuantity <= p.CriticalStockLevel)
            .OrderBy(p => p.StockQuantity)
            .Take(6)
            .Select(p => new ProductListItemDto
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                Name = p.Name,
                Brand = p.Brand,
                StockQuantity = p.StockQuantity,
                CriticalStockLevel = p.CriticalStockLevel,
                Price = p.Price,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                StockStatus = p.StockQuantity <= 0 ? StockStatus.OutOfStock : StockStatus.Critical
            })
            .ToListAsync();

        ViewBag.TotalOrders = totalOrders;
        ViewBag.TotalRevenue = totalRevenue;
        ViewBag.TotalUsers = totalUsers;
        ViewBag.CriticalStockCount = criticalStockCount;
        ViewBag.RecentOrders = recentOrders;
        ViewBag.LowStockProducts = lowStockProducts;

        return View();
    }
}
