using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightRiver.Net
{
    public sealed class ParserPool : IDisposable
    {
        private Queue<IParserWorkItem> _itemQueue = new Queue<IParserWorkItem>();

        private const int _parseSleepPeriod = 100; // 100ms

        private Task _parseTask = null;

        private CancellationTokenSource _parseCancelToken;

        private ManualResetEvent _parseSleepEvent = new ManualResetEvent(false);

        private bool _threadStopFlag = false;

        public ParserPool()
        {
        }

        public void Dispose()
        {
            _parseSleepEvent.Dispose();
        }

        private Task CreateParseWorkerTask()
        {
            _parseCancelToken = new CancellationTokenSource();
            return Task.Factory.StartNew(() => ParseWorkItemMethod(), _parseCancelToken.Token);
        }

        private void ParseWorkItemMethod()
        {
            while (!_threadStopFlag) {
                if (_itemQueue.Count == 0) {
                    _parseSleepEvent.WaitOne(_parseSleepPeriod);
                    continue;
                }

                // get a ActionItem from queue
                var item = _itemQueue.Dequeue();

                // start parse data
                item.Execute();
            }
        }

        public void Start()
        {
            _threadStopFlag = false;
            _parseTask = CreateParseWorkerTask();
        }

        public void Stop()
        {
            _threadStopFlag = true;
            _itemQueue.Clear();

            _parseCancelToken.Cancel(false);
            _parseTask.Wait(_parseCancelToken.Token);

            _parseCancelToken.Dispose();

            _parseCancelToken = null;
            _parseTask = null;
        }

        public void Enqueue(IParserWorkItem workItem)
        {
            if (workItem == null)
                return;

            _itemQueue.Enqueue(workItem);
        }
    }
}
