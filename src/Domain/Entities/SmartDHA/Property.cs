using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DHAFacilitationAPIs.Domain.Entities.SmartDHA;

public class Property : BaseAuditableEntity
{
    public CategoryType? Category { get; set; }
    public PropertyType? Type { get; set; }
    public Phase? Phase { get; set; }
    public Zone? Zone { get; set; }
    public string? Khayaban { get; set; }
    public int? Floor { get; set; }
    public string? StreetNo { get; set; }
    public string? PlotNo { get; set; }
    public string? Plot { get; set; }
    public PossessionType PossessionType { get; set; }

    public string? ProofOfPossessionImage { get; set; }
    public string? UtilityBillAttachment { get; set; }
    public Guid ApplicationUserId { get; set; }   // foreign key
    public ApplicationUser ApplicationUser { get; set; } = null!;  // navigation property
}
