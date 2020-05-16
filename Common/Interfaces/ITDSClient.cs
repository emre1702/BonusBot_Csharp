using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces
{
#nullable enable
    public interface ITDSClient
    {
        Task<string> UsedCommand(ulong userId, string command, List<string>? args = null);
    }
}
