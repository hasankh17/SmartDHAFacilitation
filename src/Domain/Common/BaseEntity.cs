using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DHAFacilitationAPIs.Domain.Common;

public abstract class BaseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column(Order = 1)] // Column order in the table
    public int Ser { get; set; } // Auto-increment

    [Key]
    [Column(Order = 2)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
}
