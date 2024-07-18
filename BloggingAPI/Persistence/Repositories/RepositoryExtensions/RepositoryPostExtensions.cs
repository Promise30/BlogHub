using BloggingAPI.Domain.Entities;
using System.Reflection;
using System.Text;
using System.Linq.Dynamic.Core;
using BloggingAPI.Domain.Enums;
using System.Globalization;
using Microsoft.EntityFrameworkCore;


namespace BloggingAPI.Persistence.Repositories.RepositoryExtensions
{
    public static class RepositoryPostExtensions
    {
        // Filter by date created
        public static IQueryable<Post> FilterPostsByDatePublished(this IQueryable<Post> posts, DateTime startDate, DateTime endDate) => posts.Where(c => c.PublishedOn >= startDate && c.PublishedOn <= endDate);
        // Filter by category
        public static IQueryable<Post> FilterPostsByCategory(this IQueryable<Post> posts, string category)
        {
            if (string.IsNullOrEmpty(category))
                return posts;

            var categoryValue = (PostCategory)Enum.Parse(typeof(PostCategory), category, true);
            return posts.Where(p => p.Category == categoryValue);
        }
        public static IQueryable<Post> Search(this IQueryable<Post> posts, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return posts;
            var lowerCaseTerm = searchTerm.Trim().ToLower();
            return posts.Where(p => p.Title.ToLower().Contains(lowerCaseTerm) || p.Content.Contains(searchTerm) || p.Author.ToLower().Contains(lowerCaseTerm));
            //var fullTextQuery = EF.Functions.FreeText(posts.GetFullTextQuery("Title", "Content", "Author"), searchTerm);
            //return posts.Where(fullTextQuery); 
        }
        public static IQueryable<Post> Sort(this IQueryable<Post> Posts, string orderByQueryString)
        {
            if (string.IsNullOrWhiteSpace(orderByQueryString))
                return Posts.OrderBy(e => e.Title);

            var orderParams = orderByQueryString.Trim().Split(',');
            var propertyInfos = typeof(Post).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var orderQueryBuilder = new StringBuilder();
            foreach (var param in orderParams)
            {
                if (string.IsNullOrWhiteSpace(param))
                    continue;
                var propertyFromQueryName = param.Split(" ")[0];
                var objectProperty = propertyInfos.FirstOrDefault(pi => pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));
                if (objectProperty == null)
                    continue;
                var direction = param.EndsWith(" desc") ? "descending" : "ascending";
                orderQueryBuilder.Append($"{objectProperty.Name.ToString()} {direction},");
            }
            var orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');
            if (string.IsNullOrWhiteSpace(orderQuery))
                return Posts.OrderBy(e => e.Title);
            return Posts.OrderBy(orderQuery);
        }

    }
}
