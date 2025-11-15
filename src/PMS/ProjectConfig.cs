using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
namespace KodeRunner.PMS
{
    public class ProjectConfig
    {
        /// <summary>
        /// The name of the project
        /// </summary>
        public string ProjectName { get; set; }
        /// <summary>
        /// The compiler to use, e.g. "csc", "vbc"
        /// </summary>
        public string Compiler { get; set; }
        /// <summary>
        /// Additional compiler options, e.g. ["-optimize", "-debug"]
        /// </summary>
        public string[] CompilerOptions { get; set; }
        /// <summary>
        /// Whether to run the project after building
        /// </summary>
        public bool RunOnBuild { get; set; }
        /// <summary>
        /// The output directory for build artifacts
        /// </summary>
        public string OutputDirectory { get; set; }

        public void SaveConfig(string FilePath)
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        public static ProjectConfig LoadConfig(string FilePath)
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<ProjectConfig>(json);
        }
    }
}