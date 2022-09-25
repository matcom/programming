namespace MatCom.Tester;
using filesystem;

public static class Test3
{
    // Testing Copy
    public static bool CopyTest(int seed, IFileSystem expected, IFileSystem output)
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
        };

        // La corremos en ambos FileSystems
        routine0(expected);
        routine0(output);

        // System.Console.WriteLine(expected.Show());
        // System.Console.WriteLine(output.Show());

        // Verificamos si ambos matchean
        if(!Utils.FileSystemComparer(expected, output))
            return false;

        // Rutina 1 copiando archivos
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

        // Rutina 2 copiando carpetas
        Action<IFileSystem> routine2 = fs => 
        {
            fs.Copy("/Music", "/Downloads");
            // Copiando archivo al origen para descartar la copia por referencia
            fs.Copy("/Downloads/chrome_0.temp", "/Music");
            // Copiando archivo al destino para descartar la copia por referencia
            fs.Copy("/Videos/movie_0.avi", "/Downloads/Music");
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