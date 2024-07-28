using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bit.Core.Abstractions;

namespace Bit.Core.Services
{
    public partial class EnvironmentService: IEnvironmentService
    {
        public string ClientCertUri { get; set; }
        public async Task SetClientCertificate(string certUri)
        {
            this.GetCurrentDomain();
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
                return null;
            }
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
