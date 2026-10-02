using System.ComponentModel.DataAnnotations;
using DeskShare.Domain;

namespace DeskShare.Api.Contracts;

public sealed record DevelopmentSignInRequest(
    [Required, StringLength(EmployeeLimits.DisplayNameMaxLength)] string DisplayName,
    [Required, EmailAddress, StringLength(EmployeeLimits.EmailMaxLength)] string Email,
    bool IsOfficeManager);
