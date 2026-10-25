using Smoke.Mapping.Dtos;
using Transformations.Mapping;

namespace Smoke.Mapping.Domain;

[MapTo<CustomerDto>]
public partial class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
