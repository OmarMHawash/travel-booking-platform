using MediatR;

namespace TravelBookingPlatform.Core.Application.Commands;

public interface ICommand : IRequest { }
    
public interface ICommand<out TResponse> : IRequest<TResponse> { }