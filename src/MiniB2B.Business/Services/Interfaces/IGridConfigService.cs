using MiniB2B.Core.Models;

namespace MiniB2B.Business.Services.Interfaces;

public interface IGridConfigService
{
    Task<List<GridColumnDto>> GetColumnsAsync(string gridName = "ProductB2BGrid");
    Task<UpdateGridColumnDto?> GetColumnForEditAsync(int id);
    Task<ServiceResult> UpdateColumnAsync(UpdateGridColumnDto dto);
}
