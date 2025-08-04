using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Identity.Application.DTOs;
using TravelBookingPlatform.Modules.Identity.Application.Services;

namespace TravelBookingPlatform.Modules.Identity.Application.Commands.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public LoginCommandHandler(
        IAuthenticationService authenticationService,
        ITokenService tokenService,
        IMapper mapper)
    {
        _authenticationService = authenticationService;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _authenticationService.AuthenticateAsync(request.Email, request.Password);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is deactivated.");
        }

        var token = _tokenService.GenerateToken(user);
        var userDto = _mapper.Map<UserDto>(user);

        return new AuthResponseDto
        {
            Token = token,
            User = userDto
        };
    }
}