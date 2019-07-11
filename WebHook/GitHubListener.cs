using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BonusBot.Common.Helpers;
using Discord;
using Newtonsoft.Json;
using WebHook.Entity;
using WebHook.Entity.GitHub;
using WebHook.PostHandler;

namespace BonusBot.WebHook
{
    public class GitHubListener
    {
        private static Dictionary<ulong, GitHubListener> _createdListeners = new Dictionary<ulong, GitHubListener>();

        private HttpListener _listener;
        private readonly Action<string, LogSeverity, Exception> _logger;
        private readonly GuildWebHookSettings _settings;
        private readonly string _url;
        private readonly Handler _postHandler;
        private bool stopped;

        public GitHubListener(string url, GuildWebHookSettings settings, Action<string, LogSeverity, Exception> logger)
        {
            if (_createdListeners.ContainsKey(settings.Guild.Id))
            {
                _createdListeners[settings.Guild.Id].Stop();
            }

            _url = url;
            _settings = settings;
            _logger = logger;
            _postHandler = new Handler(settings, logger);

            _createdListeners[settings.Guild.Id] = this;

            CreateListener();
            StartListenerAsync();
        }

        public GitHubListener(string url, GuildWebHookSettings settings) : this(url, settings, (msg, severity, ex) =>
        {
            ConsoleHelper.Log(severity, "WebHook", $"WebHook [{severity.ToString()}]: {msg}:", ex);
        })
        { }

        public void Stop()
        {
            stopped = true;
            _listener?.Close();
            _listener = null;
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
                    stopped = true;
                    _listener.Close();
                    _listener = null;
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

            ConsoleHelper.Log(LogSeverity.Info, "WebHook", $"GitHub Listener started for guild {_settings.Guild.Name}.");

            _listener.BeginGetContext(HandleContext, null);
        }

        private async void HandleContext(IAsyncResult result)
        {
            try
            {
                if (_listener == null)
                    return;
                if (stopped)
                    return;
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
