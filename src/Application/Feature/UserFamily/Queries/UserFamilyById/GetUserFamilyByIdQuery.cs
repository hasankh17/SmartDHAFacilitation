using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHAFacilitationAPIs.Application.Feature.UserFamily.Queries.UserFamilyById;

public class GetUserFamilyByIdQuery : IRequest<GetUserFamilybyIdQueryResponse>
{
    public Guid Id { get; set; }
}
