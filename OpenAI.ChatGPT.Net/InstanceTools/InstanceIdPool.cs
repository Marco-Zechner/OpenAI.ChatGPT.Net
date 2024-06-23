using System.Collections.Concurrent;

namespace OpenAI.ChatGPT.Net.InstanceTools
{
    public class InstanceIdPool
    {
        private readonly ConcurrentQueue<long> _availableIds;
        private long _nextId;

        public InstanceIdPool()
        {
            _availableIds = new ConcurrentQueue<long>();
            _nextId = 0;
        }

        public long GetId()
        {
            if (_availableIds.TryDequeue(out var id))
            {
                return id;
            }
            return _nextId++;
        }

        public void ReleaseId(long id)
        {
            _availableIds.Enqueue(id);
        }
    }
}
