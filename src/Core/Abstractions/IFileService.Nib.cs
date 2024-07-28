namespace Bit.Core.Abstractions
{
    // Tag:Nibblewarden
    public partial interface IFileService
    {
        Task<T> SelectFileAsync<T>() where T : class;
    }
}
