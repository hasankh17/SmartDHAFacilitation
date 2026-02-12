using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DHAFacilitationAPIs.Application.Common.Interfaces;
using DHAFacilitationAPIs.Domain.Entities;

namespace DHAFacilitationAPIs.Application.Feature.UserFamily.Commands.DeleteUserFamilyCommand
{
    public class DeleteUserFamilyCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteUserFamilyCommandHandler : IRequestHandler<DeleteUserFamilyCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteUserFamilyCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteUserFamilyCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.UserFamilies
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null)
                return false;

            _context.UserFamilies.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
