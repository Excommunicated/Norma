using System;
using Moq;
using Xunit;

namespace Norma.Core.Tests
{
    public class AuditLogStorageFacts
    {
        private readonly Mock<AuditLogStorage> _storage;

        public AuditLogStorageFacts()
        {
            _storage = new Mock<AuditLogStorage> {CallBase = true};
        }

        [Fact,GlobalLock(Reason = "Access static AuditLogStorage.Current member")]
        public void SetCurrent_DoesNotThrowAnException_WhenValueIsNull()
        {
            Assert.DoesNotThrow(() => AuditLogStorage.Current = null);
        }

        [Fact, GlobalLock(Reason = "Access static AuditLogStorage.Current member")]
        public void GetCurrent_ThrowsAnException_OnUninitializedValue()
        {
            AuditLogStorage.Current = null;

            Assert.Throws<InvalidOperationException>(() => AuditLogStorage.Current);
        }

        [Fact, GlobalLock(Reason = "Access static AuditLogStorage.Current member")]
        public void GetCurrent_ReturnsCurrentValue_WhenInitialized()
        {
            var storage = new Mock<AuditLogStorage>();
            AuditLogStorage.Current = storage.Object;

            Assert.Same(storage.Object, AuditLogStorage.Current);
        }
    }
}
