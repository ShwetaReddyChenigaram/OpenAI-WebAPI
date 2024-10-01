using OpenAI_WebAPI.Models;

namespace OpenAI_WebAPI.Utilities
{
    public interface IFileWrapper
    {
        void WriteAllBytes(string path, byte[] bytes);

        FileDetails GetFileDetails(string fullFilePath, string subFolder = null);
    }

    public class FileWrapper : IFileWrapper
    {
        public void WriteAllBytes(string path, byte[] bytes)
        {
            File.WriteAllBytes(path, bytes);
        }

        public FileDetails GetFileDetails(string fullFilePath, string subFolder = null)
        {
            var fileInfo = new FileInfo(fullFilePath);
            return new FileDetails()
            {
                Extension = fileInfo.Extension,
                FileName = fileInfo.Name,
                FileSizeInByte = fileInfo.Length,
                ModifiedTime = fileInfo.LastWriteTime,
                FullPath = fullFilePath,
                SubFolder = subFolder
            };
        }
    }
}