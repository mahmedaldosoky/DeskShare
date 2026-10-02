using AutoMapper;
using DeskShare.Application.Bookings;
using DeskShare.Application.Desks;
using DeskShare.Domain;
using DeskShare.Domain.Bookings;
using DeskShare.Domain.Desks;

namespace DeskShare.Application;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DeskFeatures, IReadOnlyList<DeskFeatures>>()
            .ConvertUsing(features => Enum.GetValues<DeskFeatures>()
                .Where(feature => feature != DeskFeatures.None && features.HasFlag(feature))
                .ToList());

        CreateMap<Desk, DeskDto>();

        // DeskCode and DeskFloor are flattened from Booking.Desk by convention.
        CreateMap<Booking, BookingDto>()
            .ForCtorParam(nameof(BookingDto.EmployeeName), option => option.MapFrom(booking => booking.Employee.DisplayName));
    }
}
