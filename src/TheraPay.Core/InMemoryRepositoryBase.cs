namespace TheraPay.Core;

using TheraPay.Domain;
public abstract class InMemoryRepositoryBase<TDomain> : IRepository<TDomain>
{
    protected readonly List<TDomain> Items = new();

    public virtual Result Add(TDomain entity)
    {
        Result exists = EntityExists(entity);
        if (exists.Ok)
            return new Result(false, exists.Error);

        Items.Add(entity);
        return new Result(true);
    }

    public int Count() => Items.Count;
    public TDomain GetByIndex(int index) => Items[index];
    public IReadOnlyList<TDomain> GetAll() => Items.ToList();
    public void Clear() => Items.Clear();

    protected abstract Result EntityExists(TDomain entity);
}