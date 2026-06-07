using TheraPay.Domain;

namespace TheraPay.Core;

public interface IRepository<TDomain>
{
    Result Add(TDomain domainObject);
    int Count();
    TDomain GetByIndex(int index);
    TDomain GetById(object id);
    int GetIndexById(object id);
    IReadOnlyList<TDomain> GetAll();
    Result RemoveById(object id);
    void Clear();
}
