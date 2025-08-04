using MediatR;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Modules.Identity.Application.Services;
using TravelBookingPlatform.Modules.Identity.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Identity.Application.Commands.Handlers;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthenticationService _authenticationService;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IAuthenticationService authenticationService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _authenticationService = authenticationService;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        // Verify current password
        if (!_authenticationService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect.");
        }

        // Hash new password and update
        var newHashedPassword = _authenticationService.HashPassword(request.NewPassword);
        user.UpdatePassword(newHashedPassword);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}