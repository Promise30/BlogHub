namespace BloggingAPI.Domain.Entities
{
    public class Tag
    {
        public int TagId { get; set; }
        public string Name { get; set; }
        public virtual ICollection<PostTag> PostLinks { get; set; } = new List<PostTag>();
    }
}
