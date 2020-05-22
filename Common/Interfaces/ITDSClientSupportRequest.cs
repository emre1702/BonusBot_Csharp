using System.Threading.Tasks;
using Common.Enums;

namespace Common.Interfaces
{
    public interface ITDSClientSupportRequest
    {
        Task<string> Answer(ulong userId, int supportRequestId, string text);
        Task<string> Create(ulong userId, string title, string text, SupportType supportType, int atleastAdminLevel);
        Task<string> ToggleClosed(ulong userId, int supportRequestId, bool closed);
    }
}
