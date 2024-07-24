namespace Bit.Core.Abstractions
{
    public partial interface IEnvironmentService
    {
        string ClientCertUri { get; set; }
        Task SetClientCertificate(string certUri);
        Task RemoveExistingClientCert();
    }
}
