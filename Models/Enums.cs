using System.ComponentModel.DataAnnotations;

namespace HomeBridge.Models;

// Kind of dwelling a listing represents; drives the public type filter.
public enum PropertyType
{
    Apartment,
    Unit,
    House,
    Townhouse,
    [Display(Name = "Studio / Bedsit")]
    Studio,
    [Display(Name = "Shared Room")]
    SharedRoom,
    [Display(Name = "Emergency / Crisis")]
    Emergency
}

// Lifecycle of a tenancy application, reviewed by an administrator.
public enum ApplicationStatus
{
    Pending,
    Approved,
    Rejected,
    Withdrawn
}
