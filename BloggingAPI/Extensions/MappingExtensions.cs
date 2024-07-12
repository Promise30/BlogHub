using BloggingAPI.Domain.Entities;
using BloggingAPI.Domain.Entities.Dtos.Responses.Posts;

namespace BloggingAPI.Extensions
{
    public static class MappingExtensions
    {
        public static PostWithCommentsDto MapToPostDto(this Post post)
        {
            return new PostWithCommentsDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Author = post.Author,
                PostImageUrl = post.PostImageUrl,
                PublishedOn = post.PublishedOn.ToShortDateString(),
                Category = post.Category.ToString(),
                Comments = post.Comment.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    Author = c.Author,
                    PublishedOn = c.PublishedOn
                }).ToList(),
            };

        }
        public static PostOnlyDto MapToPostOnlyDto(this Post post)
        {
            return new PostOnlyDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Author = post.Author,
                PostImageUrl = post.PostImageUrl,
                PublishedOn = post.PublishedOn.ToShortDateString(),
                Category = post.Category.ToString(),

            };

        }

    }

}
