using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtherApp.Data.Services
{
    public interface IContentAnalysisService
    {
        Task<List<(int InterestId, double Score)>> AnalyzeContentAsync(string content);
    }
}
