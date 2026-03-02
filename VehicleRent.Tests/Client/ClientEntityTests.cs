using VehicleRent.Models.Entities;

namespace VehicleRent.Tests;

public class ClientEntityTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesClient()
    {
        var client = new Client("Ana Silva", "ana@example.com", "+351912345678", "DL12345");

        Assert.Equal("Ana Silva", client.Name);
        Assert.Equal("ana@example.com", client.Email);
        Assert.Equal("+351912345678", client.PhoneNumber);
        Assert.Equal("DL12345", client.DriverLicense);
    }

    [Fact]
    public void Constructor_WithInvalidEmail_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Client("Ana", "invalid-email", "+351912345678", "DL12345"));
    }

    [Theory]
    [InlineData("351912345678")]
    [InlineData("+351912345")]
    [InlineData("+35191234567890")]
    [InlineData("+35191A345678")]
    [InlineData("+351-912345678")]
    public void Constructor_WithInvalidPhone_Throws(string phone)
    {
        Assert.Throws<ArgumentException>(() =>
            new Client("Ana", "ana@example.com", phone, "DL12345"));
    }

    [Fact]
    public void Constructor_WithEmptyDriverLicense_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Client("Ana", "ana@example.com", "+351912345678", ""));
    }

    [Fact]
    public void UpdateClient_WithValidData_UpdatesFields()
    {
        var client = new Client("Ana", "ana@example.com", "+351912345678", "DL123");

        client.UpdateClient("Ana Maria", "ana.maria@example.com", "+351987654321", "DL999");

        Assert.Equal("Ana Maria", client.Name);
        Assert.Equal("ana.maria@example.com", client.Email);
        Assert.Equal("+351987654321", client.PhoneNumber);
        Assert.Equal("DL999", client.DriverLicense);
    }
}
