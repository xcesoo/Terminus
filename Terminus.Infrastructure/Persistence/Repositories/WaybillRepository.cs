using Microsoft.EntityFrameworkCore;
using Terminus.Domain.Entities;
using Terminus.Domain.Enums;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Infrastructure.Persistence.Repositories;

public class WaybillRepository(TerminusDbContext dbContext) : IWaybillRepository
{
    public Task<Waybill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Waybills
            .Include(w => w.Items)
            .Include(w => w.Contract)
            .ThenInclude(c => c.Items)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Waybill>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Waybills
            .AsNoTracking()
            .Include(w => w.Items)
            .ThenInclude(i => i.Product)
            .Include(w => w.Contract)
            .ThenInclude(c => c.Consumer)
            .ToListAsync(cancellationToken);
    
    public async Task<IReadOnlyCollection<Waybill>> SearchByNumberAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await dbContext.Waybills
            .AsNoTracking()
            .Include(w => w.Items).ThenInclude(i => i.Product)
            .Include(w => w.Contract).ThenInclude(c => c.Consumer)
            .Where(w => EF.Functions.ILike(w.WaybillNumber, $"%{searchTerm}%") || 
                        EF.Functions.TrigramsAreSimilar(w.WaybillNumber, searchTerm))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Waybill waybill, CancellationToken cancellationToken = default) =>
        await dbContext.Waybills.AddAsync(waybill, cancellationToken);

    public async Task DeleteAsync(Waybill waybill, CancellationToken cancellationToken = default) =>
        await dbContext.Waybills.Where(w => w.Id == waybill.Id).ExecuteDeleteAsync(cancellationToken);

    // Задача 2: Раздрукувати відомості відвантаження виробів за добу
    public async Task<IReadOnlyCollection<Waybill>> GetWaybillsByDateAsync(DateTime dispatchDate, CancellationToken cancellationToken = default)
    {
        var startUtc = DateTime.SpecifyKind(dispatchDate.Date, DateTimeKind.Utc);
        var endUtc = startUtc.AddDays(1);

        return await dbContext.Waybills
            .AsNoTracking()
            .Where(w => w.Status == WaybillStatus.Dispatched 
                        && w.DispatchDate >= startUtc 
                        && w.DispatchDate < endUtc)
            .Include(w => w.Items).ThenInclude(i => i.Product)
            .Include(w => w.Contract).ThenInclude(c => c.Consumer)
            .ToListAsync(cancellationToken);
    }

    // Задача 3: Раздрукувати відомості відвантаження виробів за місяць
    public async Task<IReadOnlyCollection<Waybill>> GetWaybillsByPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var startUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc).AddDays(1);

        return await dbContext.Waybills
            .AsNoTracking()
            .Where(w => w.Status == WaybillStatus.Dispatched 
                        && w.DispatchDate >= startUtc 
                        && w.DispatchDate < endUtc)
            .Include(w => w.Items).ThenInclude(i => i.Product)
            .Include(w => w.Contract).ThenInclude(c => c.Consumer)
            .ToListAsync(cancellationToken);
    }
}