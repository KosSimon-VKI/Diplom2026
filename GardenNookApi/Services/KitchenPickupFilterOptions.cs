namespace GardenNookApi.Services
{
    public sealed class KitchenPickupFilterOptions
    {
        public const string SectionName = "KitchenPickupFilter";

        public int WindowMinutes { get; set; } = 30;
    }
}
