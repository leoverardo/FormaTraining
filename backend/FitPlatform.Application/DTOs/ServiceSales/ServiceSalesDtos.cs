using System.ComponentModel.DataAnnotations;

namespace FitPlatform.Application.DTOs.ServiceSales;

public class TrainerServiceOfferRequest
{
    [Required, MaxLength(120)] public string Title { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; set; }
    [Range(1, 1000000)] public decimal Price { get; set; }
    [Required] public string BillingType { get; set; } = "OneTime";
    [Range(1, 365)] public int? DurationDays { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = true;
    [Range(0, 500)] public int DisplayOrder { get; set; }
}

public class TrainerServiceOfferStatusRequest
{
    public bool IsActive { get; set; }
}

public class TrainerServiceOfferVisibilityRequest
{
    public bool IsPublic { get; set; }
}

public class TrainerServiceOfferResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string BillingType { get; set; } = string.Empty;
    public int? DurationDays { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PublicServiceOfferResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string BillingType { get; set; } = string.Empty;
    public int? DurationDays { get; set; }
}

public class CreateServiceOrderPublicRequest
{
    [Required, MaxLength(120)] public string BuyerName { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(160)] public string BuyerEmail { get; set; } = string.Empty;
    [MaxLength(40)] public string? BuyerPhone { get; set; }
}

public class CreateServiceOrderStudentRequest
{
    [Required] public Guid ServiceOfferId { get; set; }
}

public class ServiceOrderCreatedResponse
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
}

public class TrainerServiceOrderResponse
{
    public Guid Id { get; set; }
    public Guid ServiceOfferId { get; set; }
    public string ServiceTitle { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string? BuyerPhone { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentProvider { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? LeadId { get; set; }
    public bool RequiresManualStudentLinking { get; set; }
}

public class TrainerServiceOrderQuery
{
    public string? Status { get; set; }
    public Guid? ServiceOfferId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class TrainerServiceOrderSummaryResponse
{
    public int TotalOrders { get; set; }
    public int ApprovedOrders { get; set; }
    public int PendingOrders { get; set; }
    public int RejectedOrders { get; set; }
    public decimal ApprovedVolume { get; set; }
}

public class OwnerServiceSalesSummaryResponse
{
    public int TotalOrders { get; set; }
    public int ApprovedOrders { get; set; }
    public int PendingOrders { get; set; }
    public int RejectedOrders { get; set; }
    public decimal ApprovedVolume { get; set; }
    public int TrainersWithPublicOffers { get; set; }
    public int ActiveOffers { get; set; }
}
