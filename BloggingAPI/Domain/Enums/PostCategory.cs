using System.ComponentModel;

namespace BloggingAPI.Domain.Enums
{
    public enum PostCategory : int
    {
        [Description("Entertainment")]
        Entertainment = 0,
        [Description("Technology")]
        Technology = 1,
        [Description("Science")]
        Science = 2,
        [Description("Fashion")]
        Fashion = 3,
        [Description("Arts")]
        Arts = 4,
        [Description("Sports")]
        Sports = 5,
        [Description("News")]
        News  = 6,
        [Description("Health")]
        Health = 7,
        [Description("Travel")]
        Travel = 8,
        [Description("Food")]
        Food = 9,
        [Description("Lifestyle")]
        Lifestyle = 10,
        [Description("Education")]
        Education = 11,
        [Description("Politics")]
        Politics = 12,


    }
}
