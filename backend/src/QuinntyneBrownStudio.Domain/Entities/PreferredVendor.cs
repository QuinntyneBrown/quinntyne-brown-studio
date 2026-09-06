using QuinntyneBrownStudio.Domain.Enums;

namespace QuinntyneBrownStudio.Domain.Entities;

public sealed class PreferredVendor : Entity
{
    public string Name { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public VendorRole[] Roles { get; set; } = [];
}
