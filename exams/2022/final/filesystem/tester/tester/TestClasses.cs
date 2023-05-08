namespace MatCom.Tester;
using filesystem;

public class MyFile: IFile
{
    public string Name { get; private set; }

    public int Size { get; private set; }

    public MyFile(string name, int size)
    {
        Name = name;
        Size = size;
    }

    public MyFile Clone() => new MyFile(Name, Size);
}

public class MyFolder: IFolder
{
    public List<MyFile> Files { get; private set; }

    public List<MyFolder> Folders { get; private set; }

    public string Name { get; private set; }

    public MyFolder(string name)
    {
        Name = name;
        Folders = new List<MyFolder>();
        Files = new List<MyFile>();
    }

    public MyFolder(string name, IEnumerable<MyFile> files, IEnumerable<MyFolder> folders)
    {
        Name = name;
        Files = files.ToList();
        Folders = folders.ToList();
    }

    public IFile CreateFile(string name, int size)
    {
        if(Files.Any(f => f.Name == name))
            throw new Exception("File already exists!");

        MyFile newFile = new MyFile(name, size);

        Files.Add(newFile);

        return newFile;
    }

    public IFolder CreateFolder(string name)
    {
        if(Folders.Any(f => f.Name == name))
            throw new Exception("Folder already exists!");

        MyFolder newFolder = new MyFolder(name);

        Folders.Add(newFolder);

        return newFolder;
    }

    public IEnumerable<IFile> GetFiles() => Files.OrderBy(f => f.Name);

    public IEnumerable<IFolder> GetFolders() => Folders.OrderBy(f => f.Name);

    public int TotalSize() => Files.Sum(f => f.Size) + Folders.Sum(f => f.TotalSize());

    public void Delete(string name)
    {
        var file = Files.Find(f => f.Name == name);

        if(file != null)
            Files.Remove(file);
        else
        {
            var folder = Folders.Find(f => f.Name == name);

            if(folder != null)
                Folders.Remove(folder);
        }
    }

    public MyFolder Clone() => new MyFolder(Name, Files.Select(f => f.Clone()), Folders.Select(f => f.Clone()));

    public void Merge(MyFolder folder)
    {
        foreach(var newFile in folder.Files)
        {
            var oldFile = Files.Find(f => f.Name == newFile.Name);
            if(oldFile != null)
                Files.Remove(oldFile);
            Files.Add(newFile);
        }

        foreach(var newFolder in folder.Folders)
        {
            var oldFolder = Folders.Find(f => f.Name == newFolder.Name);
            if(oldFolder != null)
                oldFolder.Merge(newFolder);
            else
                Folders.Add(newFolder);
        }
    }
}

public class MyFileSystem: IFileSystem
{
    private MyFolder Root;

    public MyFileSystem() => Root = new MyFolder("");

    public MyFileSystem(MyFolder root) => Root = root;

    private MyFolder FolderAt(string path)
    {
        MyFolder current = Root;
        string[] names = path.TrimEnd('/').Split('/');
        for(int i = 1; i < names.Length; i++)
        {
            try
            {
                current = current.Folders.First(f => f.Name == names[i]);
            }
            catch
            {
                throw new Exception("Path is invalid!");
            }
        }

        return current;
    }

    public IFolder GetFolder(string path) => FolderAt(path);

    private MyFile FileAt(string path)
    {
        MyFolder current = Root;
        string[] names = path.Split('/');
        for(int i = 1; i < names.Length - 1; i++)
        {
            try
            {
                current = current.Folders.First(f => f.Name == names[i]);
            }
            catch
            {
                throw new Exception("Path is invalid!");
            }
        }

        try
        {
            return current.Files.First(f => f.Name == names[names.Length - 1]);
        }
        catch
        {
            throw new Exception("Ths file does not exists!");
        }
    }

    public IFile GetFile(string path) => FileAt(path);

    public IFileSystem GetRoot(string path) => new MyFileSystem(FolderAt(path));

    public IEnumerable<IFile> Find(FileFilter filter)
    {
        foreach(var file in Root.GetFiles())
            if(filter(file))
                yield return file;

        foreach(var folder in Root.GetFolders())
            foreach(var file in GetRoot("/" + folder.Name).Find(filter))
                yield return file;
    }

    public void Copy(string origin, string destination)
    {
        var destFolder = FolderAt(destination);
        try
        {
            var cloneOrigin = FileAt(origin).Clone();
            var wrapper = new MyFolder(destFolder.Name, new List<MyFile>(){cloneOrigin}, new List<MyFolder>());
            destFolder.Merge(wrapper);
        }
        catch
        {
            var cloneOrigin = FolderAt(origin).Clone();
            var wrapper = new MyFolder(destFolder.Name, new List<MyFile>(), new List<MyFolder>(){cloneOrigin});
            destFolder.Merge(wrapper);
        }
    }
    
    public void Move(string origin, string destination)
    {
        Copy(origin, destination);
        Delete(origin);
    }
    
    public void Delete(string path)
    {
        var splitedPath = path.Split('/').ToList();

        string end = splitedPath.Last();
        splitedPath.RemoveAt(splitedPath.Count - 1);

        string newPath = String.Join('/', splitedPath);
        var Folder = FolderAt(newPath);
        Folder.Delete(end);
    }
}