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
        Task<bool> IsConfigureFile(string filename);
        IConfigReader GetReader(string filename);
    }
}
