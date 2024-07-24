namespace Bit.Core.Abstractions
{
    public partial interface IFileService
    {
        bool CanOpenFile(string fileName);
        bool OpenFile(byte[] fileData, string id, string fileName);
        bool SaveFile(byte[] fileData, string id, string fileName, string contentUri);
        Task ClearCacheAsync();
        Task SelectFileAsync();
    }
}
