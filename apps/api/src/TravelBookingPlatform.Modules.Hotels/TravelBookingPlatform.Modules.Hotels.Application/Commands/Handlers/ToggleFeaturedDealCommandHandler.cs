using MediatR;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands.Handlers;

public class ToggleFeaturedDealCommandHandler : IRequestHandler<ToggleFeaturedDealCommand>
{
    private readonly IDealRepository _dealRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleFeaturedDealCommandHandler(IDealRepository dealRepository, IUnitOfWork unitOfWork)
    {
        _dealRepository = dealRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ToggleFeaturedDealCommand request, CancellationToken cancellationToken)
    {
        var deal = await _dealRepository.GetByIdAsync(request.Id);
        if (deal == null)
            throw new NotFoundException("Deal", request.Id);

        deal.ToggleFeatured();
        _dealRepository.Update(deal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}