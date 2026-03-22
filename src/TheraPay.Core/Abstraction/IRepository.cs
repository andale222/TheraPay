using TheraPay.Domain;

namespace TheraPay.Core;

public interface IRepository<TDomain>
{
    Result Add(TDomain domainObject);
    int Count();
    TDomain GetByIndex(int index);
    IReadOnlyList<TDomain> GetAll();
    void Clear();
}