using Microsoft.EntityFrameworkCore;
using MiniB2B.Business.Services.Interfaces;
using MiniB2B.Core.Entities;
using MiniB2B.Core.Models;
using MiniB2B.DataAccess.Context;

namespace MiniB2B.Business.Services.Implementations;

public class SliderService : ISliderService
{
    private readonly MiniB2BDbContext _context;

    public SliderService(MiniB2BDbContext context)
    {
        _context = context;
    }

    public async Task<List<SliderDto>> GetActiveSlidersAsync()
    {
        return await _context.SliderItems
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new SliderDto
            {
                Id = s.Id,
                Title = s.Title,
                Subtitle = s.Subtitle,
                ImageUrl = s.ImageUrl,
                TargetUrl = s.TargetUrl,
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<SliderItem>> GetAllSlidersAsync()
    {
        return await _context.SliderItems
            .AsNoTracking()
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();
    }

    public async Task<SliderCreateEditDto?> GetSliderForEditAsync(int id)
    {
        var slider = await _context.SliderItems.FindAsync(id);
        if (slider == null) return null;

        return new SliderCreateEditDto
        {
            Id = slider.Id,
            Title = slider.Title,
            Subtitle = slider.Subtitle,
            ImageUrl = slider.ImageUrl,
            TargetUrl = slider.TargetUrl,
            DisplayOrder = slider.DisplayOrder,
            IsActive = slider.IsActive
        };
    }

    public async Task<ServiceResult<int>> CreateSliderAsync(SliderCreateEditDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            return ServiceResult<int>.Failure("Görsel alanı zorunludur.");
        }

        var slider = new SliderItem
        {
            Title = dto.Title.Trim(),
            Subtitle = dto.Subtitle?.Trim(),
            ImageUrl = dto.ImageUrl.Trim(),
            TargetUrl = dto.TargetUrl?.Trim(),
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            CreatedDate = DateTime.UtcNow
        };

        await _context.SliderItems.AddAsync(slider);
        await _context.SaveChangesAsync();

        return ServiceResult<int>.Success(slider.Id, "Slider başarıyla eklendi.");
    }

    public async Task<ServiceResult> UpdateSliderAsync(SliderCreateEditDto dto)
    {
        var slider = await _context.SliderItems.FindAsync(dto.Id);
        if (slider == null)
        {
            return ServiceResult.Failure("Slider bulunamadı.");
        }

        slider.Title = dto.Title.Trim();
        slider.Subtitle = dto.Subtitle?.Trim();
        if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            slider.ImageUrl = dto.ImageUrl.Trim();
        }
        slider.TargetUrl = dto.TargetUrl?.Trim();
        slider.DisplayOrder = dto.DisplayOrder;
        slider.IsActive = dto.IsActive;
        slider.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ServiceResult.Success("Slider güncellendi.");
    }

    public async Task<ServiceResult> DeleteSliderAsync(int id)
    {
        var slider = await _context.SliderItems.FindAsync(id);
        if (slider == null)
        {
            return ServiceResult.Failure("Slider bulunamadı.");
        }

        _context.SliderItems.Remove(slider);
        await _context.SaveChangesAsync();

        return ServiceResult.Success("Slider silindi.");
    }

    public async Task<ServiceResult> ToggleSliderStatusAsync(int id)
    {
        var slider = await _context.SliderItems.FindAsync(id);
        if (slider == null)
        {
            return ServiceResult.Failure("Slider bulunamadı.");
        }

        slider.IsActive = !slider.IsActive;
        slider.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var statusText = slider.IsActive ? "aktife" : "pasife";
        return ServiceResult.Success($"Slider başarıyla {statusText} alındı.");
    }
}
