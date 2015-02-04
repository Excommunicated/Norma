using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using Dapper;
using Norma.Model;
using Norma.Storage;

namespace Norma.SqlServer
{
    public class SqlServerConnection : IStorageConnection
    {
        public bool OwnsConnection { get; private set; }
        private readonly SqlConnection _connection;

        public SqlServerConnection(SqlConnection connection, bool ownsConnection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            OwnsConnection = ownsConnection;
            _connection = connection;
        }

        public void Dispose()
        {
            if (OwnsConnection)
            {
                _connection.Dispose();
            }
        }

        public Guid AddAuditLog(string entityId, string entityFullName, string user, DateTime auditDateTime)
        {
            const string sql = @"Merge Norma.AuditLog as Target
                                using (Values(@entityFullName,@entityId)) as [Source] (EntityFullName, EntityId)
                                on Target.EntityFullName = Source.EntityFullName and Target.EntityId = Source.EntityId
                                WHEN matched then update set LastUpdated = @auditDateTime, LastUpdatedUser = @user
                                when not matched then insert(Id,Created,LastUpdated,LastUpdatedUser,EntityFullName,EntityId) values (NEWID(), @auditDateTime,@auditDateTime, @user,Source.EntityFullName,Source.EntityId)
                                output Inserted.*;";

            var auditLog = _connection.Query<AuditLog>(sql, new {entityFullName, entityId, user, auditDateTime}).SingleOrDefault();
            return auditLog.Id;
        }

        public IWriteOnlyTransaction CreateWriteOnlyTransaction()
        {
            return new SqlServerWriteOnlyTransaction(_connection);
        }
    }

    public class SqlServerWriteOnlyTransaction : IWriteOnlyTransaction
    {
        private readonly Queue<Action<SqlConnection>> _commandQueue
            = new Queue<Action<SqlConnection>>();

        private readonly SqlConnection _connection;

        public SqlServerWriteOnlyTransaction(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            _connection = connection;
        }

        public void Dispose()
        {
            
        }

        public void AddToAuditLogChange(AuditLogChange log)
        {
            QueueCommand(
                x =>
                    x.Execute(
                        @"insert into Norma.AuditLogChange ([Id],[AuditLogId],[Created],[User],[OldValue],[NewValue],[PropertyName],[Operation]) values (@Id,@AuditLogId,@Created,@User,@OldValue,@NewValue,@PropertyName,@Operation)",
                        new
                        {
                            log.Id,
                            log.AuditLogId,
                            log.Created,
                            log.User,
                            log.OldValue,
                            log.NewValue,
                            log.PropertyName,
                            log.Operation
                        }));
        }

        public void Commit()
        {
            using (var transaction = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
            {
                _connection.EnlistTransaction(Transaction.Current);

                foreach (var command in _commandQueue)
                {
                    command(_connection);
                }

                transaction.Complete();
            }
        }

        internal void QueueCommand(Action<SqlConnection> action)
        {
            _commandQueue.Enqueue(action);
        }
    }
}