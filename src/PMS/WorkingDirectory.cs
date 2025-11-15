using System.IO;
using System.Text.Json;
namespace KodeRunner
{
    public class WorkingDirectory
    {
        public string[] Projects = Directory.GetDirectories(Config.GetInstance().ProjectRoot);
        public bool CreateProject(string ProjectName)
        {
            return true;
        }
    }
}