using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Subscription;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

/// <summary>
/// Gestão da assinatura do próprio trainer (cancelamento e troca de plano).
///
/// Segurança: todas as rotas exigem JWT com papel Trainer e operam exclusivamente sobre
/// o TrainerId extraído do token. O identificador do trainer nunca é aceito pelo corpo
/// ou pela rota, de modo que não há como agir sobre a assinatura de terceiros.
///
/// Controller thin: nenhuma regra de negócio aqui — tudo delegado ao <see cref="PaymentService"/>.
/// </summary>
[ApiController]
[Route("api/subscriptions")]
[Authorize(Roles = "Trainer")]
public class SubscriptionsController : ControllerBase
{
    private readonly PaymentService _service;
    private readonly ICurrentUserService _currentUser;

    public SubscriptionsController(PaymentService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Cancela a assinatura vigente do trainer autenticado.
    /// O acesso permanece liberado até o fim do período já pago.
    /// Idempotente: cancelar duas vezes retorna 200 com AlreadyCanceled = true.
    /// </summary>
    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel(CancellationToken cancellationToken)
    {
        if (!_currentUser.TrainerId.HasValue)
            return Unauthorized(ApiResponse.Fail("Trainer não identificado no token."));

        var result = await _service.CancelSubscriptionAsync(_currentUser.TrainerId.Value, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Solicita a troca de plano/ciclo da assinatura vigente do trainer autenticado.
    /// Recebe apenas o identificador do plano (e opcionalmente o ciclo) — o preço é
    /// sempre resolvido pelo backend.
    /// A troca é aplicada pelo provedor no início do próximo ciclo de cobrança.
    /// </summary>
    [HttpPost("change-plan")]
    public async Task<IActionResult> ChangePlan(
        [FromBody] ChangeSubscriptionPlanRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUser.TrainerId.HasValue)
            return Unauthorized(ApiResponse.Fail("Trainer não identificado no token."));

        if (request.PlatformPlanId == Guid.Empty)
            return BadRequest(ApiResponse.Fail("Informe o plano de destino."));

        var result = await _service.ChangePlanAsync(_currentUser.TrainerId.Value, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
