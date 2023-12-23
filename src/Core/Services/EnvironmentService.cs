using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bit.Core.Abstractions;
using Bit.Core.Enums;
using Bit.Core.Models.Data;
using Bit.Core.Models.Domain;
using Bit.Core.Utilities;

namespace Bit.Core.Services
{
    public class EnvironmentService : IEnvironmentService
    {
        private const string DEFAULT_WEB_VAULT_URL = "https://vault.bitwarden.com";
        private const string DEFAULT_WEB_SEND_URL = "https://send.bitwarden.com/#";

        private readonly IApiService _apiService;
        private readonly IStateService _stateService;
        private readonly ICertificateService _certificateService;
        private readonly IConditionedAwaiterManager _conditionedAwaiterManager;
        private readonly IStorageService _storageService;

        public EnvironmentService(
            IApiService apiService,
            IStateService stateService,
            IConditionedAwaiterManager conditionedAwaiterManager)
        {
            _apiService = apiService;
            _stateService = stateService;
            _conditionedAwaiterManager = conditionedAwaiterManager;
            _storageService = ServiceContainer.Resolve<IStorageService>("storageService");
            _certificateService = ServiceContainer.Resolve<ICertificateService>("certificateService");
        }

        public string BaseUrl { get; set; }
        public string WebVaultUrl { get; set; }
        public string ApiUrl { get; set; }
        public string IdentityUrl { get; set; }
        public string IconsUrl { get; set; }
        public string NotificationsUrl { get; set; }
        public string EventsUrl { get; set; }
        public Region SelectedRegion { get; set; }
        public string ClientCertUri { get; set; }

        public string GetWebVaultUrl(bool returnNullIfDefault = false)
        {
            if (!string.IsNullOrWhiteSpace(WebVaultUrl))
            {
                return WebVaultUrl;
            }

            if (!string.IsNullOrWhiteSpace(BaseUrl))
            {
                return BaseUrl;
            }

            return returnNullIfDefault ? (string)null : DEFAULT_WEB_VAULT_URL;
        }

        public string GetWebSendUrl()
        {
            return GetWebVaultUrl(true) is string webVaultUrl ? $"{webVaultUrl}/#/send/" : DEFAULT_WEB_SEND_URL;
        }

        public string GetCurrentDomain()
        {
            return new EnvironmentUrlData
            {
                WebVault = WebVaultUrl,
                Base = BaseUrl,
                Api = ApiUrl,
                Identity = IdentityUrl
            }.GetDomainOrHostname();
        }

        public async Task SetUrlsFromStorageAsync()
        {
            try
            {
                var region = await _stateService.GetActiveUserRegionAsync();
                ClientCertUri = await GetClientCertificateUriFromStorageAsync();

                if (ClientCertUri != null)
                {
                    var certSpec = await _certificateService.GetCertificateAsync(ClientCertUri);
                    _apiService.UseClientCertificate(certSpec);
                }

                var urls = await _stateService.GetEnvironmentUrlsAsync();
                urls ??= await _stateService.GetPreAuthEnvironmentUrlsAsync();

                if (urls == null || urls.IsEmpty || region is null)
                {
                    await SetRegionAsync(Region.US);
                    _conditionedAwaiterManager.SetAsCompleted(AwaiterPrecondition.EnvironmentUrlsInited);
                    return;
                }

                await SetRegionAsync(region.Value, urls);
                _conditionedAwaiterManager.SetAsCompleted(AwaiterPrecondition.EnvironmentUrlsInited);
            }
            catch (System.Exception ex)
            {
                _conditionedAwaiterManager.SetException(AwaiterPrecondition.EnvironmentUrlsInited, ex);
                throw;
            }

        }

        public async Task<EnvironmentUrlData> SetRegionAsync(Region region, EnvironmentUrlData selfHostedUrls = null)
        {
            EnvironmentUrlData urls;

            if (region == Region.SelfHosted)
            {
                // If user saves a self-hosted region with empty fields, default to US
                if (selfHostedUrls.IsEmpty)
                {
                    return await SetRegionAsync(Region.US);
                }
                urls = selfHostedUrls.FormatUrls();
            }
            else
            {
                urls = region.GetUrls();
            }

            SelectedRegion = region;
            await _stateService.SetPreAuthRegionAsync(region);
            await _stateService.SetPreAuthEnvironmentUrlsAsync(urls);
            BaseUrl = urls.Base;
            WebVaultUrl = urls.WebVault;
            ApiUrl = urls.Api;
            IdentityUrl = urls.Identity;
            IconsUrl = urls.Icons;
            NotificationsUrl = urls.Notifications;
            EventsUrl = urls.Events;
            _apiService.SetUrls(urls);
            return urls;
        }

        public async Task SetClientCertificate(string certUri)
        {
            var certSpec = await _certificateService.GetCertificateAsync(certUri);
            await _storageService.SaveAsync(Constants.ClientAuthCertificateUriKey, certUri);
            _apiService.UseClientCertificate(certSpec);
            ClientCertUri = certUri;
        }

        private async Task<string> GetClientCertificateUriFromStorageAsync()
        {
            try
            {
                return await _storageService.GetAsync<string>(Constants.ClientAuthCertificateUriKey);
            }
            catch
            {
                return string.Empty;
            }
        }

        private string FormatUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }
            url = Regex.Replace(url, "\\/+$", string.Empty);
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = string.Concat("https://", url);
            }
            return url.Trim();
        }

        public async Task RemoveExistingClientCert()
        {
            var existingCertUri = await GetClientCertificateUriFromStorageAsync();
            if (existingCertUri != null)
            {
                _certificateService.TryRemoveCertificate(existingCertUri);
                await _storageService.RemoveAsync(Constants.ClientAuthCertificateUriKey);
                _apiService.UseClientCertificate(null);
            }
            ClientCertUri = null;
        }
    }
}
