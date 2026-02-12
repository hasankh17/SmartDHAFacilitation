using System.Threading;
using System.Threading.Tasks;
using DHAFacilitationAPIs.Application.Common.Interfaces;
using DHAFacilitationAPIs.Application.Feature.UserFamily.Commands.UpdateUserFamilyCommandHandler;
using DHAFacilitationAPIs.Domain.Entities;
using DHAFacilitationAPIs.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DHAFacilitationAPIs.Application.Feature.UserFamily.UserFamilyCommands.UpdateUserFamilyCommandHandler
{
    public class UpdateUserFamilyCommandHandler
        : IRequestHandler<UpdateUserFamilyCommand, UpdateUserFamilyResponse>
    {
        private readonly IApplicationDbContext _context;

        public UpdateUserFamilyCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UpdateUserFamilyResponse> Handle(UpdateUserFamilyCommand request, CancellationToken cancellationToken)
        {
            var response = new UpdateUserFamilyResponse();
            var entity = await _context.UserFamilies
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null)
                throw new Exception("No Record Found!");

            entity.ApplicationUserId = request.UserId;
            entity.Name = request.Name;
            entity.Relation = (Relation)request.Relation;
            entity.DateOfBirth = request.DOB;

            await _context.SaveChangesAsync(cancellationToken);

            response.Message = "User family updated successfully.";
            response.Success = true;
            return response;
        }
    }
}
