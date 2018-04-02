
using Unity.Build.Context;

namespace Unity.Build.Pipeline
{
    public delegate object ResolvePipeline(ResolveDelegate resolve);

    public interface IResolvePipeline
    {
        ResolvePipeline ResolvePipeline { get; }
    }

}
