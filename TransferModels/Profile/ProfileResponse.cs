using System;
using System.Collections.Generic;

namespace TransferModels.Profile
{
    public class ProfileResponse
    {
        public ProfileClientDto Client { get; set; } = new ProfileClientDto();
        public List<ProfileOrderDto> Orders { get; set; } = new List<ProfileOrderDto>();
    }

    public class ProfileClientDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class ProfileOrderDto
    {
        public int OrderId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string OrderType { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public decimal TotalCalories { get; set; }
        public string Comment { get; set; }
        public bool CanCancel { get; set; }
        public List<ProfileOrderCompositionItemDto> Items { get; set; } = new List<ProfileOrderCompositionItemDto>();
    }

    public class ProfileOrderCompositionItemDto
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public List<ProfileOrderCompositionAddonDto> Addons { get; set; } = new List<ProfileOrderCompositionAddonDto>();
    }

    public class ProfileOrderCompositionAddonDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
