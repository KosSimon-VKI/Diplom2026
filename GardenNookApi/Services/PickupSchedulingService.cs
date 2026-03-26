using System.Globalization;
using Microsoft.Extensions.Options;
using TransferModels.Orders;

namespace GardenNookApi.Services
{
    public interface IPickupSchedulingService
    {
        int TakeawayOrderTypeId { get; }
        bool IsOptional { get; }
        PickupSlotsResponse BuildSlotsResponse(DateTime? nowLocal = null);
        bool IsPickupAtAllowed(DateTime pickupAt, DateTime? nowLocal = null);
    }

    public sealed class PickupSchedulingService : IPickupSchedulingService
    {
        private const string SlotValueFormat = "yyyy-MM-ddTHH:mm:ss";
        private const string SlotLabelFormat = "HH:mm";

        private readonly PickupSchedulingOptions _options;
        private readonly TimeSpan _startTime;
        private readonly TimeSpan _endTime;
        private readonly TimeSpan _slotStep;
        private readonly TimeSpan _minLead;

        public PickupSchedulingService(IOptions<PickupSchedulingOptions> options)
        {
            _options = options.Value ?? new PickupSchedulingOptions();

            if (_options.TakeawayOrderTypeId <= 0)
            {
                throw new InvalidOperationException("PickupScheduling.TakeawayOrderTypeId должен быть > 0.");
            }

            if (!TryParseConfiguredTime(_options.StartTime, out _startTime))
            {
                throw new InvalidOperationException("PickupScheduling.StartTime должен быть в формате HH:mm.");
            }

            if (!TryParseConfiguredTime(_options.EndTime, out _endTime))
            {
                throw new InvalidOperationException("PickupScheduling.EndTime должен быть в формате HH:mm.");
            }

            if (_endTime < _startTime)
            {
                throw new InvalidOperationException("PickupScheduling.EndTime не может быть раньше StartTime.");
            }

            if (_options.SlotStepMinutes <= 0)
            {
                throw new InvalidOperationException("PickupScheduling.SlotStepMinutes должен быть > 0.");
            }

            if (_options.MinLeadMinutes < 0)
            {
                throw new InvalidOperationException("PickupScheduling.MinLeadMinutes не может быть < 0.");
            }

            if (_options.DaysAhead < 0)
            {
                throw new InvalidOperationException("PickupScheduling.DaysAhead не может быть < 0.");
            }

            _slotStep = TimeSpan.FromMinutes(_options.SlotStepMinutes);
            _minLead = TimeSpan.FromMinutes(_options.MinLeadMinutes);
        }

        public int TakeawayOrderTypeId => _options.TakeawayOrderTypeId;

        public bool IsOptional => true;

        public PickupSlotsResponse BuildSlotsResponse(DateTime? nowLocal = null)
        {
            var now = nowLocal ?? DateTime.Now;

            var slots = BuildSlotValues(now)
                .Select(slot => new PickupSlotDto
                {
                    Value = ToSlotKey(slot),
                    Label = slot.ToString(SlotLabelFormat, CultureInfo.InvariantCulture)
                })
                .ToList();

            return new PickupSlotsResponse
            {
                TakeawayOrderTypeId = TakeawayOrderTypeId,
                IsOptional = IsOptional,
                Slots = slots
            };
        }

        public bool IsPickupAtAllowed(DateTime pickupAt, DateTime? nowLocal = null)
        {
            var now = nowLocal ?? DateTime.Now;
            var pickupKey = ToSlotKey(NormalizeForComparison(pickupAt));

            foreach (var slot in BuildSlotValues(now))
            {
                if (ToSlotKey(slot) == pickupKey)
                {
                    return true;
                }
            }

            return false;
        }

        private List<DateTime> BuildSlotValues(DateTime nowLocal)
        {
            var now = NormalizeForComparison(nowLocal);
            var slotStartFromNow = now + _minLead;
            var slots = new List<DateTime>();

            for (var dayOffset = 0; dayOffset <= _options.DaysAhead; dayOffset++)
            {
                var day = now.Date.AddDays(dayOffset);
                var dayStart = day.Add(_startTime);
                var dayEnd = day.Add(_endTime);
                var windowStart = dayOffset == 0 ? Max(dayStart, slotStartFromNow) : dayStart;
                var firstSlot = RoundUpToStep(windowStart, _slotStep);

                if (firstSlot > dayEnd)
                {
                    continue;
                }

                for (var slot = firstSlot; slot <= dayEnd; slot = slot.Add(_slotStep))
                {
                    slots.Add(slot);
                }
            }

            return slots;
        }

        private static DateTime NormalizeForComparison(DateTime value)
        {
            var local = value.Kind == DateTimeKind.Utc
                ? value.ToLocalTime()
                : value;

            return new DateTime(
                local.Year,
                local.Month,
                local.Day,
                local.Hour,
                local.Minute,
                local.Second,
                DateTimeKind.Unspecified);
        }

        private static DateTime Max(DateTime left, DateTime right)
            => left >= right ? left : right;

        private static DateTime RoundUpToStep(DateTime value, TimeSpan step)
        {
            var ticksRemainder = value.Ticks % step.Ticks;
            if (ticksRemainder == 0)
            {
                return value;
            }

            return value.AddTicks(step.Ticks - ticksRemainder);
        }

        private static string ToSlotKey(DateTime value)
            => value.ToString(SlotValueFormat, CultureInfo.InvariantCulture);

        private static bool TryParseConfiguredTime(string raw, out TimeSpan time)
        {
            if (TimeSpan.TryParseExact(raw, "hh\\:mm", CultureInfo.InvariantCulture, out time))
            {
                return true;
            }

            return TimeSpan.TryParse(raw, CultureInfo.InvariantCulture, out time);
        }
    }
}
