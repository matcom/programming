namespace filesystem
{

    public delegate bool FileFilter(IFile file);

    public interface IFile
    {
        int Size { get; }
        string Name { get; }
    }

    public interface IFolder
    {
        string Name { get; }

        IFile CreateFile(string name, int size);
        IFolder CreateFolder(string name);

        IEnumerable<IFile> GetFiles();
        IEnumerable<IFolder> GetFolders();
        int TotalSize();
    }

    public interface IFileSystem
    {
        IFolder GetFolder(string path);
        IFile GetFile(string path);
        IFileSystem GetRoot(string path);

        IEnumerable<IFile> Find(FileFilter filter);

        void Copy(string origin, string destination);
        void Move(string origin, string destination);
        void Delete(string path);
    }
}
