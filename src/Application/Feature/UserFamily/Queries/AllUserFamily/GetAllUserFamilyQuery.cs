using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;


namespace DHAFacilitationAPIs.Application.Feature.UserFamily.Queries.AllUserFamily;

public class GetAllUserFamilyQuery : IRequest<List<GetAllUserFamilyQueryResponse>>
{
}
