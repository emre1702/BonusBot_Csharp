using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BonusBot.Common.Helpers;
using Discord;
using Newtonsoft.Json;
using WebHook.Entity.GitHub;
using WebHook.PostHandler;

namespace BonusBot.WebHook
{
    public class GitHubListener
    {
        private HttpListener _listener;
        private readonly Action<string, LogSeverity, Exception> _logger;
        private readonly ITextChannel _outputToChannel;
        private readonly string _url;
        private readonly Handler _postHandler;

        public GitHubListener(string url, ITextChannel outputToChannel, Action<string, LogSeverity, Exception> logger)
        {
            _url = url;
            _outputToChannel = outputToChannel;
            _logger = logger;
            _postHandler = new Handler(outputToChannel, logger);

            CreateListener();
            StartListenerAsync();
        }

        private bool CreateListener()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(_url);
                return true;
            }
            catch (Exception ex)
            {
                _logger($"Can't create the agent to listen transaction", LogSeverity.Error, ex);
                return false;
            }
        }

        private async void StartListenerAsync()
        {
            Exception exception = null;
            int countTry = 0;

            do
            {
                try
                {
                    _listener.Start();
                    exception = null;
                }
                catch (HttpListenerException)
                {
                    _listener.Close();
                    return;
                }
                catch (ObjectDisposedException)
                {
                    if (!CreateListener())
                        return;
                }
                catch (Exception ex)
                {
                    exception ??= ex;
                    await Task.Delay(3000);
                }
            }
            while (exception != null && ++countTry < 5);

            if (exception != null)
            {
                _logger($"Can't start the agent to listen transaction", LogSeverity.Error, exception);
                return;
            }

            ConsoleHelper.Log(LogSeverity.Info, "WebHook", $"GitHub Listener started for guild {_outputToChannel.Guild.Name}.");

            _listener.BeginGetContext(HandleContext, null);
        }

        public GitHubListener(string url, ITextChannel outputToChannel) : this(url, outputToChannel, (msg, severity, ex) =>
        {
            ConsoleHelper.Log(severity, "WebHook", $"WebHook [{severity.ToString()}]: {msg}:", ex);
        }) { }

        private async void HandleContext(IAsyncResult result)
        {
            try
            {
                var context = _listener.EndGetContext(result);
                var path = context.Request.Url.LocalPath;
                string responseContent = null;

                using (Stream receiveStream = context.Request.InputStream)
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    responseContent = readStream.ReadToEnd();

                if (path.StartsWith("/echoPost"))
                    await _postHandler.HandleEchoPost(responseContent);

                var response = context.Response;
                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentLength64 = 0;
                response.Close();

                _listener.BeginGetContext(HandleContext, null);
            }
            catch (Exception ex)
            {
                _logger("Error at reading GitHub request", LogSeverity.Error, ex);
            }
        }
    }

}
