using System;

namespace Common.Interfaces
{
    public interface IGitHubListener : IDisposable
    {
        void StopListener();
    }
}
