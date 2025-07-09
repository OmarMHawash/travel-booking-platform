using MediatR;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands.Handlers;

public class CreateDealCommandHandler : IRequestHandler<CreateDealCommand, Guid>
{
    private readonly IDealRepository _dealRepository;
    private readonly IHotelRepository _hotelRepository;
    private readonly IRoomTypeRepository _roomTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDealCommandHandler(
        IDealRepository dealRepository,
        IHotelRepository hotelRepository,
        IRoomTypeRepository roomTypeRepository,
        IUnitOfWork unitOfWork)
    {
        _dealRepository = dealRepository;
        _hotelRepository = hotelRepository;
        _roomTypeRepository = roomTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateDealCommand request, CancellationToken cancellationToken)
    {
        var dealData = request.DealData;

        // Validate that the hotel exists
        var hotel = await _hotelRepository.GetByIdAsync(dealData.HotelId);
        if (hotel == null)
        {
            throw new ForeignKeyViolationException("Deal", "Hotel", dealData.HotelId);
        }

        // Validate that the room type exists
        var roomType = await _roomTypeRepository.GetByIdAsync(dealData.RoomTypeId);
        if (roomType == null)
        {
            throw new ForeignKeyViolationException("Deal", "RoomType", dealData.RoomTypeId);
        }

        // Check for overlapping deals
        var overlappingDeal = await _dealRepository.GetOverlappingDealAsync(
            dealData.HotelId,
            dealData.RoomTypeId,
            dealData.ValidFrom,
            dealData.ValidTo);

        if (overlappingDeal != null)
        {
            throw new BusinessValidationException(
                $"An active deal already exists for this hotel and room type during the specified period. Existing deal: '{overlappingDeal.Title}' ({overlappingDeal.ValidFrom:yyyy-MM-dd} to {overlappingDeal.ValidTo:yyyy-MM-dd})",
                "DateRange");
        }

        // Check for duplicate title
        var existingTitleDeal = await _dealRepository.GetByHotelAndTitleAsync(
            dealData.HotelId,
            dealData.Title);

        if (existingTitleDeal != null)
        {
            throw new BusinessValidationException(
                $"A deal with title '{dealData.Title}' already exists for this hotel.",
                "Title");
        }

        try
        {
            var deal = new Deal(
                dealData.Title,
                dealData.Description,
                dealData.HotelId,
                dealData.OriginalPrice,
                dealData.DiscountedPrice,
                dealData.ValidFrom,
                dealData.ValidTo,
                dealData.IsFeatured,
                dealData.RoomTypeId,
                dealData.MaxBookings,
                dealData.ImageURL
            );

            await _dealRepository.AddAsync(deal);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return deal.Id;
        }
        catch (ArgumentException ex)
        {
            throw new BusinessValidationException(ex.Message, ex.ParamName);
        }
    }
}