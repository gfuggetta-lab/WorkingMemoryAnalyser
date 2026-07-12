using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WMAData
{
    // The interface that allows to gain access
    // to the experiment information and resources
    public interface IDataProvider
    {
        // returns true, if the file can be used for reading the configuration
        Task<bool> IsConfigureFile(string filename);

        // allocating the configuration reader, for the specified file
        IConfigReader GetReader(string filename);
    }
}
