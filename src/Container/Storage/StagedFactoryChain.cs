using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Build.Pipeline;
using Unity.Storage;

namespace Unity.Container.Storage
{
    /// <summary>
    /// Represents a chain of responsibility for builder strategies partitioned by stages.
    /// </summary>
    /// <typeparam name="TStageEnum">The stage enumeration to partition the strategies.</typeparam>
    /// <typeparam name="TPipeline"></typeparam>
    public class StagedFactoryChain<TPipeline, TStageEnum> : IEnumerable<PipelineFactory<TPipeline, TPipeline>>, 
                                                             IDisposable
    {
        #region Fields

        private readonly int _length;
        private readonly object _lockObject = new object();
        private readonly StagedFactoryChain<TPipeline, TStageEnum> _parent;

        private IList<PipelineFactory<TPipeline, TPipeline>>[] _stages;

        #endregion


        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="StagedFactoryChain{TStrategyType,TStageEnum}"/> class.
        /// </summary>
        public StagedFactoryChain()
        {
            _length = Enum.GetValues(typeof(TStageEnum)).Length;
            Initialize();
        }

        public StagedFactoryChain(StagedFactoryChain<TPipeline, TStageEnum> parent)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _length = parent._length;

            _parent.Invalidated += OnParentInvalidated;
        }

        #endregion


        #region IStagedChain

        public event EventHandler<EventArgs> Invalidated;

        public void Add(PipelineFactory<TPipeline, TPipeline> factory, TStageEnum stage)
        {
            lock (_lockObject)
            {
                if (null == _stages) Initialize();

                _stages[Convert.ToInt32(stage)].Add(factory);
                Invalidated?.Invoke(this, new EventArgs());
            }
        }

        public bool Remove(PipelineFactory<TPipeline, TPipeline> item)
        {
            lock (_lockObject)
            {
                if (null == _stages) return false;

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
                    if (null != _parent)
                        method = _parent.BuildPipeline(e, method);

                    method = BuildPipeline(e, method);
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


        #region IDisposable

        public void Dispose()
        {
            if (null != _parent) _parent.Invalidated -= OnParentInvalidated;
        }

        #endregion


        #region Implementation

        private TPipeline BuildPipeline(int index, TPipeline method)
        {
            lock (_lockObject)
            {
                if (null == _stages) return method;

                foreach (var stage in _stages[index])
                    method = stage(method);
            }

            return method;
        }

        private void OnParentInvalidated(object sender, EventArgs e)
        {
            Invalidated?.Invoke(this, e);
        }

        private void Initialize()
        {
            _stages = new IList<PipelineFactory<TPipeline, TPipeline>>[_length];

            for (var i = 0; i < _length; i++)
                _stages[i] = new List<PipelineFactory<TPipeline, TPipeline>>();
        }

        #endregion
    }
}
