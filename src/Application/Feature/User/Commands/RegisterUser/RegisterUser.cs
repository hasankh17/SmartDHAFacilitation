using DHAFacilitationAPIs.Application.Common.Interfaces;
using DHAFacilitationAPIs.Application.Common.Models;
using DHAFacilitationAPIs.Application.Interface.Repository;
using DHAFacilitationAPIs.Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace DHAFacilitationAPIs.Application.Feature.User.Commands.RegisterUser;
public record RegisterUserCommand : IRequest<Guid>
{
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RegisterUserCommandHandler(IApplicationDbContext context,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper)); _unitOfWork = unitOfWork;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {

        var User = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };
        var user = await _userManager.CreateAsync(User, request.Password);
        if (!user.Succeeded)
        {
            var errors = user.Errors.Select(e => e.Description).ToList();
            throw new Exception("Failed to create user. Errors: " + string.Join(", ", errors));
        }
        var addUserRole = await _userManager.AddToRoleAsync(User, Roles.Administrator);
        if (!addUserRole.Succeeded)
        {
            var errors = user.Errors.Select(e => e.Description).ToList();
            throw new Exception("Failed to create user. Errors: " + string.Join(", ", errors));
        }

        return new Guid(User.Id);
    }
}
