using VehicleRent.Infrastructure;
using VehicleRent.Services.Exceptions;

namespace VehicleRent.Tests;

/// <summary>
/// Represents unit tests for FrontendErrorMessagesTests.
/// </summary>
public class FrontendErrorMessagesTests
{
    [Fact]
    /// <summary>
    /// Executes the ToPt_KnownCode_ReturnsSpecificPortugueseMessage test operation.
    /// </summary>
    public void ToPt_KnownCode_ReturnsSpecificPortugueseMessage()
    {
        var message = FrontendErrorMessages.ToPt(BusinessErrorCodes.ClientEmailAlreadyExists);

        Assert.Equal("Já existe um cliente com este email.", message);
    }

    [Fact]
    /// <summary>
    /// Executes the ToPt_UnknownCode_ReturnsFallbackMessage test operation.
    /// </summary>
    public void ToPt_UnknownCode_ReturnsFallbackMessage()
    {
        var message = FrontendErrorMessages.ToPt("some.unknown.code");

        Assert.Equal("Não foi possivel concluir a operação. Verifique os dados e tente novamente.", message);
    }
}
