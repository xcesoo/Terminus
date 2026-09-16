using Terminus.Domain.Entities;

namespace Terminus.Domain.Interfaces.Repositories;

public interface IWaybillRepository
{
    //base crud operations
    Task<Waybill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Waybill>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Waybill>> SearchByNumberAsync(string searchTerm,
        CancellationToken cancellationToken = default);

    Task AddAsync(Waybill waybill, CancellationToken cancellationToken = default);
    Task DeleteAsync(Waybill waybill, CancellationToken cancellationToken = default);

    // Задача 2: Раздрукувати відомості відвантаження виробів за добу.
    // Витягує накладні за конкретну дату (включаючи інформацію про договір та виріб).
    Task<IReadOnlyCollection<Waybill>> GetWaybillsByDateAsync(DateTime dispatchDate, CancellationToken cancellationToken = default);

    // Задача 3: Раздрукувати відомості відвантаження виробів за місяць.
    // Витягує накладні за вказаний період (початок і кінець місяця).
    Task<IReadOnlyCollection<Waybill>> GetWaybillsByPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}