using Terminus.Application.DTOs;

namespace Terminus.Application.Common.Rules.Interfaces;

public interface IShipmentStatementPdfGenerator
{
    byte[] Generate(IReadOnlyCollection<WaybillDto> waybills, string periodTitle);
}
