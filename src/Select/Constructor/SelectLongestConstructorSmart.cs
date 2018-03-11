using System;
using System.Collections.Generic;
using System.Text;
using Unity.Policy;

namespace Unity.Select.Constructor
{
    public static class SelectLongestConstructorSmart
    {
        public static SelectConstructorPipeline SelectConstructorPipelineFactory(SelectConstructorPipeline next)
        {
            // Create Factory Method registration aspect
            return (IUnityContainer container, Type type, string name) =>
            {
                return next?.Invoke(container, type, name);
            };
        }
    }
}
