using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WMAData
{
    // The interface that allows to gain access
    // to the experiment information and resources
    public interface IExperimentReader
    {
        // returns true, if the file can be used for reading the configuration
        Task<bool> IsExperimentFile(string filename, CancellationToken cancel);

        // allocating the configuration reader, for the specified file
        //IExperimentReader GetReader(string filename);

        Task<IExperimentData> ReadExperiment(string filename, CancellationToken cancel);

    }
}
