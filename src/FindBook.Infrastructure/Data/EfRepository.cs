using System.Collections;
using System.Reflection;

namespace FindBook.Infrastructure.Data;

// inherit from Ardalis.Specification type
public class EfRepository<T>(AppDbContext dbContext) :
  RepositoryBase<T>(dbContext), IReadRepository<T>, IRepository<T> where T : class, IAggregateRoot
{
  private static readonly PropertyInfo? IdProperty = typeof(T).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

  public override async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
  {
    await AssignIdIfNeededAsync(entity, cancellationToken);
    return await base.AddAsync(entity, cancellationToken);
  }

  private async Task AssignIdIfNeededAsync(T entity, CancellationToken cancellationToken)
  {
    if (IdProperty is null)
    {
      return;
    }

    var idType = IdProperty.PropertyType;
    var fromMethod = idType.GetMethod("From", BindingFlags.Public | BindingFlags.Static, [typeof(int)]);
    var valueProperty = idType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);

    if (fromMethod is null || valueProperty is null)
    {
      return;
    }

    var currentValue = IdProperty.GetValue(entity);
    if (currentValue is not null)
    {
      try
      {
        var currentInt = (int)valueProperty.GetValue(currentValue)!;
        if (currentInt > 0)
        {
          return;
        }
      }
      catch
      {
      }
    }

    var maxId = 0;

    foreach (var entry in dbContext.ChangeTracker.Entries<T>()
      .Where(x => x.State != EntityState.Deleted && x.Entity is not null)
      .Select(x => x.Entity))
    {
      maxId = Math.Max(maxId, ReadId(entry, valueProperty));
    }

    var existingEntities = await dbContext.Set<T>().AsNoTracking().ToListAsync(cancellationToken);
    foreach (var existing in existingEntities)
    {
      maxId = Math.Max(maxId, ReadId(existing, valueProperty));
    }

    var nextId = fromMethod.Invoke(null, [maxId + 1]);
    IdProperty.SetValue(entity, nextId);
  }

  private static int ReadId(object entity, PropertyInfo valueProperty)
  {
    var idObject = IdProperty!.GetValue(entity);
    return idObject is null ? 0 : (int)valueProperty.GetValue(idObject)!;
  }
}
