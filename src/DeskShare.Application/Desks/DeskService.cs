using AutoMapper;
using DeskShare.Application.Abstractions;
using DeskShare.Application.Bookings;
using DeskShare.Application.Common;
using DeskShare.Application.Common.Exceptions;
using DeskShare.Domain.Desks;

namespace DeskShare.Application.Desks;

public sealed class DeskService(
    IDeskRepository desks,
    IBookingRepository bookings,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    TimeProvider timeProvider)
{
    public async Task<IReadOnlyList<DeskDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var allDesks = await desks.GetAllAsync(cancellationToken);
        return mapper.Map<IReadOnlyList<DeskDto>>(allDesks);
    }

    public async Task<IReadOnlyList<DeskDto>> GetAvailableAsync(DateOnly date, CancellationToken cancellationToken)
    {
        BookingDateRules.EnsureDateIsNotInPast(date, timeProvider.GetLocalToday());

        var availableDesks = await desks.GetAvailableOnAsync(date, cancellationToken);
        return mapper.Map<IReadOnlyList<DeskDto>>(availableDesks);
    }

    public async Task<DeskDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var desk = await GetExistingDeskAsync(id, cancellationToken);
        return mapper.Map<DeskDto>(desk);
    }

    public async Task<DeskDto> CreateAsync(SaveDeskRequest request, CancellationToken cancellationToken)
    {
        var desk = Desk.Create(request.Code, request.Floor, request.CombineFeatures());
        await EnsureCodeIsUniqueAsync(desk.Code, excludingDeskId: null, cancellationToken);

        desks.Add(desk);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<DeskDto>(desk);
    }

    public async Task<DeskDto> UpdateAsync(Guid id, SaveDeskRequest request, CancellationToken cancellationToken)
    {
        var desk = await GetExistingDeskAsync(id, cancellationToken);
        desk.UpdateDetails(request.Code, request.Floor, request.CombineFeatures());
        await EnsureCodeIsUniqueAsync(desk.Code, excludingDeskId: desk.Id, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<DeskDto>(desk);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var desk = await GetExistingDeskAsync(id, cancellationToken);
        var today = timeProvider.GetLocalToday();

        if (await bookings.DeskHasBookingsFromAsync(desk.Id, today, cancellationToken))
            throw new ConflictException($"Desk {desk.Code} has upcoming bookings and cannot be deleted.");

        desk.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Desk> GetExistingDeskAsync(Guid id, CancellationToken cancellationToken) =>
        await desks.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Desk", id);

    private async Task EnsureCodeIsUniqueAsync(string code, Guid? excludingDeskId, CancellationToken cancellationToken)
    {
        if (await desks.CodeExistsAsync(code, excludingDeskId, cancellationToken))
            throw new ConflictException($"A desk with code {code} already exists.");
    }
}
