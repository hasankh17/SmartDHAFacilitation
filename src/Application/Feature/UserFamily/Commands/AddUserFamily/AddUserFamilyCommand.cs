using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DHAFacilitationAPIs.Application.Feature.UserFamily.Commands.AddUserFamilyCommandHandler;

public class AddUserFamilyCommand : IRequest<AddUserFamilyResponse>
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Relation { get; set; }
    public DateTime DOB { get; set; }
    public string CNIC { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public IFormFile? ProfilePicture { get; set; }
    public string PhoneNo { get; set; } = string.Empty;
}
