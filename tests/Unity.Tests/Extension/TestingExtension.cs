using Unity.Extension;
using Unity.Storage;

namespace Unity.Container.Tests.Extension
{
    /// <summary>
    /// A simple extension that puts the supplied strategy into the
    /// chain at the indicated stage.
    /// </summary>
    internal class TestingExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
        }

        public IPolicyList Policies => Context.Policies;
    }
}
