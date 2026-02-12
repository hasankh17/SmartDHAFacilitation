using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediatR;
using System.Threading.Tasks;

namespace DHAFacilitationAPIs.Application.Feature.UserFamily.UserFamilyCommands.DeleteUserFamilyCommand;


public class DeleteUserFamilyCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

