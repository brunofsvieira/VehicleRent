using VehicleRent.Services.Exceptions;

namespace VehicleRent.Tests;

public class BusinessValidationExceptionTests
{
    [Fact]
    public void Constructor_WithMessageOnly_UsesGenericValidationCode()
    {
        var ex = new BusinessValidationException("plain message");

        Assert.Equal(BusinessErrorCodes.GenericValidation, ex.ErrorCode);
        Assert.Equal("plain message", ex.Message);
    }

    [Fact]
    public void Constructor_WithCodeAndMessage_UsesProvidedCode()
    {
        var ex = new BusinessValidationException(BusinessErrorCodes.VehicleLicensePlateAlreadyExists, "duplicate");

        Assert.Equal(BusinessErrorCodes.VehicleLicensePlateAlreadyExists, ex.ErrorCode);
        Assert.Equal("duplicate", ex.Message);
    }
}
