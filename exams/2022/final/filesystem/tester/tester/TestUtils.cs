namespace MatCom.Tester;
using filesystem;

public class FileComparer: IEqualityComparer<IFile>
{
    public bool Equals(IFile? f1, IFile? f2)
    {
        if(f1 is null)
            return f2 is null;

        if(f2 is null)
            return f1 is null;

        return f1.Name == f2.Name && f1.Size == f2.Size;
    }

    public int GetHashCode(IFile f) => (f.Name, f.Size).GetHashCode();
}

public class FolderComparer: IEqualityComparer<IFolder>
{
    public bool Equals(IFolder? f1, IFolder? f2)
    {
        if(f1 is null)
            return f2 is null;

        if(f2 is null)
            return f1 is null;

        return f1.Name == f2.Name &&
                f1.TotalSize() == f2.TotalSize() &&
                f1.GetFiles().SequenceEqual(f2.GetFiles(), new FileComparer()) &&
                f1.GetFolders().SequenceEqual(f2.GetFolders(), this);
    }

    public int GetHashCode(IFolder f) => (f.Name, f.TotalSize()).GetHashCode();
}

public static class Utils
{
    public static bool FileSystemComparer(IFileSystem fs1, IFileSystem fs2)
    {
        var root1 = fs1.GetFolder("/");
        var root2 = fs2.GetFolder("/");

        var files1 = root1.GetFiles();
        var files2 = root2.GetFiles();

        if(!files1.SequenceEqual(files2, new FileComparer()))
            return false;

        var folders1 = root1.GetFolders();
        var folders2 = root2.GetFolders();

        if(!folders1.SequenceEqual(folders2, new FolderComparer()))
            return false;

        return true;
    }

    public static List<IFolder> CreateSubFolders(IFolder folder, int count, string prefix="folder_", string suffix="")
    {
        var list = new List<IFolder>();
        
        for(int i = 0; i < count; i++)
            list.Add(folder.CreateFolder(prefix + i + suffix));
        
        return list;
    }

    public static List<IFile> CreateSubFiles(IFolder folder, IEnumerable<int> sizes, string prefix="file_", string suffix=".ext")
    {
        var list = new List<IFile>();
        
        foreach(var size in sizes.Select((Value, Index) => new {Value, Index}))
            list.Add(folder.CreateFile(prefix + size.Index + suffix, size.Value));
        
        return list;
    }

    public static string Show(this IFile file) => $"{file.Name} ({file.Size})";

    public static string Show(this IFolder folder)
    {
        var subFiles = folder.GetFiles();
        var subFolds = folder.GetFolders();
        
        string subFolderStrings = String.Join(
            '\n', 
            subFolds.Select(sf => 
                String.Join('\n', 
                    sf.Show()!.Split('\n').Select((s, i) => (i == 0 ? "|__" : "|  ") + s)
                )
            )
        );

        string subFilesStrings = String.Join(
            '\n', 
            subFiles.Select(sf => "|__" + sf.Show())
        );

        string subItemStrings = subFolderStrings + 
            (subFolderStrings.Length*subFilesStrings.Length > 0 ? "\n": "") + 
            subFilesStrings + "\n";
        
        return "/" + folder.Name + (subItemStrings.Length > 1 ? "\n": "") + subItemStrings;
    }

    public static string Show(this IFileSystem fs)
    {
        var root = fs.GetFolder("/");
        return root.Show();
    }
}