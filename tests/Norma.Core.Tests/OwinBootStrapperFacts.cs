using System;
using Moq;
using Owin;
using Xunit;

namespace Norma.Core.Tests
{
    public class OwinBootStrapperFacts
    {
        [Fact, GlobalLock(Reason = "Access static OwinBootstrapper.UseNorma member")]
        public void UseNorma_ThrowsArgumentNullException_WhenIAppBuilderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => OwinBootstrapper.UseNorma(null, null));
        }

        [Fact, GlobalLock(Reason = "Access static OwinBootstrapper.UseNorma member")]
        public void UseNorma_ThrowsArgumentNullException_WhenActionIsNull()
        {
            var mockBuilder = new Mock<IAppBuilder>();
            Assert.Throws<ArgumentNullException>(() => mockBuilder.Object.UseNorma(null));
        }

        [Fact, GlobalLock(Reason = "Access static OwinBootstrapper.UseNorma member")]
        public void UseNorma_ThrowsInvalidOperationException_WhenStorageIsNotConfigured()
        {
            var mockBuilder = new Mock<IAppBuilder>();
            Assert.Throws<InvalidOperationException>(() => mockBuilder.Object.UseNorma(x=>{}));
        }

        [Fact, GlobalLock(Reason = "Access static OwinBootstrapper.UseNorma member")]
        public void UseNorma_ThrowsInvalidOperationException_WhenOrmTypeIsNotConfigured()
        {
            var mockBuilder = new Mock<IAppBuilder>();
            var mockStorage = new Mock<AuditLogStorage>();
            Assert.Throws<InvalidOperationException>(() => mockBuilder.Object.UseNorma(x => x.UseStorage(mockStorage.Object)));
        }
        
        [Fact, GlobalLock(Reason = "Access static OwinBootstrapper.UseNorma member")]
        public void UseNorma_DoesNotOverrideActivator_WhenNotConfigured()
        {
            AuditLogActivator.Current = new AuditLogActivator();

            var mockBuilder = new Mock<IAppBuilder>();
            var mockStorage = new Mock<AuditLogStorage>();
            var mockOrmType = new Mock<AuditLogOrmType>();
            mockBuilder.Object.UseNorma(x =>
            {
                x.UseStorage(mockStorage.Object);
                x.UseOrm(mockOrmType.Object);
            });
            Assert.IsType<AuditLogActivator>(AuditLogActivator.Current);
        }

        [Fact, GlobalLock(Reason = "Access static OwinBootstrapper.UseNorma member")]
        public void UseNorma_OverridesActivator_WhenConfigured()
        {
            var mockBuilder = new Mock<IAppBuilder>();
            var mockStorage = new Mock<AuditLogStorage>();
            var mockOrmType = new Mock<AuditLogOrmType>();
            var mockActivator = new Mock<AuditLogActivator>();
            mockBuilder.Object.UseNorma(x =>
            {
                x.UseStorage(mockStorage.Object);
                x.UseOrm(mockOrmType.Object);
                x.UseActivator(mockActivator.Object);
            });
            Assert.IsNotType<AuditLogActivator>(AuditLogActivator.Current);
            Assert.IsAssignableFrom<AuditLogActivator>(AuditLogActivator.Current);
        }

        [Fact, GlobalLock(Reason = "Access static OwinBootstrapper.UseNorma member")]
        public void UseNorma_AuditLogStorage_CorrectlyConfigured()
        {
            var mockBuilder = new Mock<IAppBuilder>();
            var mockStorage = new Mock<AuditLogStorage>();
            var mockOrmType = new Mock<AuditLogOrmType>();
            mockBuilder.Object.UseNorma(x =>
            {
                x.UseStorage(mockStorage.Object);
                x.UseOrm(mockOrmType.Object);
            });
            Assert.Same(mockStorage.Object, AuditLogStorage.Current);
        }

        [Fact, GlobalLock(Reason = "Access static OwinBootstrapper.UseNorma member")]
        public void UseNorma_AuditLogOrmType_CorrectlyConfigured()
        {
            var mockBuilder = new Mock<IAppBuilder>();
            var mockStorage = new Mock<AuditLogStorage>();
            var mockOrmType = new Mock<AuditLogOrmType>();
            mockBuilder.Object.UseNorma(x =>
            {
                x.UseStorage(mockStorage.Object);
                x.UseOrm(mockOrmType.Object);
            });
            Assert.Same(mockOrmType.Object, AuditLogOrmType.Current);
        }
    }
}