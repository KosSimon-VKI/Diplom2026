namespace GardenNookApi.Services
{
    public sealed class PickupSchedulingOptions
    {
        public const string SectionName = "PickupScheduling";

        public int TakeawayOrderTypeId { get; set; } = 2;
        public string StartTime { get; set; } = "08:00";
        public string EndTime { get; set; } = "21:00";
        public int SlotStepMinutes { get; set; } = 15;
        public int MinLeadMinutes { get; set; } = 15;
        public int DaysAhead { get; set; } = 0;
    }
}
