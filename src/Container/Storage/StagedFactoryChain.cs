using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Build.Pipeline;
using Unity.Storage;

namespace Unity.Container.Storage
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a chain of responsibility for builder strategies partitioned by stages.
    /// </summary>
    /// <typeparam name="TStageEnum">The stage enumeration to partition the strategies.</typeparam>
    /// <typeparam name="TPipeline"></typeparam>
    public class StagedFactoryChain<TPipeline, TStageEnum> : IStagedFactoryChain<TPipeline, TStageEnum>
    {
        #region Fields

        private readonly object _lockObject = new object();
        private readonly IList<PipelineFactory<TPipeline, TPipeline>>[] _stages;

        #endregion


        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="StagedFactoryChain{TStrategyType,TStageEnum}"/> class.
        /// </summary>
        public StagedFactoryChain()
        {
            var values = Enum.GetValues(typeof(TStageEnum));
            _stages = new IList<PipelineFactory<TPipeline, TPipeline>>[values.Length];

            for (var i = 0; i < values.Length; i++)
                _stages[i] = new List<PipelineFactory<TPipeline, TPipeline>>();
        }

        #endregion


        #region IStagedChain


        /// <summary>
        /// Signals that chain has been changed
        /// </summary>
        public event EventHandler<EventArgs> Invalidated;

        public void Add(PipelineFactory<TPipeline, TPipeline> factory, TStageEnum stage)
        {
            lock (_lockObject)
            {
                _stages[Convert.ToInt32(stage)].Add(factory);
                Invalidated?.Invoke(this, new EventArgs());
            }
        }

        public bool Remove(PipelineFactory<TPipeline, TPipeline> item)
        {
            lock (_lockObject)
            {
                foreach (var list in _stages)
                {
                    if (list.Contains(item))
                    {
                        list.Remove(item);
                        Invalidated?.Invoke(this, new EventArgs());
                        return true;
                    }
                }
            }

            return false;
        }

        public TPipeline BuildPipeline()
        {
            TPipeline method = default;
            lock (_lockObject)
            {
                for (var e = _stages.Length - 1; e > -1; --e)
                {
                    var list = _stages[e];
                    for (var i = 0; i < list.Count; i++)
                    {
                        method = list[i](method);
                    }
                }
            }

            return method;
        }

        #endregion


        #region IEnumerable

        public IEnumerator<PipelineFactory<TPipeline, TPipeline>> GetEnumerator()
        {
            lock (_lockObject)
            {
                foreach (var list in _stages)
                {
                    foreach (var pipeline in list)
                    {
                        yield return pipeline;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
