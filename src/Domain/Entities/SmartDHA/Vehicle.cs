using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHAFacilitationAPIs.Domain.Entities.SmartDHA;
public class Vehicle : BaseAuditableEntity
{
    public int LicenseNo { get; set; } 
    public string License { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? Attachment { get; set; }
    public string? ETagId { get; set; }
    public DateTime? ValidUpTo { get; set; }
    public Guid ApplicationUserId { get; set; }   // foreign key
    public ApplicationUser ApplicationUser { get; set; } = null!;  // navigation property
}
