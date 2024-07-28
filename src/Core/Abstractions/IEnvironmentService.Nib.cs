namespace Bit.Core.Abstractions
{
    // Tag:Nibblewarden
    public partial interface IEnvironmentService
    {
        string ClientCertUri { get; set; }
        Task SetClientCertificate(string certUri);
        Task RemoveExistingClientCert();
    }
}
