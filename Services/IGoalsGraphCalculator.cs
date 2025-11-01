using EngineeringTargets.Models;

namespace EngineeringTargets.Services
{
    public interface IGoalsGraphCalculator
    {
        CalculationResult Calculate(ProjectModel project);
    }
}

