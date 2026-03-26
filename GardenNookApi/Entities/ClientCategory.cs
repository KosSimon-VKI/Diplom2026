using System;
using System.Collections.Generic;

namespace GardenNookApi.Entities;

public partial class ClientCategory
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
}
