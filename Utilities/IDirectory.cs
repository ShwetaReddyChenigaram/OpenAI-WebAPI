using System.Security.AccessControl;

namespace OpenAI_WebAPI.Utilities
{
    public interface IDirectoryWrapper
    {
        bool Exists(string path);

        string[] GetFiles(string path);

        string[] GetDirectories(string path);

        DirectoryInfo CreateDirectory(string path);

        DirectorySecurity GetAccessControl(string directory);

        void SetAccessControl(string directory, DirectorySecurity security);
    }

    public class DirectoryWrapper : IDirectoryWrapper
    {
        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        public string[] GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        public string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        public DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }

        public DirectorySecurity GetAccessControl(string directory)
        {
            return new DirectoryInfo(directory).GetAccessControl();
        }

        public void SetAccessControl(string directory, DirectorySecurity security)
        {
            new DirectoryInfo(directory).SetAccessControl(security);
        }
    }
}