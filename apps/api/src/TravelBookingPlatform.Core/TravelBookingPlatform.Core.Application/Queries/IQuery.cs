using MediatR;

namespace TravelBookingPlatform.Core.Application.Queries;

public interface IQuery<out TResponse>: IRequest<TResponse> { }