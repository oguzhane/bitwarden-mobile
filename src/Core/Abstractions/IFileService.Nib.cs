namespace Bit.Core.Abstractions
{
    public partial interface IFileService
    {
        Task<T> SelectFileAsync<T>() where T : class;
    }
}
