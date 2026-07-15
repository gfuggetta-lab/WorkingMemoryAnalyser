using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WMAData
{
    public static class Helpers
    {


        #region Async Experiment Notifier helpers
        public static Task StartTrial(this IAsyncExperimentNotifier nt)
        {
            return nt.StartTrial(CancellationToken.None);
        }

        public static Task MarkerS1(this IAsyncExperimentNotifier nt, int marker)
        {
            return nt.MarkerS1(marker, CancellationToken.None);
        }
        public static Task MarkerS2(this IAsyncExperimentNotifier nt, int marker)
        {
            return nt.MarkerS2(marker, CancellationToken.None);
        }
        public static Task MarkerS3(this IAsyncExperimentNotifier nt, int marker)
        {
            return nt.MarkerS3(marker, CancellationToken.None);
        }
        public static Task MarkerS4(this IAsyncExperimentNotifier nt, int marker)
        {
            return nt.MarkerS4(marker, CancellationToken.None);
        }
        public static Task Feedback(this IAsyncExperimentNotifier nt, bool isCorrect)
        {
            return nt.Feedback(isCorrect, CancellationToken.None);
        }

        #endregion
    }
}
