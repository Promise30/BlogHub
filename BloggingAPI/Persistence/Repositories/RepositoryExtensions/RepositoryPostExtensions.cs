using BloggingAPI.Domain.Entities;
using System.Reflection;
using System.Text;
using System.Linq.Dynamic.Core;

using System.Globalization;
using Microsoft.EntityFrameworkCore;
using BloggingAPI.Domain.Enums;


namespace BloggingAPI.Persistence.Repositories.RepositoryExtensions
{
    public static class RepositoryPostExtensions
    {
        public static IQueryable<Post> FilterPostsByDatePublished(this IQueryable<Post> posts, DateTime startDate, DateTime endDate) => 
               posts.Where(c => c.PublishedOn >= startDate && c.PublishedOn <= endDate);
        public static IQueryable<Post> FilterPostsByTag(this IQueryable<Post> posts, string? tagName)
        {
            if (string.IsNullOrEmpty(tagName))
                return posts;
            return posts.Where(p=> p.TagLinks.Any(pt=> pt.Tag.Name.ToLower() == tagName.ToLower()));
        }
        public static IQueryable<Post> Search(this IQueryable<Post> posts, string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return posts;
            var lowerCaseTerm = searchTerm.Trim().ToLower();
            return posts.Where(p => p.Title.ToLower().Contains(lowerCaseTerm) || p.Content.ToLower().Contains(lowerCaseTerm));
           
        }
        public static IQueryable<Post> Sort(this IQueryable<Post> Posts, string? orderByQueryString)
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
