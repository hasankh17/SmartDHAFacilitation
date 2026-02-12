using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DHAFacilitationAPIs.Application.Common.Interfaces;
using DHAFacilitationAPIs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using DHAFacilitationAPIs.Application.Feature.UserFamily.Queries.AllUserFamily;

namespace DHAFacilitationAPIs.Application.Feature.UserFamily.Queries.AllUserFamily
{
    public class GetAllUserFamilyQueryHandler : IRequestHandler<GetAllUserFamilyQuery, List<GetAllUserFamilyQueryResponse>>
    {
        private readonly IApplicationDbContext _context;

        public GetAllUserFamilyQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<GetAllUserFamilyQueryResponse>> Handle(
    GetAllUserFamilyQuery request,
    CancellationToken cancellationToken)
        {
            return await _context.UserFamilies
                .Include(x => x.ApplicationUser)
                .Select(x => new GetAllUserFamilyQueryResponse
                {
                    DOB = x.DateOfBirth,
                    Name = x.Name,
                    Phone = x.PhoneNumber!,
                    Relation = (int)x.Relation,
                    CNIC = x.Cnic!,
                    Image = x.ProfilePicture ?? string.Empty,
                    ResidentCardNumber = x.ResidentCardNumber!
                })
                .ToListAsync(cancellationToken);
        }

    }
}
