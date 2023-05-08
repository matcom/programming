namespace MatCom.Tester;
using filesystem;

public static class Test2
{
    // Testing GetFolder & GetFile Methods
    public static bool GetFolderAndFileTest(int seed, IFileSystem expected, IFileSystem output)
    {
        // Random para cantidad y tamanho de archivos
        Random random = new Random(seed);

        // Lista de los tamanhos de archivos a generar
        int hidden_cnt = 3;
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
            // Creamos varias carpetas anidadas de prueba
            foreach(var hf in Utils.CreateSubFolders(root, hidden_cnt, "hidden_folder_"))
                foreach(var hsf in Utils.CreateSubFolders(hf, hidden_cnt, "hidden_subfolder_"))
                    Utils.CreateSubFiles(hsf, Enumerable.Repeat(4, hidden_cnt), "hidden_subfile_", ".hidden");
        };

        // La corremos en ambos FileSystems
        routine0(expected);
        routine0(output);

        // System.Console.WriteLine(expected.Show());
        // System.Console.WriteLine(output.Show());

        // Verificamos si ambos matchean
        if(!Utils.FileSystemComparer(expected, output))
            return false;

        // Rutina 1 accediendo a todas las carpetas
        Func<IFileSystem, IEnumerable<IFolder>> routine1 = fs => 
        {
            var list = new List<IFolder>();
            
            // Carpetas simples
            list.Add(fs.GetFolder("/Music"));
            list.Add(fs.GetFolder("/Videos"));
            list.Add(fs.GetFolder("/Downloads"));

            // Carpetas ocultas
            for(int i = 0; i < hidden_cnt; i++)
            {
                list.Add(fs.GetFolder($"/hidden_folder_{i}"));
                for(int j = 0; j < hidden_cnt; j++)
                    list.Add(fs.GetFolder($"/hidden_folder_{i}/hidden_subfolder_{j}"));
            }

            return list;
        };

        // La corremos en ambos FileSystems
        var expectedFolders = routine1(expected);
        var outputFolders = routine1(output);

        // Verificamos si coinciden los resultados
        var folderComparer = new FolderComparer();
        if(!expectedFolders.SequenceEqual(outputFolders, folderComparer))
            return false;

        // Rutina 2 accediendo a todos los archivos
        Func<IFileSystem, IEnumerable<IFile>> routine2 = fs => 
        {
            var list = new List<IFile>();
            
            // Archivos del sistema
            for(int i = 0; i < system_files_cnt; i++)
                list.Add(fs.GetFile($"/system_{i}.dll"));

            // Archivos de musica
            for(int i = 0; i < music_cnt; i++)
                list.Add(fs.GetFile($"/Music/track_{i}.mp3"));

            // Archivos de videos
            for(int i = 0; i < videos_cnt; i++)
                list.Add(fs.GetFile($"/Videos/movie_{i}.avi"));

            // Archivos de descarga
            for(int i = 0; i < downloads_cnt; i++)
                list.Add(fs.GetFile($"/Downloads/chrome_{i}.temp"));

            // Archivos ocultas
            for(int i = 0; i < hidden_cnt; i++)
                for(int j = 0; j < hidden_cnt; j++)
                    for(int k = 0; k < hidden_cnt; k++)
                        list.Add(fs.GetFile($"/hidden_folder_{i}/hidden_subfolder_{j}/hidden_subfile_{k}.hidden"));

            return list;
        };

        // La corremos en ambos FileSystems
        var expectedFiles = routine2(expected);
        var outputFiles = routine2(output);

        // Verificamos si coinciden los resultados
        var fileComparer = new FileComparer();
        if(!expectedFiles.SequenceEqual(outputFiles, fileComparer))
            return false;
        
        return true;
    }
}