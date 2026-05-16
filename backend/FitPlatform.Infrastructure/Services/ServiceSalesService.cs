using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FitPlatform.Application.Common;
using FitPlatform.Application.Configuration;
using FitPlatform.Application.DTOs.ServiceSales;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FitPlatform.Infrastructure.Services;

public class ServiceSalesService
{
    private readonly AppDbContext _db;
    private readonly MercadoPagoOptions _mercadoPago;
    private readonly IDateTimeProvider _clock;
    private readonly NotificationService _notifications;

    public ServiceSalesService(
        AppDbContext db,
        IOptions<MercadoPagoOptions> mercadoPago,
        IDateTimeProvider clock,
        NotificationService notifications)
    {
        _db = db;
        _mercadoPago = mercadoPago.Value;
        _clock = clock;
        _notifications = notifications;
    }

    public async Task<ApiResponse<List<TrainerServiceOfferResponse>>> GetTrainerOffersAsync(Guid trainerId)
    {
        var offers = await _db.TrainerServiceOffers
            .Where(o => o.TrainerId == trainerId)
            .OrderBy(o => o.DisplayOrder).ThenBy(o => o.Title)
            .ToListAsync();
        return ApiResponse<List<TrainerServiceOfferResponse>>.Ok(offers.Select(MapOffer).ToList());
    }

    public async Task<ApiResponse<TrainerServiceOfferResponse>> GetTrainerOfferByIdAsync(Guid trainerId, Guid offerId)
    {
        var offer = await _db.TrainerServiceOffers.FirstOrDefaultAsync(o => o.Id == offerId && o.TrainerId == trainerId);
        if (offer == null) return ApiResponse<TrainerServiceOfferResponse>.Fail("Oferta não encontrada.");
        return ApiResponse<TrainerServiceOfferResponse>.Ok(MapOffer(offer));
    }

    public async Task<ApiResponse<TrainerServiceOfferResponse>> CreateOfferAsync(Guid trainerId, TrainerServiceOfferRequest request)
    {
        if (!Enum.TryParse<TrainerServiceBillingType>(request.BillingType, true, out var billingType))
            return ApiResponse<TrainerServiceOfferResponse>.Fail("Tipo de cobrança inválido.");
        if (billingType == TrainerServiceBillingType.MonthlyRecurring)
            return ApiResponse<TrainerServiceOfferResponse>.Fail("Cobrança recorrente B2C será liberada em próxima fase.");

        var offer = new TrainerServiceOffer
        {
            TrainerId = trainerId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            BillingType = billingType,
            DurationDays = request.DurationDays,
            IsActive = request.IsActive,
            IsPublic = request.IsPublic,
            DisplayOrder = request.DisplayOrder
        };
        _db.TrainerServiceOffers.Add(offer);
        await _db.SaveChangesAsync();
        return ApiResponse<TrainerServiceOfferResponse>.Ok(MapOffer(offer), "Oferta criada.");
    }

    public async Task<ApiResponse<TrainerServiceOfferResponse>> UpdateOfferAsync(Guid trainerId, Guid offerId, TrainerServiceOfferRequest request)
    {
        if (!Enum.TryParse<TrainerServiceBillingType>(request.BillingType, true, out var billingType))
            return ApiResponse<TrainerServiceOfferResponse>.Fail("Tipo de cobrança inválido.");
        if (billingType == TrainerServiceBillingType.MonthlyRecurring)
            return ApiResponse<TrainerServiceOfferResponse>.Fail("Cobrança recorrente B2C será liberada em próxima fase.");

        var offer = await _db.TrainerServiceOffers.FirstOrDefaultAsync(o => o.Id == offerId && o.TrainerId == trainerId);
        if (offer == null) return ApiResponse<TrainerServiceOfferResponse>.Fail("Oferta não encontrada.");

        offer.Title = request.Title.Trim();
        offer.Description = request.Description?.Trim();
        offer.Price = request.Price;
        offer.BillingType = billingType;
        offer.DurationDays = request.DurationDays;
        offer.IsActive = request.IsActive;
        offer.IsPublic = request.IsPublic;
        offer.DisplayOrder = request.DisplayOrder;
        offer.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<TrainerServiceOfferResponse>.Ok(MapOffer(offer), "Oferta atualizada.");
    }

    public async Task<ApiResponse<TrainerServiceOfferResponse>> UpdateOfferStatusAsync(Guid trainerId, Guid offerId, bool isActive)
    {
        var offer = await _db.TrainerServiceOffers.FirstOrDefaultAsync(o => o.Id == offerId && o.TrainerId == trainerId);
        if (offer == null) return ApiResponse<TrainerServiceOfferResponse>.Fail("Oferta não encontrada.");
        offer.IsActive = isActive;
        offer.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<TrainerServiceOfferResponse>.Ok(MapOffer(offer));
    }

    public async Task<ApiResponse<TrainerServiceOfferResponse>> UpdateOfferVisibilityAsync(Guid trainerId, Guid offerId, bool isPublic)
    {
        var offer = await _db.TrainerServiceOffers.FirstOrDefaultAsync(o => o.Id == offerId && o.TrainerId == trainerId);
        if (offer == null) return ApiResponse<TrainerServiceOfferResponse>.Fail("Oferta não encontrada.");
        offer.IsPublic = isPublic;
        offer.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<TrainerServiceOfferResponse>.Ok(MapOffer(offer));
    }

    public async Task<ApiResponse<List<PublicServiceOfferResponse>>> GetPublicOffersAsync(string slugOrId)
    {
        var trainer = await ResolvePublicTrainerAsync(slugOrId);
        if (trainer == null) return ApiResponse<List<PublicServiceOfferResponse>>.Fail("Trainer não encontrado.");

        var offers = await _db.TrainerServiceOffers
            .Where(o => o.TrainerId == trainer.Id && o.IsActive && o.IsPublic && o.BillingType == TrainerServiceBillingType.OneTime)
            .OrderBy(o => o.DisplayOrder).ThenBy(o => o.Title)
            .ToListAsync();
        return ApiResponse<List<PublicServiceOfferResponse>>.Ok(offers.Select(MapPublicOffer).ToList());
    }

    public async Task<ApiResponse<ServiceOrderCreatedResponse>> CreatePublicOrderAsync(string slugOrId, Guid offerId, CreateServiceOrderPublicRequest request)
    {
        var trainer = await ResolvePublicTrainerAsync(slugOrId);
        if (trainer == null) return ApiResponse<ServiceOrderCreatedResponse>.Fail("Trainer não encontrado.");
        return await CreateOrderCoreAsync(trainer.Id, offerId, request.BuyerName, request.BuyerEmail, request.BuyerPhone, null);
    }

    public async Task<ApiResponse<ServiceOrderCreatedResponse>> CreateStudentOrderAsync(Guid trainerId, Guid studentId, Guid offerId)
    {
        var student = await _db.Students.Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == studentId && s.TrainerId == trainerId && s.Status == StudentStatus.Active);
        if (student == null) return ApiResponse<ServiceOrderCreatedResponse>.Fail("Aluno não encontrado.");
        return await CreateOrderCoreAsync(trainerId, offerId, student.User.Name, student.User.Email, student.Phone, studentId);
    }

    public async Task<ApiResponse<List<TrainerServiceOrderResponse>>> GetTrainerOrdersAsync(Guid trainerId, TrainerServiceOrderQuery query)
    {
        var q = _db.TrainerServiceOrders.AsNoTracking().Where(o => o.TrainerId == trainerId);
        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<TrainerServiceOrderStatus>(query.Status, true, out var status))
            q = q.Where(o => o.Status == status);
        if (query.ServiceOfferId.HasValue)
            q = q.Where(o => o.ServiceOfferId == query.ServiceOfferId);
        if (query.StartDate.HasValue)
            q = q.Where(o => o.CreatedAt >= query.StartDate.Value);
        if (query.EndDate.HasValue)
            q = q.Where(o => o.CreatedAt <= query.EndDate.Value);

        var orders = await q.OrderByDescending(o => o.CreatedAt).Take(200).ToListAsync();
        return ApiResponse<List<TrainerServiceOrderResponse>>.Ok(orders.Select(MapOrder).ToList());
    }

    public async Task<ApiResponse<TrainerServiceOrderResponse>> GetTrainerOrderByIdAsync(Guid trainerId, Guid orderId)
    {
        var order = await _db.TrainerServiceOrders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId && o.TrainerId == trainerId);
        if (order == null) return ApiResponse<TrainerServiceOrderResponse>.Fail("Pedido não encontrado.");
        return ApiResponse<TrainerServiceOrderResponse>.Ok(MapOrder(order));
    }

    public async Task<ApiResponse<TrainerServiceOrderSummaryResponse>> GetTrainerOrdersSummaryAsync(Guid trainerId)
    {
        var orders = await _db.TrainerServiceOrders
            .Where(o => o.TrainerId == trainerId)
            .ToListAsync();
        return ApiResponse<TrainerServiceOrderSummaryResponse>.Ok(new TrainerServiceOrderSummaryResponse
        {
            TotalOrders = orders.Count,
            ApprovedOrders = orders.Count(o => o.Status == TrainerServiceOrderStatus.Approved),
            PendingOrders = orders.Count(o => o.Status == TrainerServiceOrderStatus.PendingPayment),
            RejectedOrders = orders.Count(o => o.Status == TrainerServiceOrderStatus.Rejected || o.Status == TrainerServiceOrderStatus.Cancelled || o.Status == TrainerServiceOrderStatus.Expired),
            ApprovedVolume = orders.Where(o => o.Status == TrainerServiceOrderStatus.Approved).Sum(o => o.Amount)
        });
    }

    public async Task<ApiResponse<OwnerServiceSalesSummaryResponse>> GetOwnerSummaryAsync()
    {
        var orders = await _db.TrainerServiceOrders.ToListAsync();
        var activeOffers = await _db.TrainerServiceOffers.CountAsync(o => o.IsActive);
        var trainersWithPublicOffers = await _db.TrainerServiceOffers.Where(o => o.IsActive && o.IsPublic).Select(o => o.TrainerId).Distinct().CountAsync();

        return ApiResponse<OwnerServiceSalesSummaryResponse>.Ok(new OwnerServiceSalesSummaryResponse
        {
            TotalOrders = orders.Count,
            ApprovedOrders = orders.Count(o => o.Status == TrainerServiceOrderStatus.Approved),
            PendingOrders = orders.Count(o => o.Status == TrainerServiceOrderStatus.PendingPayment),
            RejectedOrders = orders.Count(o => o.Status == TrainerServiceOrderStatus.Rejected || o.Status == TrainerServiceOrderStatus.Cancelled || o.Status == TrainerServiceOrderStatus.Expired),
            ApprovedVolume = orders.Where(o => o.Status == TrainerServiceOrderStatus.Approved).Sum(o => o.Amount),
            ActiveOffers = activeOffers,
            TrainersWithPublicOffers = trainersWithPublicOffers
        });
    }

    public async Task<ApiResponse> HandleWebhookPaymentAsync(JsonElement payload, string? eventId, string? resourceId, CancellationToken cancellationToken = default)
    {
        var normalizedResourceId = resourceId?.Trim() ?? ExtractPayloadValue(payload, "data.id") ?? ExtractPayloadValue(payload, "id");
        if (string.IsNullOrWhiteSpace(normalizedResourceId))
            return ApiResponse.Ok("Webhook sem payment id.");

        var normalizedEventId = (eventId?.Trim() ?? $"payment-{normalizedResourceId}").Trim();
        var existingLog = await _db.PaymentWebhookLogs
            .FirstOrDefaultAsync(x => x.Provider == "MercadoPagoB2C" && x.EventId == normalizedEventId, cancellationToken);
        if (existingLog != null) return ApiResponse.Ok("Webhook já processado.");

        _db.PaymentWebhookLogs.Add(new PaymentWebhookLog
        {
            Provider = "MercadoPagoB2C",
            EventId = normalizedEventId,
            Type = "payment",
            ResourceId = normalizedResourceId,
            RawPayload = payload.ValueKind == JsonValueKind.Undefined ? "{}" : payload.GetRawText()
        });
        await _db.SaveChangesAsync(cancellationToken);

        var payment = await GetMercadoPagoPaymentAsync(normalizedResourceId, cancellationToken);
        if (payment == null) return ApiResponse.Ok("Pagamento não encontrado no provider.");
        if (!Guid.TryParse(payment.ExternalReference, out var orderId)) return ApiResponse.Ok("Payment sem referência local.");

        var order = await _db.TrainerServiceOrders
            .Include(o => o.Trainer).ThenInclude(t => t.User)
            .Include(o => o.Student).ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order == null) return ApiResponse.Ok("Pedido local não encontrado.");

        order.ProviderPaymentId = payment.Id;
        order.ProviderPaymentStatus = payment.Status;
        order.UpdatedAt = _clock.UtcNow;

        if (payment.Status is "approved")
        {
            if (order.Status != TrainerServiceOrderStatus.Approved)
            {
                order.Status = TrainerServiceOrderStatus.Approved;
                order.PaidAt = payment.DateApproved ?? _clock.UtcNow;
                await LinkLeadOrStudentAfterApprovalAsync(order);
                await _notifications.CreateAsync(order.Trainer.UserId, "Nova venda aprovada", $"{order.BuyerName} comprou {order.ServiceTitleSnapshot}.", NotificationType.ServiceOfferPurchased, order.TrainerId, order.StudentId);
                if (order.RequiresManualStudentLinking)
                {
                    await _notifications.CreateAsync(order.Trainer.UserId, "Ação necessária na venda", $"Venda aprovada de {order.BuyerEmail}. Verifique vínculo com aluno.", NotificationType.ServiceOrderPendingAction, order.TrainerId, order.StudentId);
                }
                if (order.Student != null)
                {
                    await _notifications.CreateAsync(order.Student.UserId, "Pagamento aprovado", $"Seu pagamento para {order.ServiceTitleSnapshot} foi aprovado.", NotificationType.BuyerPaymentApproved, order.TrainerId, order.StudentId);
                }
            }
        }
        else if (payment.Status is "rejected")
            order.Status = TrainerServiceOrderStatus.Rejected;
        else if (payment.Status is "cancelled")
            order.Status = TrainerServiceOrderStatus.Cancelled;

        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok("Webhook B2C processado.");
    }

    private async Task<ApiResponse<ServiceOrderCreatedResponse>> CreateOrderCoreAsync(
        Guid trainerId,
        Guid offerId,
        string buyerName,
        string buyerEmail,
        string? buyerPhone,
        Guid? studentId)
    {
        var offer = await _db.TrainerServiceOffers
            .FirstOrDefaultAsync(o => o.Id == offerId && o.TrainerId == trainerId && o.IsActive && o.IsPublic && o.BillingType == TrainerServiceBillingType.OneTime);
        if (offer == null) return ApiResponse<ServiceOrderCreatedResponse>.Fail("Oferta indisponível.");

        var order = new TrainerServiceOrder
        {
            TrainerId = trainerId,
            ServiceOfferId = offer.Id,
            StudentId = studentId,
            BuyerName = buyerName.Trim(),
            BuyerEmail = buyerEmail.Trim().ToLowerInvariant(),
            BuyerPhone = buyerPhone?.Trim(),
            Amount = offer.Price,
            Status = TrainerServiceOrderStatus.PendingPayment,
            PaymentProvider = "MercadoPago",
            ServiceTitleSnapshot = offer.Title,
            ServiceDescriptionSnapshot = offer.Description,
            BillingTypeSnapshot = offer.BillingType,
            DurationDaysSnapshot = offer.DurationDays,
            ExpiresAt = _clock.UtcNow.AddDays(2)
        };
        _db.TrainerServiceOrders.Add(order);
        await _db.SaveChangesAsync();

        var preference = await CreateMercadoPagoPreferenceAsync(order);
        order.ProviderPreferenceId = preference.PreferenceId;
        order.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync();

        return ApiResponse<ServiceOrderCreatedResponse>.Ok(new ServiceOrderCreatedResponse
        {
            OrderId = order.Id,
            Status = order.Status.ToString(),
            CheckoutUrl = preference.InitPoint
        });
    }

    private async Task LinkLeadOrStudentAfterApprovalAsync(TrainerServiceOrder order)
    {
        if (!order.StudentId.HasValue)
        {
            var student = await _db.Students.Include(s => s.User)
                .FirstOrDefaultAsync(s => s.TrainerId == order.TrainerId && s.User.Email == order.BuyerEmail);
            if (student != null) order.StudentId = student.Id;
        }

        var lead = await _db.TrainerLeads
            .FirstOrDefaultAsync(l => l.TrainerId == order.TrainerId && l.Email == order.BuyerEmail);
        if (lead == null)
        {
            lead = new TrainerLead
            {
                TrainerId = order.TrainerId,
                Name = order.BuyerName,
                Email = order.BuyerEmail,
                Phone = order.BuyerPhone,
                Goal = "Compra de serviço",
                Message = $"Compra aprovada: {order.ServiceTitleSnapshot}",
                Source = TrainerLeadSource.DirectLink,
                Status = order.StudentId.HasValue ? TrainerLeadStatus.Converted : TrainerLeadStatus.Contacted
            };
            _db.TrainerLeads.Add(lead);
            await _db.SaveChangesAsync();
        }
        else
        {
            lead.Status = order.StudentId.HasValue ? TrainerLeadStatus.Converted : TrainerLeadStatus.Contacted;
            lead.UpdatedAt = _clock.UtcNow;
            await _db.SaveChangesAsync();
        }

        order.LeadId = lead.Id;

        var activeSub = await _db.TrainerSubscriptions
            .Include(s => s.PlatformPlan)
            .Where(s => s.TrainerId == order.TrainerId && s.Status == TrainerSubscriptionStatus.Active)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();
        var activeStudents = await _db.Students.CountAsync(s => s.TrainerId == order.TrainerId && s.Status == StudentStatus.Active);
        var canAutoLink = activeSub != null && activeStudents < activeSub.PlatformPlan.MaxActiveStudents;

        order.RequiresManualStudentLinking = !order.StudentId.HasValue || !canAutoLink;
        if (order.RequiresManualStudentLinking)
            order.InternalNotes = "Venda aprovada. Verificar conversão/vínculo manual do comprador.";
    }

    private async Task<(string PreferenceId, string InitPoint)> CreateMercadoPagoPreferenceAsync(TrainerServiceOrder order)
    {
        EnsureAccessToken();
        var successUrl = string.IsNullOrWhiteSpace(_mercadoPago.SuccessUrl) ? "https://example.com" : _mercadoPago.SuccessUrl;
        var failureUrl = string.IsNullOrWhiteSpace(_mercadoPago.FailureUrl) ? successUrl : _mercadoPago.FailureUrl;
        var pendingUrl = string.IsNullOrWhiteSpace(_mercadoPago.PendingUrl) ? successUrl : _mercadoPago.PendingUrl;

        var payload = new
        {
            items = new[]
            {
                new
                {
                    title = order.ServiceTitleSnapshot,
                    quantity = 1,
                    currency_id = "BRL",
                    unit_price = order.Amount
                }
            },
            payer = new { email = order.BuyerEmail, name = order.BuyerName },
            external_reference = order.Id.ToString(),
            notification_url = _mercadoPago.NotificationUrl,
            back_urls = new { success = successUrl, failure = failureUrl, pending = pendingUrl },
            auto_return = "approved"
        };

        var raw = await SendMercadoPagoAsync(HttpMethod.Post, "/checkout/preferences", payload);
        using var doc = JsonDocument.Parse(raw);
        var root = doc.RootElement;
        var preferenceId = root.TryGetProperty("id", out var idNode) ? idNode.GetString() : null;
        var initPoint = root.TryGetProperty("init_point", out var initNode) ? initNode.GetString() : null;
        if (string.IsNullOrWhiteSpace(preferenceId) || string.IsNullOrWhiteSpace(initPoint))
            throw new InvalidOperationException("Não foi possível gerar checkout para o pedido.");

        return (preferenceId, initPoint);
    }

    private async Task<MercadoPagoPaymentDetails?> GetMercadoPagoPaymentAsync(string paymentId, CancellationToken cancellationToken)
    {
        EnsureAccessToken();
        var raw = await SendMercadoPagoAsync(HttpMethod.Get, $"/v1/payments/{paymentId}", null, cancellationToken);
        using var doc = JsonDocument.Parse(raw);
        var root = doc.RootElement;
        var id = root.TryGetProperty("id", out var idNode) ? idNode.ToString() : null;
        var status = root.TryGetProperty("status", out var statusNode) ? statusNode.GetString() : null;
        var externalReference = root.TryGetProperty("external_reference", out var extNode) ? extNode.GetString() : null;
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(status)) return null;
        DateTime? approvedAt = null;
        if (root.TryGetProperty("date_approved", out var dateNode) && dateNode.ValueKind == JsonValueKind.String && DateTime.TryParse(dateNode.GetString(), out var dt))
            approvedAt = dt;

        return new MercadoPagoPaymentDetails
        {
            Id = id!,
            Status = status!,
            ExternalReference = externalReference,
            DateApproved = approvedAt
        };
    }

    private async Task<string> SendMercadoPagoAsync(HttpMethod method, string path, object? payload = null, CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        using var req = new HttpRequestMessage(method, $"https://api.mercadopago.com{path}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _mercadoPago.AccessToken);
        if (payload != null)
            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await client.SendAsync(req, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Falha Mercado Pago ({(int)response.StatusCode}).");
        return raw;
    }

    private async Task<Trainer?> ResolvePublicTrainerAsync(string slugOrId)
    {
        if (Guid.TryParse(slugOrId, out var trainerId))
            return await _db.Trainers.FirstOrDefaultAsync(t => t.Id == trainerId && t.PublicPageEnabled && t.AcceptingStudents);
        return await _db.Trainers.FirstOrDefaultAsync(t => t.PublicSlug == slugOrId && t.PublicPageEnabled && t.AcceptingStudents);
    }

    private void EnsureAccessToken()
    {
        if (string.IsNullOrWhiteSpace(_mercadoPago.AccessToken))
            throw new InvalidOperationException("MercadoPago:AccessToken não configurado.");
    }

    private static string? ExtractPayloadValue(JsonElement payload, string path)
    {
        try
        {
            var current = payload;
            foreach (var segment in path.Split('.'))
            {
                if (!current.TryGetProperty(segment, out current))
                    return null;
            }
            return current.ValueKind == JsonValueKind.String ? current.GetString() : current.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static TrainerServiceOfferResponse MapOffer(TrainerServiceOffer offer) => new()
    {
        Id = offer.Id,
        Title = offer.Title,
        Description = offer.Description,
        Price = offer.Price,
        BillingType = offer.BillingType.ToString(),
        DurationDays = offer.DurationDays,
        IsActive = offer.IsActive,
        IsPublic = offer.IsPublic,
        DisplayOrder = offer.DisplayOrder,
        CreatedAt = offer.CreatedAt
    };

    private static PublicServiceOfferResponse MapPublicOffer(TrainerServiceOffer offer) => new()
    {
        Id = offer.Id,
        Title = offer.Title,
        Description = offer.Description,
        Price = offer.Price,
        BillingType = offer.BillingType.ToString(),
        DurationDays = offer.DurationDays
    };

    private static TrainerServiceOrderResponse MapOrder(TrainerServiceOrder order) => new()
    {
        Id = order.Id,
        ServiceOfferId = order.ServiceOfferId,
        ServiceTitle = order.ServiceTitleSnapshot,
        BuyerName = order.BuyerName,
        BuyerEmail = order.BuyerEmail,
        BuyerPhone = order.BuyerPhone,
        Amount = order.Amount,
        Status = order.Status.ToString(),
        PaymentProvider = order.PaymentProvider,
        CreatedAt = order.CreatedAt,
        PaidAt = order.PaidAt,
        StudentId = order.StudentId,
        LeadId = order.LeadId,
        RequiresManualStudentLinking = order.RequiresManualStudentLinking
    };

    private sealed class MercadoPagoPaymentDetails
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ExternalReference { get; set; }
        public DateTime? DateApproved { get; set; }
    }
}
