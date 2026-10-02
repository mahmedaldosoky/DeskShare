using DeskShare.Domain.Common;
using DeskShare.Domain.Desks;
using DeskShare.Domain.Employees;

namespace DeskShare.Domain.Bookings;

public sealed class Booking : AuditedEntity
{
    public Guid DeskId { get; private set; }
    public Desk Desk { get; private set; } = null!;
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public DateOnly Date { get; private set; }

    private Booking()
    {
    }

    public static Booking Create(Desk desk, Employee employee, DateOnly date) =>
        new()
        {
            Desk = desk,
            DeskId = desk.Id,
            Employee = employee,
            EmployeeId = employee.Id,
            Date = date,
        };

    public bool IsOwnedBy(Guid employeeId) => EmployeeId == employeeId;

    public bool IsInPast(DateOnly today) => Date < today;

    public void Reschedule(Desk desk, DateOnly date)
    {
        Desk = desk;
        DeskId = desk.Id;
        Date = date;
    }
}
