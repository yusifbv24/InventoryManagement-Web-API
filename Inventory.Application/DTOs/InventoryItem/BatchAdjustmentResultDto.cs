namespace Inventory.Application.DTOs.InventoryItem
{
    public record BatchAdjustmentResultDto
    {
        public List<InventoryItemDto> SuccessfulAdjustments { get; set; } = new List<InventoryItemDto>();
        public List<FailedAdjustmentDto> FailedAdjustments { get; set; } = new List<FailedAdjustmentDto>();
        public int TotalCount => SuccessfulAdjustments.Count + FailedAdjustments.Count;
        public int SuccessCount => SuccessfulAdjustments.Count;
        public int FailCount => FailedAdjustments.Count;
    }
}
