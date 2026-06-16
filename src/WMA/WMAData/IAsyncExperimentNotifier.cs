using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WMAData
{
    // The async interface for the flow notifier.
    public interface IAsyncExperimentNotifier
    {
        Task StartTrial(CancellationToken cancel);
        Task MarkerS1(int marker, CancellationToken cancel);
        Task MarkerS2(int marker, CancellationToken cancel);
        Task MarkerS3(int marker, CancellationToken cancel);
        Task MarkerS4(int marker, CancellationToken cancel);
        Task Feedback(bool isCorrect, CancellationToken cancel);
    }
}
