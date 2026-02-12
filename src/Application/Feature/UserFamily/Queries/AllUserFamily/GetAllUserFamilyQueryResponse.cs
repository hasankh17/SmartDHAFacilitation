using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHAFacilitationAPIs.Application.Feature.UserFamily.Queries.AllUserFamily;

public class GetAllUserFamilyQueryResponse
{
    public string Name { get; set; } = string.Empty;
    public int Relation { get; set; }
    public string Phone { get; set; } = string.Empty;
    public DateTime DOB { get; set; }
    public string ResidentCardNumber { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string CNIC { get; set; } = string.Empty;

}
