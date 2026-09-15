using Microsoft.EntityFrameworkCore;
using Terminus.Domain.Entities;
using Terminus.Domain.Enums;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Infrastructure.Persistence.Repositories;

public class WaybillRepository(TerminusDbContext dbContext) : IWaybillRepository
{
    public Task<Waybill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Waybills.Include(w => w.Contract).FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Waybill>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Waybills.AsNoTracking().Include(w=>w.Contract).ThenInclude(c=>c.Product).ToListAsync(cancellationToken);

    public async Task AddAsync(Waybill waybill, CancellationToken cancellationToken = default) =>
        await dbContext.Waybills.AddAsync(waybill, cancellationToken);

    public async Task DeleteAsync(Waybill waybill, CancellationToken cancellationToken = default) =>
        await dbContext.Waybills.Where(w => w.Id == waybill.Id).ExecuteDeleteAsync(cancellationToken);

    // Задача 2: Раздрукувати відомості відвантаження виробів за добу
    public async Task<IReadOnlyCollection<Waybill>> GetWaybillsByDateAsync(DateTime dispatchDate, CancellationToken cancellationToken = default) =>
        await dbContext.Waybills
            .AsNoTracking()
            .Where(w => w.Status == WaybillStatus.Dispatched 
                        && w.DispatchDate >= dispatchDate.Date 
                        && w.DispatchDate < dispatchDate.Date.AddDays(1))
            .Include(w => w.Contract)
            .ThenInclude(c => c.Product)
            .ToListAsync(cancellationToken);

    // Задача 3: Раздрукувати відомості відвантаження виробів за місяць
    public async Task<IReadOnlyCollection<Waybill>> GetWaybillsByPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default) =>
        await dbContext.Waybills
            .AsNoTracking()
            .Where(w => w.Status == WaybillStatus.Dispatched 
                        && w.DispatchDate >= startDate.Date 
                        && w.DispatchDate < endDate.Date.AddDays(1))
            .Include(w => w.Contract)
            .ThenInclude(c => c.Product)
            .ToListAsync(cancellationToken);
}