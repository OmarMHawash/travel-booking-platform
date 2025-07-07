using MediatR;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands.Handlers;

public class UpdateDealCommandHandler : IRequestHandler<UpdateDealCommand>
{
    private readonly IDealRepository _dealRepository;
    private readonly IRoomTypeRepository _roomTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDealCommandHandler(IDealRepository dealRepository, IRoomTypeRepository roomTypeRepository, IUnitOfWork unitOfWork)
    {
        _dealRepository = dealRepository;
        _roomTypeRepository = roomTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateDealCommand request, CancellationToken cancellationToken)
    {
        var deal = await _dealRepository.GetByIdAsync(request.Id);
        if (deal == null)
            throw new NotFoundException("Deal", request.Id);

        var dealData = request.DealData;

        // Validate that the room type exists
        var roomType = await _roomTypeRepository.GetByIdAsync(dealData.RoomTypeId);
        if (roomType == null)
        {
            throw new ForeignKeyViolationException("Deal", "RoomType", dealData.RoomTypeId);
        }

        try
        {
            deal.Update(
                dealData.Title,
                dealData.Description,
                dealData.RoomTypeId,
                dealData.OriginalPrice,
                dealData.DiscountedPrice,
                dealData.ValidFrom,
                dealData.ValidTo,
                dealData.MaxBookings,
                dealData.ImageURL
            );

            if (dealData.IsFeatured != deal.IsFeatured)
            {
                deal.ToggleFeatured();
            }

            _dealRepository.Update(deal);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ArgumentException ex)
        {
            throw new BusinessValidationException(ex.Message, ex.ParamName);
        }
    }
}