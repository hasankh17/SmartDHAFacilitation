using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace DHAFacilitationAPIs.Application.Feature.UserFamily.Commands.UpdateUserFamilyCommandHandler;

using MediatR;

public class UpdateUserFamilyCommand : IRequest<UpdateUserFamilyResponse>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }  
        public string Name { get; set; } = string.Empty;
        public int Relation { get; set; }
        public DateTime DOB { get; set; }
    
    }

