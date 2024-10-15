namespace FileShareClientIntegrationTests.Models
{
    public class SearchAsyncModel
    {
        public required string SearchQuery { get; set; }
        public required int PageSize { get; set; }
        public required int Start { get; set; }
    }
}
