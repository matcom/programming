namespace MatCom.Tester;
using filesystem;

public static class Test6
{
    // Testing Copy 2
    public static bool CopyTest2(int seed, IFileSystem expected, IFileSystem output)
    {
        // Random para cantidad y tamanho de archivos
        Random random = new Random(seed);

        // Lista de los tamanhos de archivos a generar
        // System
        int system_files_cnt = random.Next(1, 21);
        List<int> system_files_sizes = new List<int>();
        for(int i = 0; i < system_files_cnt; i++)
            system_files_sizes.Add(random.Next(1, 1000));
        // Downloads
        int downloads_cnt = random.Next(1, 21);
        List<int> downloads_sizes = new List<int>();
        for(int i = 0; i < downloads_cnt; i++)
            downloads_sizes.Add(random.Next(1, 1000));
        // Music
        int music_cnt = random.Next(1, 21);
        List<int> music_sizes = new List<int>();
        for(int i = 0; i < music_cnt; i++)
            music_sizes.Add(random.Next(1, 1000));
        // Videos
        int videos_cnt = random.Next(1, 21);
        List<int> videos_sizes = new List<int>();
        for(int i = 0; i < videos_cnt; i++)
            videos_sizes.Add(random.Next(1, 1000));

        // Rutina base
        Action<IFileSystem> routine0 = fs => 
        {
            // Obtenemos el directorio raiz
            var root = fs.GetFolder("/");
            // Creamos algunas carpetas simples
            var music = root.CreateFolder("Music");
            var videos = root.CreateFolder("Videos");
            var downloads = root.CreateFolder("Downloads");
            // Creamos varios archivos y carpetas
            Utils.CreateSubFiles(root, system_files_sizes, "system_", ".dll");
            Utils.CreateSubFiles(downloads, downloads_sizes, "chrome_", ".temp");
            Utils.CreateSubFiles(music, music_sizes, "track_", ".mp3");
            Utils.CreateSubFiles(videos, videos_sizes, "movie_", ".avi");
            // Creamos varios archivos a sobreescribir en el futuro
            downloads.CreateFile("system_0.dll", 0);
            music.CreateFile("system_0.dll", 0);
            videos.CreateFile("system_0.dll", 0);
            root.CreateFile("chrome_0.temp", 0);
            root.CreateFile("track_0.mp3", 0);
            root.CreateFile("movie_0.avi", 0);
            // Creamos carpetas para sobreescribir en el furuto
            var music_in_downloads = downloads.CreateFolder("Music");
            Utils.CreateSubFiles(music_in_downloads, Enumerable.Repeat(0, 2 * music_cnt), "track_", ".mp3");
            var videos_in_music = music.CreateFolder("Videos");
            Utils.CreateSubFiles(videos_in_music, Enumerable.Repeat(0, 2 * videos_cnt), "movie_", ".avi");
        };

        // La corremos en ambos FileSystems
        routine0(expected);
        routine0(output);

        // System.Console.WriteLine(expected.Show());
        // System.Console.WriteLine(output.Show());

        // Verificamos si ambos matchean
        if(!Utils.FileSystemComparer(expected, output))
            return false;

        // Rutina 1 sobreecribiendo archivos
        Action<IFileSystem> routine1 = fs => 
        {
            fs.Copy("/system_0.dll", "/Downloads");
            fs.Copy("/system_0.dll", "/Music");
            fs.Copy("/system_0.dll", "/Videos");

            fs.Copy("/Downloads/chrome_0.temp", "/");
            fs.Copy("/Music/track_0.mp3", "/");
            fs.Copy("/Videos/movie_0.avi", "/");
        };

        // La corremos en ambos FileSystems
        routine1(expected);
        routine1(output);

        // System.Console.WriteLine(expected.Show());
        // System.Console.WriteLine(output.Show());

        // Verificamos si ambos matchean
        if(!Utils.FileSystemComparer(expected, output))
            return false;

        // Rutina 2 sobreecribiendo carpetas
        Action<IFileSystem> routine2 = fs => 
        {
            fs.Copy("/Music", "/Downloads");
            fs.Copy("/Videos", "/Downloads/Music");
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