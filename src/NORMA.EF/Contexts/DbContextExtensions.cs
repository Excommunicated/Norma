using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norma.EF.Contexts
{
    public static class DbContextExtensions
    {
        private const string KeySeparator = "►";

        public static EntityKey GetEntityKey<T>(this IObjectContextAdapter context, T entity) where T : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            var oc = context.ObjectContext;
            ObjectStateEntry ose;
            oc.AcceptAllChanges();
            return oc.ObjectStateManager.TryGetObjectStateEntry(entity, out ose) ? ose.EntityKey : null;
        }

        public static string GetEntityString(this EntityKey entityKey)
        {
            var result = new StringBuilder();
            if (entityKey == null)
            {
                throw new ArgumentNullException("entityKey");
            }

            foreach (var entry in entityKey.EntityKeyValues)
            {
                result.Append(string.Format("{0}={1}{2}", entry.Key, entry.Value, KeySeparator));
            }

            result.Remove(result.Length - 1, 1);
            return result.ToString();
        }
    }
}
