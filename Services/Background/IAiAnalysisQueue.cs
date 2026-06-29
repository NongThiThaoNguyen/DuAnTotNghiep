using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Background
{
    public interface IAiAnalysisQueue
    {
        ValueTask QueueAttemptForAnalysisAsync(int attemptId, int studentId);
        ValueTask<(int AttemptId, int StudentId)> DequeueAsync(CancellationToken cancellationToken);
    }

    public class AiAnalysisQueue : IAiAnalysisQueue
    {
        private readonly Channel<(int, int)> _queue;

        public AiAnalysisQueue()
        {
            var options = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<(int, int)>(options);
        }

        public async ValueTask QueueAttemptForAnalysisAsync(int attemptId, int studentId)
        {
            await _queue.Writer.WriteAsync((attemptId, studentId));
        }

        public async ValueTask<(int AttemptId, int StudentId)> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
