using FitPlatform.Application.DTOs.Auth;
using FitPlatform.Infrastructure.Services;
using Xunit;

namespace FitPlatform.Tests;

public class EncodingTests
{
    [Fact]
    public async Task RegistrationConsentMessagePreservesPortugueseAccents()
    {
        var service = new AuthService(null!, null!, null!);

        var result = await service.RegisterTrainerAsync(new RegisterTrainerRequest());

        Assert.False(result.Success);
        Assert.Equal("Você precisa aceitar os Termos de Uso e a Política de Privacidade.", result.Message);
    }
}
