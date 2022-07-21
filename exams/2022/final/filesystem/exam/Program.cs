using System.Diagnostics;
using filesystem;

class Program
{
    static void Main()
    {
        // Esto está aquí para que no te olvides de implementarlo
        Console.WriteLine($"{Exam.Nombre} - {Exam.Grupo}");

        // Creando un sistema de ficheros vacío
        var fs = Exam.CreateFileSystem();

        // Creando un par de carpetas en la raíz
        var root = fs.GetFolder("/");
        var home = root.CreateFolder("home");
        var tmp = root.CreateFolder("tmp");

        // Creando 10 archivos dentro de la carpeta `tmp`
        for (int i = 0; i < 10; i++)
            tmp.CreateFile($"file{i}.tmp", 10);

        // Verificando el tamaño de `tmp`
        Debug.Assert(tmp.TotalSize() == 100);

        // Creando archivos en `home`
        home.CreateFile("picture.png", 20);
        home.CreateFile("document.docx", 150);
        home.CreateFile("virus.exe", 300);

        // Buscando un archivo concreto
        var virusFile = fs.GetFile("/home/virus.exe");
        Debug.Assert(virusFile.Name == "virus.exe");

        // Verificando el método `Find` con archivos grandes
        foreach (var file in fs.Find(file => file.Size > 50))
            Debug.Assert(file.Size > 50);

        // Verificando el método `Find` con nombres
        foreach (var file in fs.Find(file => file.Name.EndsWith(".png")))
            Debug.Assert(file.Name == "picture.png");

        // Ahora vamos a copiar `/tmp` para `/home` y verificar los tamaños
        fs.Copy("/tmp", "/home");
        Debug.Assert(home.TotalSize() == 570);
        Debug.Assert(fs.GetFolder("/tmp").TotalSize() ==
                     fs.GetFolder("/home/tmp").TotalSize());

        // Añade tus pruebas aquí
        // ...
    }
}
