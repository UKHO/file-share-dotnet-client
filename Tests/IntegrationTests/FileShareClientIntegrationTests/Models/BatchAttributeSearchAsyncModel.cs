namespace FileShareClientIntegrationTests.Models
{
    public class BatchAttributeSearchAsyncModel
    {
        public required string SearchQuery { get; set; }
        public required int MaxAttributeValueCount { get; set; }
    }
}
