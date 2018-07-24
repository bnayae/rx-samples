using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnaya.Samples
{
    public class FaultObservable : IObservable<long>
    {
        public IDisposable Subscribe(IObserver<long> observer)
        {
            throw new NotImplementedException(":(");
        }
    }
}
