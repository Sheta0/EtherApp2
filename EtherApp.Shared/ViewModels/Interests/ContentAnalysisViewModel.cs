namespace EtherApp.Shared.ViewModels.Interests
{
    public class ContentAnalysisViewModel
    {
        public string Content { get; set; } = string.Empty;
        public List<InterestScore> Results { get; set; } = new();
    }

}
