namespace BloggingAPI.Persistence.RequestFeatures
{
    public class CommentParameters : RequestParameters
    {
        private const string DefaultStartDateString = "0001-01-01";
        private const string DefaultEndDateString = "9999-12-31";

        private DateOnly? _startDate = DateOnly.Parse(DefaultStartDateString);
        private DateOnly? _endDate = DateOnly.Parse(DefaultEndDateString);

        public DateOnly? StartDate
        {
            get => _startDate;
            set => _startDate = value;
        }

        public DateOnly? EndDate
        {
            get => _endDate;
            set => _endDate = value;
        }

        public bool ValidDateRange => !StartDate.HasValue || !EndDate.HasValue || EndDate.Value > StartDate.Value;
        public string? SearchTerm { get; set; }

    }
}
