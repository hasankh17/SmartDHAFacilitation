using System.Threading;
using System.Threading.Tasks;
using DHAFacilitationAPIs.Application.Common.Interfaces;
using DHAFacilitationAPIs.Application.Feature.UserFamily.Commands.AddUserFamilyCommandHandler;
using DHAFacilitationAPIs.Application.Interface.Service;
using DHAFacilitationAPIs.Domain.Entities;
using DHAFacilitationAPIs.Domain.Enums;
using MediatR;

namespace DHAFacilitationAPIs.Application.Feature.UserFamily.UserFamilyCommands.AddUserFamilyCommandHandler;

public class AddUserFamilyCommandHandler : IRequestHandler<AddUserFamilyCommand, AddUserFamilyResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    public AddUserFamilyCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<AddUserFamilyResponse> Handle(AddUserFamilyCommand request, CancellationToken cancellationToken)
    {
        var response = new AddUserFamilyResponse();
        string? profileRelativePath = null;
        if (request.ProfilePicture is not null && request.ProfilePicture.Length > 0)
        {
            // use a folder per user or per family: e.g., "user-family/{userId}"
            var folder = $"user-family/{request.UserId}";
            profileRelativePath = await _fileStorage.SaveFileAsync(request.ProfilePicture, folder, cancellationToken);
        }
        var entity = new DHAFacilitationAPIs.Domain.Entities.UserFamily
        {
            Name = request.Name,
            Relation = (Relation)request.Relation,
            DateOfBirth = request.DOB,
            Cnic = request.CNIC,
            FatherOrHusbandName = request.FatherName,
            ProfilePicture = profileRelativePath,
            PhoneNumber = request.PhoneNo,
        };

        await _context.UserFamilies.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        response.Success = true;
        response.Message = "Family member added successfully.";
        response.Id = entity.Id;
        return response;
    }
}
