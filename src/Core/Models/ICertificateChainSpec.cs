namespace Bit.Core.Models
{
    // Tag:Nibblewarden
    public interface ICertificateChainSpec<T, U> : ICertificateChainSpec
    {
        U PrivateKeyRef { get; }
        T LeafCertificate { get; }
        T RootCertificate { get; }
        T[] CertificateChain { get; }
    }

    // Tag:Nibblewarden
    public interface ICertificateChainSpec : IFormattable
    {
        string Alias { get; }
    }
}
