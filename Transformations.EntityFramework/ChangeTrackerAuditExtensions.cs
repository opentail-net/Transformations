using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Transformations.EntityFramework
{
    /// <summary>
    /// Represents a single property change captured from the EF Core ChangeTracker.
    /// </summary>
    public sealed class AuditEntry
    {
        /// <summary>
        /// The CLR type name of the entity that changed.
        /// </summary>
        public string EntityType { get; init; } = string.Empty;

        /// <summary>
        /// The entity state at capture time (Added, Modified, Deleted).
        /// </summary>
        public EntityState State { get; init; }

        /// <summary>
        /// The property that changed. Null for Added/Deleted entries that represent the whole entity.
        /// </summary>
        public string? PropertyName { get; init; }

        /// <summary>
        /// The original value before the change. Null for Added entities.
        /// </summary>
        public object? OriginalValue { get; init; }

        /// <summary>
        /// The current value after the change. Null for Deleted entities.
        /// </summary>
        public object? CurrentValue { get; init; }

        /// <summary>
        /// The primary key values of the entity, formatted as a string.
        /// </summary>
        public string KeyValues { get; init; } = string.Empty;

        /// <summary>
        /// The UTC timestamp when the audit entry was captured.
        /// </summary>
        public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Extracts structured audit entries from the EF Core <see cref="ChangeTracker"/>
    /// for entities that have been Added, Modified, or Deleted.
    /// </summary>
    /// <remarks>
    /// Call <see cref="GetAuditEntries"/> <strong>before</strong> <c>SaveChangesAsync</c> to capture
    /// pending changes. After save, the ChangeTracker resets entity states.
    /// </remarks>
    public static class ChangeTrackerAuditExtensions
    {
        /// <summary>
        /// Captures audit entries for all pending Added, Modified, and Deleted entities.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/> whose ChangeTracker to inspect.</param>
        /// <returns>A list of <see cref="AuditEntry"/> records describing each change.</returns>
        public static IReadOnlyList<AuditEntry> GetAuditEntries(this DbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return GetAuditEntries(context, EntityState.Added | EntityState.Modified | EntityState.Deleted);
        }

        /// <summary>
        /// Captures audit entries for pending changes matching the specified entity states.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/> whose ChangeTracker to inspect.</param>
        /// <param name="states">The entity states to capture (e.g., <c>EntityState.Modified</c>).</param>
        /// <returns>A list of <see cref="AuditEntry"/> records describing each change.</returns>
        public static IReadOnlyList<AuditEntry> GetAuditEntries(this DbContext context, EntityState states)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var entries = new List<AuditEntry>();
            DateTime timestamp = DateTime.UtcNow;

            foreach (EntityEntry entityEntry in context.ChangeTracker.Entries())
            {
                if ((entityEntry.State & states) == 0)
                {
                    continue;
                }

                string entityType = entityEntry.Entity.GetType().Name;
                string keyValues = GetKeyValues(entityEntry);

                switch (entityEntry.State)
                {
                    case EntityState.Added:
                        foreach (PropertyEntry prop in entityEntry.Properties)
                        {
                            entries.Add(new AuditEntry
                            {
                                EntityType = entityType,
                                State = EntityState.Added,
                                PropertyName = prop.Metadata.Name,
                                OriginalValue = null,
                                CurrentValue = prop.CurrentValue,
                                KeyValues = keyValues,
                                TimestampUtc = timestamp
                            });
                        }
                        break;

                    case EntityState.Deleted:
                        foreach (PropertyEntry prop in entityEntry.Properties)
                        {
                            entries.Add(new AuditEntry
                            {
                                EntityType = entityType,
                                State = EntityState.Deleted,
                                PropertyName = prop.Metadata.Name,
                                OriginalValue = prop.OriginalValue,
                                CurrentValue = null,
                                KeyValues = keyValues,
                                TimestampUtc = timestamp
                            });
                        }
                        break;

                    case EntityState.Modified:
                        foreach (PropertyEntry prop in entityEntry.Properties.Where(p => p.IsModified))
                        {
                            entries.Add(new AuditEntry
                            {
                                EntityType = entityType,
                                State = EntityState.Modified,
                                PropertyName = prop.Metadata.Name,
                                OriginalValue = prop.OriginalValue,
                                CurrentValue = prop.CurrentValue,
                                KeyValues = keyValues,
                                TimestampUtc = timestamp
                            });
                        }
                        break;
                }
            }

            return entries;
        }

        private static string GetKeyValues(EntityEntry entry)
        {
            var keyProperties = entry.Metadata.FindPrimaryKey()?.Properties;
            if (keyProperties == null || keyProperties.Count == 0)
            {
                return string.Empty;
            }

            if (keyProperties.Count == 1)
            {
                return entry.Property(keyProperties[0].Name).CurrentValue?.ToString() ?? string.Empty;
            }

            return string.Join(", ", keyProperties.Select(
                k => $"{k.Name}={entry.Property(k.Name).CurrentValue}"));
        }
    }
}
