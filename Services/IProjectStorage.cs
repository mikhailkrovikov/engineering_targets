using EngineeringTargets.Models;

namespace EngineeringTargets.Services
{
    public interface IProjectStorage
    {
        void Save(ProjectModel project, string filePath);
        ProjectModel Load(string filePath);
    }
}

