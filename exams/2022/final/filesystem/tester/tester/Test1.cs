namespace MatCom.Tester;
using filesystem;

public static class Test1
{
    // Testing CrateFolder, CreateFile, GetFolders, GetFiles & TotalSize Methods
    public static bool CreateFolderAndFileTest(int seed, IFileSystem expected, IFileSystem output)
    {
        // Random para cantidad y tamanho de archivos
        Random random = new Random(seed);

        // Primera rutina
        Action<IFileSystem> routine1 = fs => 
        {
            // Obtenemos el directorio raiz
            var root = fs.GetFolder("/");
            // Creamos algunas carpetas simples
            root.CreateFolder("Music");
            root.CreateFolder("Videos");
            root.CreateFolder("Downloads");
        };

        // La corremos en ambos FileSystems
        routine1(expected);
        routine1(output);

        // System.Console.WriteLine(expected.Show());
        // System.Console.WriteLine(output.Show());

        // Verificamos si ambos matchean
        if(!Utils.FileSystemComparer(expected, output))
            return false;

        // Generando tamanhos para cada tipo de archivo
        // Sistema
        int system_files_cnt = random.Next(1, 21);
        List<int> system_files_sizes = new List<int>();
        while(system_files_cnt-- > 0)
            system_files_sizes.Add(random.Next(1, 1000));
        // Downloads
        int downloads_cnt = random.Next(1, 21);
        List<int> downloads_sizes = new List<int>();
        while(downloads_cnt-- > 0)
            downloads_sizes.Add(random.Next(1, 1000));
        // Music
        int music_cnt = random.Next(1, 21);
        List<int> music_sizes = new List<int>();
        while(music_cnt-- > 0)
            music_sizes.Add(random.Next(1, 1000));
        // Videos
        int videos_cnt = random.Next(1, 21);
        List<int> videos_sizes = new List<int>();
        while(videos_cnt-- > 0)
            videos_sizes.Add(random.Next(1, 1000));

        // Segunda rutina
        Action<IFileSystem> routine2 = fs => 
        {
            // Obtenemos el directorio raiz
            var root = fs.GetFolder("/");

            // Creamos varios archivos del sistema
            Utils.CreateSubFiles(root, system_files_sizes, "system_", ".dll");

            // Obtenemos las 3 carpetas anteriormente creadas
            var root_folders = root.GetFolders().ToList();
            // Las guardamos en variables
            var downloads = root_folders[0];
            var music = root_folders[1];
            var videos = root_folders[2];
            
            // Generamos los archivos para esas 3 carpetas
            Utils.CreateSubFiles(downloads, downloads_sizes, "chrome_", ".temp");
            Utils.CreateSubFiles(music, music_sizes, "track_", ".mp3");
            Utils.CreateSubFiles(videos, videos_sizes, "movie_", ".avi");
        };
        
        // La corremos en ambos FileSystems
        routine2(expected);
        routine2(output);

        // System.Console.WriteLine(expected.Show());
        // System.Console.WriteLine(output.Show());

        // Verificamos si ambos matchean
        if(!Utils.FileSystemComparer(expected, output))
            return false;

        return true;
    }
}