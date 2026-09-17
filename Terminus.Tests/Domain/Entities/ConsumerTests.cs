using Terminus.Domain.Entities;

namespace Terminus.Tests.Domain.Entities;

public class ConsumerTests
{
    // Factory sets a generated Id and stores all three fields as given.
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var consumer = Consumer.Create("ТОВ Алло", "м. Дніпро, вул. Барикадна, 15", "UA893052990000026001234567006");

        Assert.NotEqual(Guid.Empty, consumer.Id);
        Assert.Equal("ТОВ Алло", consumer.Name);
        Assert.Equal("м. Дніпро, вул. Барикадна, 15", consumer.Address);
        Assert.Equal("UA893052990000026001234567006", consumer.BankAccount);
    }

    // Name, address and bank account are all mandatory — null/empty/whitespace in any of them must reject creation.
    [Theory]
    [InlineData(null, "address", "bank")]
    [InlineData("", "address", "bank")]
    [InlineData("   ", "address", "bank")]
    [InlineData("name", null, "bank")]
    [InlineData("name", "", "bank")]
    [InlineData("name", "address", null)]
    [InlineData("name", "address", "")]
    public void Create_WithMissingRequiredField_Throws(string? name, string? address, string? bankAccount)
    {
        // ArgumentException.ThrowIfNullOrWhiteSpace throws ArgumentNullException (a subclass) for null,
        // and plain ArgumentException for empty/whitespace — ThrowsAny accepts either.
        Assert.ThrowsAny<ArgumentException>(() => Consumer.Create(name!, address!, bankAccount!));
    }

    // Renaming a consumer updates the Name property in place.
    [Fact]
    public void ChangeName_WithValidName_UpdatesName()
    {
        var consumer = Consumer.Create("Old Name", "Address", "Bank");

        consumer.ChangeName("New Name");

        Assert.Equal("New Name", consumer.Name);
    }

    // A blank name must be rejected, not silently accepted.
    [Fact]
    public void ChangeName_WithEmptyName_Throws()
    {
        var consumer = Consumer.Create("Name", "Address", "Bank");

        Assert.Throws<ArgumentException>(() => consumer.ChangeName(" "));
    }

    // Changing the address updates the Address property in place.
    [Fact]
    public void ChangeAddress_WithValidAddress_UpdatesAddress()
    {
        var consumer = Consumer.Create("Name", "Old Address", "Bank");

        consumer.ChangeAddress("New Address");

        Assert.Equal("New Address", consumer.Address);
    }

    // A blank address must be rejected.
    [Fact]
    public void ChangeAddress_WithEmptyAddress_Throws()
    {
        var consumer = Consumer.Create("Name", "Address", "Bank");

        Assert.Throws<ArgumentException>(() => consumer.ChangeAddress(""));
    }

    // Changing the bank account updates the BankAccount property in place.
    [Fact]
    public void ChangeBankAccount_WithValidAccount_UpdatesAccount()
    {
        var consumer = Consumer.Create("Name", "Address", "OldBank");

        consumer.ChangeBankAccount("NewBank");

        Assert.Equal("NewBank", consumer.BankAccount);
    }

    // A null bank account must be rejected, not silently accepted.
    [Fact]
    public void ChangeBankAccount_WithEmptyAccount_Throws()
    {
        var consumer = Consumer.Create("Name", "Address", "Bank");

        Assert.ThrowsAny<ArgumentException>(() => consumer.ChangeBankAccount(null!));
    }
}
