using System.Reflection;
using Terminus.Domain.Entities;

namespace Terminus.Tests.TestHelpers;

internal static class TestFactory
{
    private static void SetProp(object target, string propName, object? value)
    {
        var type = target.GetType();
        PropertyInfo? prop = null;
        while (type != null && prop == null)
        {
            prop = type.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            type = type.BaseType;
        }

        prop!.SetValue(target, value);
    }

    public static Consumer CreateConsumer(
        string name = "ТОВ Тест",
        string address = "м. Київ, вул. Тестова, 1",
        string bankAccount = "UA000000000000000000000000000")
        => Consumer.Create(name, address, bankAccount);

    public static Product CreateProduct(
        string code = "P-001",
        string name = "Тестовий виріб",
        decimal price = 100m,
        string priceListNumber = "UA-2026")
        => Product.Create(code, name, price, priceListNumber);

    public static ContractItem CreateContractItem(Product product, int quantity)
    {
        var item = new ContractItem(product.Id, quantity);
        SetProp(item, nameof(ContractItem.Product), product);
        return item;
    }

    public static WaybillItem CreateWaybillItem(Product product, int shippedQuantity, decimal? price = null)
    {
        var item = new WaybillItem(product.Id, shippedQuantity, price ?? product.Price);
        SetProp(item, nameof(WaybillItem.Product), product);
        return item;
    }

    public static Contract CreateContract(Consumer consumer, IEnumerable<ContractItem> items, string number = "CTR-TEST-0001")
    {
        var contract = Contract.Create(number, consumer.Id, items);
        SetProp(contract, nameof(Contract.Consumer), consumer);
        return contract;
    }

    public static void LinkWaybillToContract(Waybill waybill, Contract contract)
    {
        SetProp(waybill, nameof(Waybill.Contract), contract);

        var field = typeof(Contract).GetField("_waybills", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var list = (List<Waybill>)field.GetValue(contract)!;
        list.Add(waybill);
    }
}
