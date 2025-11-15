namespace KodeRunner;
using System.IO;
public interface ILang
{
    /// <summary>
    /// Initalizes a project based on it's name and project directory.
    /// </summary>
    /// <param name="name">The project Namme</param>
    /// <param name="projectDir">The top level Location for where the project shall be created </param>
    /// <returns>Root directory of project</returns>
    public Path init(string name, Path projectDir);

    public void Execute(Path executeDir);
}
