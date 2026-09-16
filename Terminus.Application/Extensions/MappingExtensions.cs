using Terminus.Application.DTOs;
using Terminus.Domain.Entities;

namespace Terminus.Application.Extensions;

internal static class MappingExtensions
{
    public static ConsumerDto MapToDto(this Consumer consumer) =>
        new(consumer.Id, consumer.Name, consumer.Address, consumer.BankAccount);

    public static ProductDto MapToDto(this Product product) =>
        new(product.Id, product.Code, product.Name, product.Price, product.PriceListNumber);

    public static ContractDto MapToDto(this Contract contract) =>
        new(
            contract.Id, 
            contract.ContractNumber, 
            contract.ConsumerId, 
            contract.ConclusionDate,
            contract.Items.Select(i => new ContractItemDto(i.ProductId, i.Quantity)).ToList()
        );
    
    public static WaybillDto MapToDto(this Waybill waybill)
    {
        var (transportType, details, sum) = waybill switch
        {
            AutoWaybill auto => ("Автомобіль", $"Авто: {auto.CarNumber}, Лист: {auto.RouteSheetNumber}", auto.AutoServiceSum),
            TrainWaybill train => ("Залізниця", $"Вагон: {train.ContainerNumber}, Квитанція: {train.RailwayReceiptNumber}", train.TrainServiceSum),
            AviaWaybill avia => ("Авіа", $"Рейс: {avia.FlightNumber}, Квитанція: {avia.AviaReceiptNumber}", avia.AviaServiceSum),
            _ => ("Невідомо", "Невідомо", 0m)
        };

        return new WaybillDto(
            WaybillNumber: waybill.WaybillNumber,
            DispatchDate: waybill.DispatchDate,
            ConsumerName: waybill.Contract.Consumer.Name,
            TransportType: transportType,
            TransportDetails: details,
            ServiceSum: sum,
            Items: waybill.Items.Select(i => new WaybillItemDto(i.ProductId, i.ShippedQuantity)).ToList()
        );
    }
}