using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Norma.Attributes;
using Norma.EF.Contexts;
using Norma.Model;
using Norma.Storage;

namespace Norma.EF.Interceptors
{
    public class EFAuditLogInterceptor : IDbCommandInterceptor, IDbCommandTreeInterceptor
    {
        private readonly string _nameOrConnectionString;


        public void TreeCreated(DbCommandTreeInterceptionContext interceptionContext)
        {
            var dbCommandTreeKind = interceptionContext.Result.CommandTreeKind;
            var context = interceptionContext.DbContexts.First();
            using (var auditContext = AuditLogStorage.Current.GetConnection())
            {

                switch (dbCommandTreeKind)
                {
                    case DbCommandTreeKind.Update:
                    case DbCommandTreeKind.Delete:
                        var entries = context.ChangeTracker.Entries().Where(
                            e => (e.State == EntityState.Deleted || e.State == EntityState.Modified)
                                 && e.IsAttr<AuditableAttribute>()).ToList();

                        foreach (var entry in entries)
                        {
                            ApplyAuditLog(auditContext, context,entry);
                        }
                        break;
                }
            }
        }

        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            if (command.CommandText.StartsWith("insert", StringComparison.InvariantCultureIgnoreCase))
            {
                var context = interceptionContext.DbContexts.First();
                var entries = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added && e.IsAttr<AuditableAttribute>()).ToList();

                using (var auditContext = AuditLogStorage.Current.GetConnection())
                {
                    foreach (var entry in entries)
                    {
                        ApplyAuditLog(auditContext,context,entry,LogOperation.Create);
                    }
                }
            }
        }

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {

        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
        }

        private void ApplyAuditLog(IStorageConnection auditContext, DbContext workingContext, DbEntityEntry entry)
        {
            LogOperation operation;
            switch (entry.State)
            {
                case EntityState.Added:
                    operation = LogOperation.Create;
                    break;
                case EntityState.Deleted:
                    operation = LogOperation.Delete;
                    break;
                case EntityState.Modified:
                    operation = LogOperation.Update;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ApplyAuditLog(auditContext, workingContext, entry, operation);
        }

        public void ApplyAuditLog(IStorageConnection auditContext, DbContext workingContext, DbEntityEntry entry, LogOperation logOperation)
        {
            var currentPrincipal = Thread.CurrentPrincipal;
            var user = currentPrincipal != null ? (currentPrincipal.Identity).Name : string.Empty;
            var includedProperties = new List<string>();
            var entityKey = workingContext.GetEntityKey(entry.Entity).GetEntityString();
            var entityType = entry.Entity.GetType();

            if (entry.IsAttr<AuditableAttribute>())
            {
                var props = entityType.GetProperties().Where(pi => !pi.IsAttr<NotAuditableAttribute>());
                includedProperties.AddRange(props.Select(pi => pi.Name));
            }
            else
            {
                var props = entityType.GetProperties()
                    .Where(p => p.IsAttr<AuditableAttribute>() && !p.IsAttr<NotAuditableAttribute>());

                includedProperties.AddRange(props.Select(pi => pi.Name));
            }

            if (entry.State == EntityState.Modified)
            {
                var originalValues = workingContext.Entry(entry.Entity).GetDatabaseValues();
                var changedProperties = (from propertyName in originalValues.PropertyNames
                                         let propertyEntry = entry.Property(propertyName)
                                         let currentValue = propertyEntry.CurrentValue
                                         let originalValue = originalValues[propertyName]
                                         where (!Equals(currentValue, originalValue) && includedProperties.Contains(propertyName))
                                         select new ChangedProperty
                                         {
                                             Name = propertyName,
                                             CurrentValue = currentValue,
                                             OriginalValue = originalValue
                                         }).ToArray();

                if (changedProperties.Any())
                {
                    var auditDateTime = AuditLogStorage.Current.AuditDateTime;
                    var id = auditContext.AddAuditLog(entityKey, entityType.FullName,user,auditDateTime);
                    using (var trans = auditContext.CreateWriteOnlyTransaction())
                    {
                        foreach (var log in changedProperties.Select(changedProperty => new AuditLogChange
                        {
                            Created = auditDateTime,
                            AuditLogId = id,
                            Operation = logOperation,
                            OldValue = changedProperty.OriginalValue.ToString(),
                            NewValue = changedProperty.CurrentValue.ToString(),
                            PropertyName = changedProperty.Name,
                            User = user
                        }))
                        {
                            trans.AddToAuditLogChange(log);
                        }
                        trans.Commit();
                    }


                }
            }
            else
            {
                var loggedProperties = (from propertyName in entry.CurrentValues.PropertyNames
                                        let propertyEntry = entry.Property(propertyName)
                                        let currentValue = propertyEntry.CurrentValue
                                        where includedProperties.Contains(propertyName)
                                        select new ChangedProperty
                                        {
                                            Name = propertyName,
                                            CurrentValue = currentValue,
                                            OriginalValue = null
                                        }).ToArray();
                if (loggedProperties.Any())
                {
                    var auditDateTime = AuditLogStorage.Current.AuditDateTime;
                    var id = auditContext.AddAuditLog(entityKey, entityType.FullName,user,auditDateTime);
                    using (var trans = auditContext.CreateWriteOnlyTransaction())
                    {
                        foreach (var log in loggedProperties.Select(loggedProperty => new AuditLogChange
                        {
                            Created = auditDateTime,
                            AuditLogId = id,
                            Operation = logOperation,
                            OldValue = null,
                            NewValue = loggedProperty.CurrentValue.ToString(),
                            PropertyName = loggedProperty.Name,
                            User = user
                        }))
                        {
                          trans.AddToAuditLogChange(log);
                        }
                        trans.Commit();
                    }
                }
                
            }
        }
      
    }

    internal static class Utils
    {
        #region ReflectionExt

        public static bool IsAttr<T>(this PropertyInfo entry) where T : Attribute
        {
            return entry.CustomAttributes.Any(q => q.AttributeType == typeof (T));
        }

        public static bool IsAttr<T>(this DbEntityEntry entry) where T : Attribute
        {
            var entity = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(entry.Entity.GetType());
            return entity.CustomAttributes.Any(q => q.AttributeType == typeof (T));
        }

        public static byte[] Serialize<T>(T entity) where T : class
        {
            var bf = new BinaryFormatter();

            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, entity);
                return ms.GetBuffer();
            }

        }

        public static T Deserialize<T>(byte[] arrBytes) where T : class
        {
            var bf = new BinaryFormatter();
            T result;

            using (var ms = new MemoryStream())
            {
                ms.Write(arrBytes, 0, arrBytes.Length);
                ms.Position = 0;
                result = bf.Deserialize(ms) as T;
            }
            return result;
        }

        #endregion
    }
}