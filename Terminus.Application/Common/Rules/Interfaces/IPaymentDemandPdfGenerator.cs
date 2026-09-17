using Terminus.Application.DTOs;

namespace Terminus.Application.Common.Rules.Interfaces;

public interface IPaymentDemandPdfGenerator
{
    byte[] Generate(WaybillDto waybill);
}