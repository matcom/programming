namespace MatCom.Exam;
using filesystem;

public static class Wrapper
{
    public static IFileSystem CreateFileSystem(int input) => Exam.CreateFileSystem();

    public static string Nombre => Exam.Nombre;

    public static string Grupo => Exam.Grupo;
}