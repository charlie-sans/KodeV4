using System;
using System.IO;
using Newtonsoft.Json;

namespace KodeRunner
{
    public class Config
    {
        public string Language { get; set; }
        public string Version { get; set; }
        public bool EnableLogging { get; set; }

        // the root directory of where the project folders are located
        public string ProjectRoot { get; set; }

        // the location of those generated files
        public string OutputDirectory { get; set; }
     

        private static Config instance;


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public Config() { 
            if (instance == null)
            {
                instance = this;
            }
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public static Config Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Configuration file not found.", filePath);
            }

            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<Config>(json);
        }

        public void Save(string filePath)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static Config GetInstance()
        {
            return instance;
        }
    }
}