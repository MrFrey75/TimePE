# Soft Delete Implementation

## Overview

TimePE uses **DevExpress XPO's built-in soft delete system** for data deletion. This approach ensures deleted records are marked as deleted rather than physically removed from the database, allowing for potential recovery and maintaining data integrity.

## Implementation Details

### Base Entity Configuration

```csharp
[DeferredDeletion(true)] // Enable XPO's built-in soft delete
public abstract class BaseEntity : XPObject
{
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### How It Works

1. **Automatic Filtering**: When `[DeferredDeletion(true)]` is set, XPO automatically:
   - Sets an internal `GCRecord` property when `.Delete()` is called
   - Filters out deleted records from all queries automatically
   - Maintains referential integrity

2. **Deletion Process**:
   ```csharp
   // In services
   var entity = uow.GetObjectByKey<Entity>(id);
   entity.Delete(); // XPO handles the soft delete internally
   uow.CommitChanges();
   ```

3. **Query Behavior**:
   ```csharp
   // This automatically excludes soft-deleted records
   var activeRecords = new XPQuery<Entity>(session).ToList();
   
   // GetObjectByKey returns null for soft-deleted objects
   var entity = session.GetObjectByKey<Entity>(id); // null if deleted
   ```

## Benefits

✅ **Automatic**: XPO handles filtering deleted records automatically  
✅ **Consistent**: All queries respect the soft delete state by default  
✅ **Integrated**: Works seamlessly with XPO's session/UnitOfWork lifecycle  
✅ **Recoverable**: Deleted data can be restored if needed (via direct database access)  
✅ **Performant**: No manual filtering required in queries  

## Important Considerations

### includeDeleted Parameter

Several service methods have an `includeDeleted` parameter that is currently **not functional** with XPO's automatic filtering:

```csharp
Task<IEnumerable<Project>> GetAllProjectsAsync(bool includeDeleted = false);
```

**Note**: XPO's `[DeferredDeletion(true)]` automatically filters deleted records. To include deleted records would require:
- Using a separate session configuration, or
- Querying the `GCRecord` table directly, or
- Switching to manual soft delete (not recommended)

For now, these parameters are preserved for API compatibility but don't affect the results.

### Migration from Custom Soft Delete

Previous implementation used a custom `DeletedAt` property. The migration to XPO's built-in system involved:

1. Removing `DeletedAt` property from `BaseEntity`
2. Changing `[DeferredDeletion(false)]` to `[DeferredDeletion(true)]`
3. Removing all `DeletedAt == null` filters from queries
4. Updating delete methods to use `entity.Delete()` instead of `entity.DeletedAt = DateTime.UtcNow`

### Database Schema

XPO creates a `XPObjectType` table and uses it along with internal mechanisms to track deleted objects. The `GCRecord` property links to this table.

## Examples

### Deleting an Entity

```csharp
public async Task DeleteProjectAsync(int id)
{
    await Task.Run(() =>
    {
        using var uow = new UnitOfWork(XpoDefault.DataLayer);
        var project = uow.GetObjectByKey<Project>(id);
        if (project != null)
        {
            project.Delete(); // XPO's built-in soft delete
            uow.CommitChanges();
        }
    });
}
```

### Querying Active Entities

```csharp
public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
{
    return await Task.Run(() =>
    {
        using var session = new Session(XpoDefault.DataLayer);
        // Soft-deleted records automatically excluded
        var criteria = CriteriaOperator.Parse("IsActive = True");
        return new XPCollection<Project>(session, criteria).ToList();
    });
}
```

### Checking if Entity Exists

```csharp
// Returns null for soft-deleted entities
var project = session.GetObjectByKey<Project>(id);
if (project == null) 
{
    // Entity doesn't exist OR has been soft-deleted
}
```

## Best Practices

1. **Always use `entity.Delete()`** instead of manually setting flags
2. **Trust XPO's filtering** - don't add manual `GCRecord != null` checks
3. **Use `GetObjectByKey`** to check existence - it respects soft delete state
4. **Document behavior** when `includeDeleted` parameters aren't functional
5. **Test thoroughly** after any changes to ensure deleted records stay hidden

## References

- [DevExpress XPO Deferred Deletion](https://docs.devexpress.com/XPO/2035/concepts/deferred-and-immediate-object-deletion)
- `BaseEntity.cs` - Base class with soft delete configuration
- All service classes in `TimePE.Core/Services/` - Implementation examples
