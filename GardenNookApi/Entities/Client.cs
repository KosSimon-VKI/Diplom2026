using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class Client
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Password { get; set; }

    public int? ClientCategoryId { get; set; }

    public int? OrderCount { get; set; }

    public virtual ClientCategory? ClientCategory { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
