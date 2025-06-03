using EtherApp.Controllers;

namespace EtherApp.ViewModels.Interests
{
    public class ContentAnalysisViewModel
    {
        public string Content { get; set; } = string.Empty;
        public List<InterestScore> Results { get; set; } = new();
    }

}
