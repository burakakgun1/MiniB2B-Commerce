using Microsoft.EntityFrameworkCore;
using MiniB2B.Business.Services.Interfaces;
using MiniB2B.Core.Models;
using MiniB2B.DataAccess.Context;

namespace MiniB2B.Business.Services.Implementations;

public class GridConfigService : IGridConfigService
{
    private readonly MiniB2BDbContext _context;

    public GridConfigService(MiniB2BDbContext context)
    {
        _context = context;
    }

    public async Task<List<GridColumnDto>> GetColumnsAsync(string gridName = "ProductB2BGrid")
    {
        return await _context.GridColumnConfigs
            .AsNoTracking()
            .Where(g => g.GridName == gridName)
            .OrderBy(g => g.DisplayOrder)
            .Select(g => new GridColumnDto
            {
                Id = g.Id,
                GridName = g.GridName,
                PropertyName = g.PropertyName,
                HeaderTitle = g.HeaderTitle,
                DisplayOrder = g.DisplayOrder,
                IsVisible = g.IsVisible,
                RenderType = g.RenderType,
                Width = g.Width,
                Alignment = g.Alignment,
                VisibleOnMobile = g.VisibleOnMobile,
                VisibleOnTablet = g.VisibleOnTablet,
                VisibleOnDesktop = g.VisibleOnDesktop
            })
            .ToListAsync();
    }

    public async Task<UpdateGridColumnDto?> GetColumnForEditAsync(int id)
    {
        var col = await _context.GridColumnConfigs.FindAsync(id);
        if (col == null) return null;

        return new UpdateGridColumnDto
        {
            Id = col.Id,
            HeaderTitle = col.HeaderTitle,
            DisplayOrder = col.DisplayOrder,
            IsVisible = col.IsVisible,
            Width = col.Width,
            Alignment = col.Alignment,
            VisibleOnMobile = col.VisibleOnMobile,
            VisibleOnTablet = col.VisibleOnTablet,
            VisibleOnDesktop = col.VisibleOnDesktop
        };
    }

    public async Task<ServiceResult> UpdateColumnAsync(UpdateGridColumnDto dto)
    {
        var col = await _context.GridColumnConfigs.FindAsync(dto.Id);
        if (col == null)
        {
            return ServiceResult.Failure("Kolon yapılandırması bulunamadı.");
        }

        col.HeaderTitle = dto.HeaderTitle.Trim();
        col.DisplayOrder = dto.DisplayOrder;
        col.IsVisible = dto.IsVisible;
        col.Width = dto.Width?.Trim();
        col.Alignment = dto.Alignment;
        col.VisibleOnMobile = dto.VisibleOnMobile;
        col.VisibleOnTablet = dto.VisibleOnTablet;
        col.VisibleOnDesktop = dto.VisibleOnDesktop;
        col.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ServiceResult.Success("Kolon ayarları güncellendi.");
    }
}
