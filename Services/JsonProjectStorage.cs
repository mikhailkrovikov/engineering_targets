/// <summary>
/// Сервис сохранения и загрузки проектов в формате JSON
/// </summary>
using System.IO;
using EngineeringTargets.Models;
using Newtonsoft.Json;

namespace EngineeringTargets.Services
{
    public class JsonProjectStorage : IProjectStorage
    {
        public void Save(ProjectModel project, string filePath)
        {
            var json = JsonConvert.SerializeObject(project, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public ProjectModel Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<ProjectModel>(json) ?? new ProjectModel();
        }
    }
}
