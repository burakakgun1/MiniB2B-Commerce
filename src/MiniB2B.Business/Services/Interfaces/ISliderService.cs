using MiniB2B.Core.Entities;
using MiniB2B.Core.Models;

namespace MiniB2B.Business.Services.Interfaces;

public interface ISliderService
{
    Task<List<SliderDto>> GetActiveSlidersAsync();
    Task<List<SliderItem>> GetAllSlidersAsync();
    Task<SliderCreateEditDto?> GetSliderForEditAsync(int id);
    Task<ServiceResult<int>> CreateSliderAsync(SliderCreateEditDto dto);
    Task<ServiceResult> UpdateSliderAsync(SliderCreateEditDto dto);
    Task<ServiceResult> DeleteSliderAsync(int id);
    Task<ServiceResult> ToggleSliderStatusAsync(int id);
}
