using BloggingAPI.Contracts.Dtos.Responses;
using BloggingAPI.Contracts.Dtos.Responses.Posts;
using BloggingAPI.Domain.Entities;

namespace BloggingAPI.Persistence.Extensions
{
    public static class MappingExtensions
    {
        public static UserResponseDto MapToUserResponseDto(this User user, IEnumerable<string> roles)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = $"{user.PhoneCountryCode} {user.PhoneNumber}",
                Roles = roles.ToList(),
                DateCreated = user.DateCreated,
                DateModified = user.DateModified,
            };
        }
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
                    UpVoteCount = c.UpVoteCount,
                    DownVoteCount = c.DownVoteCount,
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
                PublishedOn = post.PublishedOn,
                Category = post.Category.ToString(),

            };

        }

    }

}
