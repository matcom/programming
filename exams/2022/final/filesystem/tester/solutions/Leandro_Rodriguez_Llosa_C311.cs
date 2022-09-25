
using System.Text;
using filesystem;

namespace MatCom.Exam;

public class Exam
{
    public static IFileSystem CreateFileSystem()
    {
        return new FileSystem();
    }

    public static string Nombre => "Leandro Rodriguez Llosa";

    public static string Grupo => "C311";
}

public class FileSystem : IFileSystem
{
    IFolder root; // Root directory

    public FileSystem(IFolder? root = null)
    {
        this.root = root is null? new Folder("") : root;
    }

    public void Copy(string origin, string destination)
    {
        // Separates the path in the root folder and the name of the folder or file
        var (rootFolderPath, fileOrFolderName) = Utils.SeparateByRootFolder(origin);

        IFile? originFile = null;
        var rootFolder = GetFolder(rootFolderPath); // Gets the root folder
        var destinationFolder = GetFolder(destination); // Gets the destination folder
            
        // If the origin path matches a file, get it
        foreach (var file in rootFolder.GetFiles())
        {
            if (file.Name == fileOrFolderName)
                originFile = file;
        }

        // If the file exists copy it
        if (originFile != null)
        {
            destinationFolder.Copy(originFile);
            return;
        }

        IFolder? originFolder = null;

        // If the origin path matches a folder, get it
        foreach (var folder in rootFolder.GetFolders())
        {
            if (folder.Name == fileOrFolderName)
                originFolder = folder;
        }

        // If the folder exists copy it
        if (originFolder != null)
        {
            destinationFolder.Copy(originFolder);
            return;
        }

        // At this point there is definitely no folder or file corresponding to this path, 
        // it throws an exception
        throw new Exception($"Does not exist any file or folder named {fileOrFolderName}");
    }

    public void Delete(string path)
    {
        ((Folder)this.root).Delete(path);
    }

    public IEnumerable<IFile> Find(FileFilter filter)
    {
        var currentFolder = this.root;

        // To do a correct traversal in preorder, the files that are in the current folder
        // are checked first
        foreach (var file in currentFolder.GetFiles())
        { 
            if (filter(file))
                yield return file;
        }

        // Then the folders
        foreach (var folder in currentFolder.GetFolders()) 
        {
            // Recursive call to the Find method for each folder
            foreach (var file in new FileSystem(folder).Find(filter))
                yield return file;
        } 
    }

    public IFile GetFile(string path)
    {
        var (rootFolder, fileName) = Utils.SeparateByRootFolder(path);

        var file = GetFolder(rootFolder)
            .GetFiles()
            .Where(file => file.Name == fileName)
            .FirstOrDefault();

        if (file == null)
            throw new Exception("File does not exist");

        return file;
}
    
    public IFolder GetFolder(string path)
    {   
        // The idea is to move through the folders, for that the route will be separated by folders. 
        string[] parsedPath = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        IFolder? currentFolder = this.root;

        // They are then traversed going from the current folder to the next in the path. 
        foreach (var folderName in parsedPath)
        {
            currentFolder = currentFolder.GetFolders() 
                .Where(folder => folder.Name == folderName)
                .FirstOrDefault();

            // If not found, an exception is thrown.
            if (currentFolder == null)
                throw new Exception($"Folder `{folderName}` does not exist");
        }

        return currentFolder;
    }

    public IFileSystem GetRoot(string path)
    {
        this.root = (Folder)GetFolder(path);
        return this;
    }

    public void Move(string origin, string destination)
    {
        Copy(origin, destination);
        Delete(origin);
    }
}

public class File : IFile
{
    public File(string name, int size)
    {
        this.Name = name;
        this.Size = size;
    }

    public int Size { get; set; }

    public string Name { get; set; }
}

public class Folder : IFolder
{
    List<Folder> folders;
    List<File> files;

    public Folder(string name)
    {
        this.Name = name;
        this.files = new List<File>();
        this.folders = new List<Folder>();
    }

    public string Name { get; set; }

    public IFile CreateFile(string name, int size)
    {
        // If the file already exists, an exception is thrown
        if (this.files.Where(file => file.Name == name).Any())
            throw new Exception("File already exists");

        var newFile = new File(name, size);
        this.files.Add(newFile);

        return newFile;
    }

    public IFolder CreateFolder(string name)
    {
        // If the folder already exists, an exception is thrown
        if (this.folders.Where(folder => folder.Name == name).Any())
            throw new Exception("Folder already exists");

        var newFolder = new Folder(name);
        this.folders.Add(newFolder);

        return newFolder;
    }

    public IEnumerable<IFile> GetFiles()
    {
        // Returns the files in the current folder in lexicographical order
        return this.files.OrderBy(f => f.Name);
    }

    public IEnumerable<IFolder> GetFolders()
    {
        // Returns the files in the current folder in lexicographical order
        return this.folders.OrderBy(f => f.Name);
    }

    public int TotalSize()
    {
        int totalSize = 0;

        foreach (var folder in this.folders)
        {
            totalSize += folder.TotalSize();   
        }

        foreach (var file in this.files)
        {
            totalSize += file.Size;   
        }

        return totalSize;
    }

    public void Delete(string path)
    {
        // The idea is repetitive, we have a path that we want to travel, for that we divide it 
        // by the name of its subfolders. 
        string[] parsedPath = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        if (parsedPath.Length == 1)
        {
            // Once the last folder or file name of the path is reached, we delete the folder or file from it. 
            int removed = this.files.RemoveAll(file => file.Name == parsedPath[0]);
            removed += this.folders.RemoveAll(folder => folder.Name == parsedPath[0]);
            
            // In case there is none, we throw an exception.
            if (removed == 0)
                throw new Exception($"File or folder named {parsedPath[0]} does not exist");
            
            return;
        }

        // Then we stop at the first folder and delete the same file from it.
        var folder = this.folders
            .Where(f => f.Name == parsedPath[0])
            .FirstOrDefault();

        // If that folder does not exist, we throw an exception.
        if (folder == null)
            throw new Exception($"Folder {parsedPath[0]} does not exist");
        
        folder.Delete
        (
            path.Substring(parsedPath[0].Length + 1)
        );   
    }
}

// The fundamental idea with these extender methods was to make life easier when copying 
// files or folders to a given folder.
public static class FolderExts
{
    public static void Copy(this IFolder destinationFolder, IFolder sourceFolder)
    {
        // The first thing is to check if there is a folder with the same name in the  
        // destination path, in that case we take it. 
        var newFolder = destinationFolder
            .GetFolders()
            .Where(folder => folder.Name == sourceFolder.Name)
            .FirstOrDefault(); 

        // If the folder is not found, we create a new one.
        if (newFolder == null)
            newFolder = destinationFolder.CreateFolder(sourceFolder.Name);

        /* 
            Let's take some time to discuss how to handle the problem of mixing folders or 
            files with the same name.
            
            In the case of files, the extension method for copying files to a folder handles 
            it correctly (more on that later).
            
            In the case of folders, as successive recursive calls are made, from the new folder 
            to copy, located in the destination path, and the first instruction that this method 
            does is to search if there is a folder with the same name, this allows descending 
            one level below the file system (we can think of it as a tree) and successfully copy 
            the files and folders one by one. And as you go down the tree, combinations of files 
            and folders with the same name are resolved recursively. 
        */

        foreach (var file in sourceFolder.GetFiles())
        {
            newFolder.Copy(file);
        }        

        foreach (var folder in sourceFolder.GetFolders())
        {
            newFolder.Copy(folder);
        }
    }

    public static void Copy(this IFolder destinationFolder, IFile sourceFile)
    {
        // First we check if a file with the same name exists in the folder
        var newFile = destinationFolder
            .GetFiles()
            .Where(file => file.Name == sourceFile.Name)
            .FirstOrDefault(); 

        // If the file is not found, we create a new one.
        if (newFile == null)
            newFile = destinationFolder.CreateFile(sourceFile.Name, sourceFile.Size);

        // If the file exists, the way to overwrite it is to resize it to match 
        // the size of the file you want to copy.
        var file = (File)newFile;
        file!.Size = sourceFile.Size;
    } 
}

public static class Utils
{
    public static (string,string) SeparateByRootFolder(string path)
    {
        /*
            The idea of ​​this method is to split the path into the root folder path and 
            the final file or folder name. For this we look for the index of the last 
            character ´/´ and using the substring function the Path is divided.
        */
        
        int i = path.Length - 1;

        while (i >= 0 && path[i] != '/') { i--; }  

        string newPath = path.Substring(0, i);
        string fileName = path.Substring(i + 1);

        return (newPath, fileName);
    }
}
