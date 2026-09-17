using Terminus.Application.DTOs;
using Terminus.Domain.Entities;
using Terminus.Domain.Enums;

namespace Terminus.Application.Extensions;

internal static class MappingExtensions
{
    public static ConsumerDto MapToDto(this Consumer consumer) =>
        new(consumer.Id, consumer.Name, consumer.Address, consumer.BankAccount);

    public static ProductDto MapToDto(this Product product) =>
        new(product.Id, product.Code, product.Name, product.Price, product.PriceListNumber);

    public static ContractDto MapToDto(this Contract contract)
    {
        var mappedItems = contract.Items.Select(i => new ContractItemDto(
            ProductId: i.ProductId,
            ProductName: i.Product.Name,
            Price: i.Product.Price,
            Quantity: i.Quantity,
            TotalPrice: i.Product.Price * i.Quantity
        )).ToList();

        return new ContractDto(
            Id: contract.Id, 
            ContractNumber: contract.ContractNumber, 
            ConsumerId: contract.ConsumerId, 
            Status: contract.Status.ToString(), 
            ConclusionDate: contract.ConclusionDate,
            Items: mappedItems,
            TotalAmount: mappedItems.Sum(i => i.TotalPrice)
        );
    }
    
    public static WaybillDto MapToDto(this Waybill waybill)
    {
        var (transportTypeCode, transportType, details, transportSum) = waybill switch
        {
            AutoWaybill auto => (TransportType.Auto, "Автомобіль", $"Авто: {auto.CarNumber}, Лист: {auto.RouteSheetNumber}", auto.AutoServiceSum),
            TrainWaybill train => (TransportType.Train, "Залізниця", $"Вагон: {train.ContainerNumber}, Квитанція: {train.RailwayReceiptNumber}", train.TrainServiceSum),
            AviaWaybill avia => (TransportType.Avia, "Авіа", $"Рейс: {avia.FlightNumber}, Квитанція: {avia.AviaReceiptNumber}", avia.AviaServiceSum),
            _ => throw new ArgumentOutOfRangeException(nameof(waybill), "Невідомий тип ТТН")
        };
        
        var mappedItems = waybill.Items.Select(i => new WaybillItemDto(
            ProductId: i.ProductId,
            ProductName: i.Product.Name,
            Price: i.Price,
            ShippedQuantity: i.ShippedQuantity,
            TotalPrice: i.Price * i.ShippedQuantity
        )).ToList();

        var productsTotalSum = mappedItems.Sum(i => i.TotalPrice);
        var totalAmount = productsTotalSum + transportSum; 

        return new WaybillDto(
            Id: waybill.Id,
            WaybillNumber: waybill.WaybillNumber,
            ContractNumber: waybill.Contract.ContractNumber,
            DispatchDate: waybill.DispatchDate,
            Status: waybill.Status,
            ConsumerName: waybill.Contract.Consumer.Name,
            ConsumerAddress: waybill.Contract.Consumer.Address,
            ConsumerBankAccount: waybill.Contract.Consumer.BankAccount,
            TransportTypeCode: transportTypeCode,
            TransportType: transportType,
            TransportDetails: details,
            Items: mappedItems,
            ProductsTotalSum: productsTotalSum,
            TransportServiceSum: transportSum,
            TotalAmount: totalAmount,
            DeliveryBaseCost: waybill.DeliveryBaseCost,
            DeliveryCommissionCost: waybill.DeliveryCommissionCost,
            DeliveryTransportMultiplier: waybill.DeliveryTransportMultiplier
        );
    }
}