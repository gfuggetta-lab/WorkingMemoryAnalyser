using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WMAData
{
    public class NothingNotifier : IAsyncExperimentNotifier
    {
        public Task StartExperiment()
        {
            return Task.CompletedTask;
        }

        public Task MarkerS1(int marker)
        {
            return Task.CompletedTask;
        }
        public Task MarkerS2(int marker)
        {
            return Task.CompletedTask;
        }
        public Task MarkerS3(int marker)
        {
            return Task.CompletedTask;
        }

        public Task MarkerS4(int marker)
        {
            return Task.CompletedTask;
        }

        public Task Feedback(bool isCorrect)
        {
            return Task.CompletedTask;
        }
    }
}
