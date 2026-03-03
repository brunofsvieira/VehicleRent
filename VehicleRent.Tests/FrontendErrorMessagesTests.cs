using VehicleRent.Infrastructure;
using VehicleRent.Services.Exceptions;

namespace VehicleRent.Tests;

public class FrontendErrorMessagesTests
{
    [Fact]
    public void ToPt_KnownCode_ReturnsSpecificPortugueseMessage()
    {
        var message = FrontendErrorMessages.ToPt(BusinessErrorCodes.ClientEmailAlreadyExists);

        Assert.Equal("Já existe um cliente com este email.", message);
    }

    [Fact]
    public void ToPt_UnknownCode_ReturnsFallbackMessage()
    {
        var message = FrontendErrorMessages.ToPt("some.unknown.code");

        Assert.Equal("Não foi possivel concluir a operação. Verifique os dados e tente novamente.", message);
    }
}
