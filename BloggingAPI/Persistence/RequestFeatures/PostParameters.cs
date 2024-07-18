namespace BloggingAPI.Persistence.RequestFeatures
{
    public class PostParameters : RequestParameters
    {
        public PostParameters() => OrderBy = "title";
        public DateOnly StartDate { get; set; } = DateOnly.MinValue;
        public DateOnly EndDate { get; set; } = DateOnly.MaxValue;
        public string? Category { get; set; }
        public string? SearchTerm { get; set; }
    }
}
