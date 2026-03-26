using System.Collections.Generic;

namespace TransferModels.Orders
{
    public sealed class PickupSlotsResponse
    {
        public int TakeawayOrderTypeId { get; set; }
        public bool IsOptional { get; set; }
        public List<PickupSlotDto> Slots { get; set; } = new List<PickupSlotDto>();
    }

    public sealed class PickupSlotDto
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
