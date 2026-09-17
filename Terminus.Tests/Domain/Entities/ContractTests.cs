using Terminus.Domain.Entities;
using Terminus.Domain.Enums;

namespace Terminus.Tests.Domain.Entities;

public class ContractTests
{
    private static Contract CreateDraftContract()
    {
        var consumerId = Guid.NewGuid();
        var items = new[] { new ContractItem(Guid.NewGuid(), 5) };
        return Contract.Create("CTR-0001", consumerId, items);
    }

    // A freshly created contract starts in Draft with no conclusion date and carries its items.
    [Fact]
    public void Create_SetsDefaultStatusToDraft()
    {
        var contract = CreateDraftContract();

        Assert.Equal(ContractStatus.Draft, contract.Status);
        Assert.Null(contract.ConclusionDate);
        Assert.Single(contract.Items);
    }

    // Signing a Draft contract moves it to Signed and records the conclusion date.
    [Fact]
    public void Sign_FromDraft_SetsSignedStatusAndConclusionDate()
    {
        var contract = CreateDraftContract();
        var date = new DateTime(2026, 9, 17, 0, 0, 0, DateTimeKind.Utc);

        contract.Sign(date);

        Assert.Equal(ContractStatus.Signed, contract.Status);
        Assert.Equal(date, contract.ConclusionDate);
    }

    // A contract cannot be signed twice.
    [Fact]
    public void Sign_WhenAlreadySigned_Throws()
    {
        var contract = CreateDraftContract();
        contract.Sign(DateTime.UtcNow);

        Assert.Throws<InvalidOperationException>(() => contract.Sign(DateTime.UtcNow));
    }

    // A terminated contract can no longer be signed.
    [Fact]
    public void Sign_WhenTerminated_Throws()
    {
        var contract = CreateDraftContract();
        contract.Sign(DateTime.UtcNow);
        contract.Terminate();

        Assert.Throws<InvalidOperationException>(() => contract.Sign(DateTime.UtcNow));
    }

    // Terminating a Signed contract moves it to Terminated.
    [Fact]
    public void Terminate_FromSigned_SetsTerminatedStatus()
    {
        var contract = CreateDraftContract();
        contract.Sign(DateTime.UtcNow);

        contract.Terminate();

        Assert.Equal(ContractStatus.Terminated, contract.Status);
    }

    // Terminating is also allowed straight from Draft (voiding a contract that was never signed) — confirmed intentional.
    [Fact]
    public void Terminate_FromDraft_SetsTerminatedStatus()
    {
        var contract = CreateDraftContract();

        contract.Terminate();

        Assert.Equal(ContractStatus.Terminated, contract.Status);
    }

    // A contract cannot be terminated twice.
    [Fact]
    public void Terminate_WhenAlreadyTerminated_Throws()
    {
        var contract = CreateDraftContract();
        contract.Terminate();

        Assert.Throws<InvalidOperationException>(() => contract.Terminate());
    }
}
