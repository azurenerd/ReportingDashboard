using System.Threading.Tasks;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public interface IDataService
    {
        System.Threading.Tasks.Task<ProjectStatus> ReadProjectDataAsync();
    }
}