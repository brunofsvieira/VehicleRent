using System.ComponentModel.DataAnnotations;
using VehicleRent.Models.Enumerators;
using VehicleRent.Models.Validation;

namespace VehicleRent.Tests;

/// <summary>
/// Represents unit tests for ValidationAttributesTests.
/// </summary>
public class ValidationAttributesTests
{
    [Fact]
    /// <summary>
    /// Executes the ManufacturingYearRange_WithValidYear_ReturnsSuccess test operation.
    /// </summary>
    public void ManufacturingYearRange_WithValidYear_ReturnsSuccess()
    {
        var attr = new ManufacturingYearRangeAttribute();
        var context = new ValidationContext(new object());

        var result = attr.GetValidationResult(DateTime.Now.Year, context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    /// <summary>
    /// Executes the ManufacturingYearRange_WithInvalidYear_ReturnsError test operation.
    /// </summary>
    public void ManufacturingYearRange_WithInvalidYear_ReturnsError()
    {
        var attr = new ManufacturingYearRangeAttribute();
        var context = new ValidationContext(new object());

        var result = attr.GetValidationResult(1800, context);

        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    /// <summary>
    /// Executes the EnsureFuelNotNone_WithNoneEnum_ReturnsError test operation.
    /// </summary>
    public void EnsureFuelNotNone_WithNoneEnum_ReturnsError()
    {
        var attr = new EnsureFuelNotNoneAttribute();
        var context = new ValidationContext(new object());

        var result = attr.GetValidationResult(FuelType.None, context);

        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    /// <summary>
    /// Executes the EnsureFuelNotNone_WithValidEnum_ReturnsSuccess test operation.
    /// </summary>
    public void EnsureFuelNotNone_WithValidEnum_ReturnsSuccess()
    {
        var attr = new EnsureFuelNotNoneAttribute();
        var context = new ValidationContext(new object());

        var result = attr.GetValidationResult(FuelType.Diesel, context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    /// <summary>
    /// Executes the EnsureFuelNotNone_WithStringNone_ReturnsError test operation.
    /// </summary>
    public void EnsureFuelNotNone_WithStringNone_ReturnsError()
    {
        var attr = new EnsureFuelNotNoneAttribute();
        var context = new ValidationContext(new object());

        var result = attr.GetValidationResult("None", context);

        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("11-AA-11")]
    [InlineData("AA-11-11")]
    [InlineData("11-11-AA")]
    [InlineData("AA-11-AA")]
    /// <summary>
    /// Executes the LicensePlateFormat_WithValidPatterns_ReturnsSuccess test operation.
    /// </summary>
    public void LicensePlateFormat_WithValidPatterns_ReturnsSuccess(string plate)
    {
        var attr = new LicensePlateFormatAttribute();
        var context = new ValidationContext(new object());

        var result = attr.GetValidationResult(plate, context);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("AAA-11-11")]
    [InlineData("11-AAA-11")]
    [InlineData("11-AA-111")]
    [InlineData("A1-11-AA")]
    [InlineData("11AA11")]
    /// <summary>
    /// Executes the LicensePlateFormat_WithInvalidPatterns_ReturnsError test operation.
    /// </summary>
    public void LicensePlateFormat_WithInvalidPatterns_ReturnsError(string plate)
    {
        var attr = new LicensePlateFormatAttribute();
        var context = new ValidationContext(new object());

        var result = attr.GetValidationResult(plate, context);

        Assert.NotEqual(ValidationResult.Success, result);
    }
}
