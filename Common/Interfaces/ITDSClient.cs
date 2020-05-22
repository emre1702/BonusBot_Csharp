using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces
{
#nullable enable
    public interface ITDSClient
    {
        ITDSClientCommand Command { get; }
        ITDSClientSupportRequest SupportRequest { get; }
        
    }
}
