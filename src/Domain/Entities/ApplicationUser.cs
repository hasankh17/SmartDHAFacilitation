using Microsoft.AspNetCore.Identity;

namespace DHAFacilitationAPIs.Domain.Entities;
public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = default!;
    public string CNIC { get; set; } = default!;
    public int Floor { get; set; }
    public int PlotNo { get; set; }
    public string Plot { get; set; } = string.Empty;
    public string StreetNo { get; set; } = string.Empty;
    public CategoryType CategoryType { get; set; }
    public PropertyType PropertyType { get; set; }
    public Phase Phase { get; set; }
    public string? FrontSideCnic { get; set; }
    public string? BackSideCnic { get; set; }
    public string? Image { get; set; }
    public string? UtilityBill { get; set; }
    public string? ProofOfResidence { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    // Navigation property
    public ICollection<UserFamily> UserFamilies { get; set; } = new List<UserFamily>();
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
