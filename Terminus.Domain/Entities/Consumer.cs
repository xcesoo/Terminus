namespace Terminus.Domain.Entities;

public class Consumer
{
    public Guid Id { get; init; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public string BankAccount { get; private set; }
    
    private readonly List<Contract> _contracts = new();
    public IReadOnlyCollection<Contract> Contracts => _contracts.AsReadOnly();

    private Consumer() { } // for EF-core

    private Consumer(string name, string address, string bankAccount)
    {
        Id = Guid.CreateVersion7();
        Name = name;
        Address = address;
        BankAccount = bankAccount;
    }

    public static Consumer Create(string name, string address, string bankAccount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        ArgumentException.ThrowIfNullOrWhiteSpace(bankAccount);
        return new Consumer(name, address, bankAccount);
    }

    public void UpdateBankDetails(string newBankAccount, string newAddress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newBankAccount);
        ArgumentException.ThrowIfNullOrWhiteSpace(newAddress);
        BankAccount = newBankAccount;
        Address = newAddress;
    }
}