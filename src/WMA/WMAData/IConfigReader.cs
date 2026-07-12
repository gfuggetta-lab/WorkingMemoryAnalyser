using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WMAData
{
    // The interface to implement the configuration reader
    public interface IConfigReader
    {
        // populates the configuration class
        Task<bool> ReadConfig(Configuration cfg, CancellationToken cancel);

        // returns the list of numbers that are available with the configuration
        int[] GetInputDataListSync();
    }
}
