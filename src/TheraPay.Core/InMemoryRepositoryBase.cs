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
    public TDomain GetById(object id)
    {
        ArgumentNullException.ThrowIfNull(id);

        var index = GetIndexById(id);
        if (index < 0)
            throw new KeyNotFoundException($"Entity with ID '{id}' not found.");

        return Items[index];
    }

    public int GetIndexById(object id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Items.FindIndex(entity => Equals(GetEntityId(entity), id));
    }

    public IReadOnlyList<TDomain> GetAll() => Items.ToList();
    public void Clear() => Items.Clear();

    protected abstract object GetEntityId(TDomain entity);
    protected abstract Result EntityExists(TDomain entity);
}
