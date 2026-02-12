using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHAFacilitationAPIs.Application.Feature.UserFamily.Commands.AddUserFamilyCommandHandler;
public class AddUserFamilyResponse
{
    public string Message { get; set; } = "";
    public bool Success { get; set; }
    public Guid Id { get; set; }
}
