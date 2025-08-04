using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Identity.Application.DTOs;
using TravelBookingPlatform.Modules.Identity.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Identity.Application.Queries.Handlers;

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUserByEmailQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserDto?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        return user == null ? null : _mapper.Map<UserDto>(user);
    }
}