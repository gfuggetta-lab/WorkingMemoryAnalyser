using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WMAData
{
    // The async interface for the flow notifier.
    public interface IAsyncExperimentNotifier
    {
        Task StartExperiment();
        Task MarkerS1(int marker);
        Task MarkerS2(int marker);
        Task MarkerS3(int marker);
        Task MarkerS4(int marker);
        Task Feedback(bool isCorrect);
    }
}
