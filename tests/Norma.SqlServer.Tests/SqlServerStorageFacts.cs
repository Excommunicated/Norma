using System;
using Xunit;

namespace Norma.SqlServer.Tests
{
    public class SqlServerStorageFacts
    {

        private SqlServerStorageOptions _options;

        public SqlServerStorageFacts()
        {
            _options = new SqlServerStorageOptions{PrepareSchemaIfNecessary = false};
        }

        [Fact]
        public void Ctor_ThrowsAnException_WhenConnectionStringIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new SqlServerStorage((string) null));
            Assert.Equal("nameOrConnectionString",exception.ParamName);
        }

        [Fact]
        public void Ctor_ThrowsAnException_WhenOptionsValueIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new SqlServerStorage("hello", null));

            Assert.Equal("options", exception.ParamName);
        }

        [Fact, CleanDatabase]
        public void Ctor_CanCreateSqlServerStorage_WithExistingConnection()
        {
            var connection = ConnectionUtils.CreateConnection();
            var storage = new SqlServerStorage(connection);

            Assert.NotNull(storage);
        }

        [Fact, CleanDatabase]
        public void GetConnection_ReturnsExistingConnection_WhenStorageUsesIt()
        {
            var connection = ConnectionUtils.CreateConnection();
            var storage = new SqlServerStorage(connection);

            using (var storageConnection = (SqlServerConnection)storage.GetConnection())
            {
                Assert.Same(connection, storageConnection.Connection);
                Assert.False(storageConnection.OwnsConnection);
            }
        }

        [Fact, CleanDatabase]
        public void GetConnection_ReturnsNonNullInstance()
        {
            var storage = CreateStorage();
            using (var connection = (SqlServerConnection)storage.GetConnection())
            {
                Assert.NotNull(connection);
                Assert.True(connection.OwnsConnection);
            }
        }

        [Fact, CleanDatabase]
        public void AddAuditLog_ReturnsValidGuid()
        {
            var storage = CreateStorage();
            using (var connection = (SqlServerConnection) storage.GetConnection())
            {
                var guid = connection.AddAuditLog("1", "DummyEntity", "UnitTestingUser", DateTime.Now);
                Assert.NotEqual(Guid.Empty,guid);
            }
        }

        private SqlServerStorage CreateStorage()
        {
            return new SqlServerStorage(
                ConnectionUtils.GetConnectionString(),
                _options);
        }
    }
}
