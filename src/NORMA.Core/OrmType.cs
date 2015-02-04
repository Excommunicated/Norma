using System;
using System.Collections.Generic;

namespace Norma
{
    public abstract class AuditLogOrmType
    {
        private static readonly object LockObject = new object();
        private static AuditLogOrmType _current;

        public static AuditLogOrmType Current
        {
            get
            {
                lock (LockObject)
                {
                    if (_current == null)
                    {
                        throw new InvalidOperationException("OrmType.Current property value has not been initialized. You must set it before using Norma");
                    }
                    return _current;
                }
            }
            set
            {
                lock (LockObject)
                {
                    _current = value;
                }
            }
        }

        public abstract AuditableEntityModelConfiguration AuditableEntityModelConfiguration { get; set; }
    }

    public class AuditableEntityModelConfiguration
    {
        private readonly Dictionary<Type, EntityModelConfiguration> configurations = new Dictionary<Type, EntityModelConfiguration>();

        internal virtual EntityModelConfiguration Entity(Type entityType)
        {
            EntityModelConfiguration entityModelConfiguration;
            if (!configurations.TryGetValue(entityType, out entityModelConfiguration))
            {
                configurations.Add(entityType,entityModelConfiguration = new EntityModelConfiguration(entityType));
            }
            return entityModelConfiguration;
        }
    }

    public class EntityModelConfiguration<T>
    {
        private readonly EntityModelConfiguration _entityModelConfiguration;

        public EntityModelConfiguration() : this(new EntityModelConfiguration(typeof(T)))
        {
            
        }

        internal EntityModelConfiguration(EntityModelConfiguration entityModelConfiguration)
        {
            _entityModelConfiguration = entityModelConfiguration;
        }

        public void IsAuditable()
        {
            
        }
    }

    public class EntityModelConfiguration
    {
        private readonly Type _type;

        public EntityModelConfiguration(Type type)
        {
            _type = type;
        }
    }

    public class AuditableEntityModelBuilder
    {
        public AuditableEntityModelBuilder()
        {
            modelConfiguration = new AuditableEntityModelConfiguration();
        }
        private readonly AuditableEntityModelConfiguration modelConfiguration;
        public EntityModelConfiguration<T> Entity<T>()
        {
            return new EntityModelConfiguration<T>(this.modelConfiguration.Entity(typeof(T)));
        }

        public AuditableEntityModelConfiguration ModelConfiguration { get { return modelConfiguration; } }
    }
}