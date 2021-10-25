using System;
using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;
using PnP.Framework;

namespace FuncInsertSharepointDocumentUrls.Clients
{
    public class SharepointClient : ISharepointClient
    {
        private readonly string _sharepointUrl;
        private readonly string _appId;
        private readonly string _appSecret;

        private readonly ILogger<SharepointClient> _log;

        public SharepointClient(ILogger<SharepointClient> log)
        {
            _sharepointUrl = Environment.GetEnvironmentVariable("SharepointUrl");
            _appId = Environment.GetEnvironmentVariable("AppId");
            _appSecret = Environment.GetEnvironmentVariable("AppSecret");
            _log = log;
        }

        public ClientContext GetContext()
        {
            try
            {
                using AuthenticationManager authenticationManager = new();
                return authenticationManager.GetACSAppOnlyContext(_sharepointUrl, _appId, _appSecret);
            }
            catch(Exception e)
            {
                _log.LogCritical("Could not authenticate to Sharepoint: {Message}, {StackTrace}", e.Message, e.StackTrace);
                return null;
            }
        }
    }
}