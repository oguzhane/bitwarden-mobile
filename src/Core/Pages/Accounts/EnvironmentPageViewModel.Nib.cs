using System.Windows.Input;
using Bit.Core.Resources.Localization;
using Bit.Core.Abstractions;
using Bit.Core.Utilities;
using BwRegion = Bit.Core.Enums.Region;

namespace Bit.App.Pages
{
    // Tag:Nibblewarden
    public partial class EnvironmentPageViewModel : BaseViewModel
    {
        private ICertificateService _certificateService;

        private string _certificateAlias = "";
        private string _certificateUri = null;
        private string _certificateDetails = "";
        private bool _certificateHasChanged;

        private void NibbleCtor()
        {
            _certificateService = ServiceContainer.Resolve<ICertificateService>("certificateService");
            ImportCertCommand = CreateDefaultAsyncRelayCommand(ImportCertAsync, onException: OnCertCommandException, allowsMultipleExecutions: false);
            UseSystemCertCommand = CreateDefaultAsyncRelayCommand(UseSystemCertAsync, onException: OnCertCommandException, allowsMultipleExecutions: false);
            RemoveCertCommand = CreateDefaultAsyncRelayCommand(RemoveCertAsync, onException: OnCertCommandException, allowsMultipleExecutions: false);
        }

        private void NibbleInit()
        {
            _certificateUri = _environmentService.ClientCertUri;
            BindCertDetailsAsync().FireAndForget();
        }

        private void OnCertCommandException(Exception ex)
        {
            _logger.Value.Exception(ex);
            Page.DisplayAlert(AppResources.AnErrorHasOccurred, AppResources.GenericErrorMessage, AppResources.Ok);
        }

        public ICommand ImportCertCommand { get; set; }
        public ICommand UseSystemCertCommand { get; set; }
        public ICommand RemoveCertCommand { get; set; }

        public string CertificateAlias
        {
            get => _certificateAlias;
            set => SetProperty(ref _certificateAlias, value);
        }
        public string CertificateDetails
        {
            get => _certificateDetails;
            set => SetProperty(ref _certificateDetails, value);
        }
        public string CertificateUri
        {
            get => _certificateUri;
            set
            {
                SetProperty(ref _certificateUri, value);
                _certificateHasChanged = true;
                BindCertDetailsAsync().FireAndForget();
            }
        }

        public async Task ImportCertAsync()
        {
            try
            {
                CertificateUri = await _certificateService.ImportCertificateAsync();
            }
            catch (Exception ex)
            {
                await Page.DisplayAlert(AppResources.AnErrorHasOccurred, $"Failed to import the cert!\n{ex.Message}", AppResources.Ok);
            }

        }

        public async Task RemoveCertAsync()
        {
            // Mark current certificate to be removed
            CertificateUri = null;
        }

        public async Task UseSystemCertAsync()
        {
            CertificateUri = await _certificateService.ChooseSystemCertificateAsync();
        }

        private async Task BindCertDetailsAsync()
        {
            try
            {
                if (CertificateUri != null)
                {
                    var cert = await _certificateService.GetCertificateAsync(CertificateUri);

                    CertificateAlias = cert.Alias;
                    CertificateDetails = cert.ToString();
                }
                else
                {
                    CertificateAlias = null;
                    CertificateDetails = null;
                }
            }
            catch (Exception ex)
            {
                await Page.DisplayAlert(AppResources.AnErrorHasOccurred, $"Failed to read cert details!\n{ex.Message}", AppResources.Ok);
            }
        }

        private async Task ApplyCertChanges()
        {
            if (!_certificateHasChanged) return;

            await _environmentService.RemoveExistingClientCert();

            if (CertificateUri != null)
            {
                await _environmentService.SetClientCertificate(CertificateUri);
            }
        }
    }
}
