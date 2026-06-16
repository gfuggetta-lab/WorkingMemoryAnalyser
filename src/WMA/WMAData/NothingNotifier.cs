using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WMAData
{
    public class NothingNotifier : IAsyncExperimentNotifier
    {
        public Task StartTrial(CancellationToken cancel)
        {
            return Task.CompletedTask;
        }

        public Task MarkerS1(int marker, CancellationToken cancel)
        {
            return Task.CompletedTask;
        }
        public Task MarkerS2(int marker, CancellationToken cancel)
        {
            return Task.CompletedTask;
        }
        public Task MarkerS3(int marker, CancellationToken cancel)
        {
            return Task.CompletedTask;
        }

        public Task MarkerS4(int marker, CancellationToken cancel)
        {
            return Task.CompletedTask;
        }

        public Task Feedback(bool isCorrect, CancellationToken cancel)
        {
            return Task.CompletedTask;
        }
    }
}
