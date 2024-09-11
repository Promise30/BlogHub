using BloggingAPI.Contracts.Dtos.Responses.Auth;
using BloggingAPI.Contracts.Dtos.Responses.Posts;
using BloggingAPI.Contracts.Dtos.Responses.Tags;
using BloggingAPI.Domain.Entities;

namespace BloggingAPI.Persistence.Extensions
{
    public static class MappingExtensions
    {
        public static UserResponseDto MapToUserResponseDto(this ApplicationUser user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneCountryCode = user.PhoneCountryCode,
                PhoneNumber = user.PhoneNumber,
                DateCreated = user.DateCreated,
                DateModified = user.DateModified,
            };
        }
        public static PostDetailDto ToPostDetailDto(this Post post)
        {
            return new PostDetailDto
            {
                Id = post.PostId,
                Title = post.Title,
                Content = post.Content,
                Author = post.Author,
                PostImageUrl = post.PostImageUrl,
                Tags = post.TagLinks.Select(t => t.Tag.Name).ToList(),
                PublishedOn = post.PublishedOn,
                Comments = post.Comment.Select(c=> new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    Author = c.Author,
                    UpVoteCount = c.UpVoteCount,
                    DownVoteCount = c.DownVoteCount,
                    PublishedOn = post.PublishedOn
                }).ToList(),

            };
        }
        public static PostDto ToPostDto(this Post post)
        {
            return new PostDto
            {
                Id = post.PostId,
                Title = post.Title,
                Content = post.Content,
                Author = post.Author,
                PostImageUrl = post.PostImageUrl,
                Tags = post.TagLinks.Select(t => t.Tag.Name).ToList(),
                PublishedOn = post.PublishedOn,
        
            };
        }
        public static NewPostDto ToNewlyCreatedPost(this Post newPost, IEnumerable<Tag> tags)
        {
            return new NewPostDto
            {
                Id = newPost.PostId,
                Title = newPost.Title,
                Content = newPost.Content,
                Author = newPost.Author,
                PostImageUrl = newPost.PostImageUrl,
                Tags = tags.Select(t => t.Name).ToList(),
                PublishedOn = newPost.PublishedOn,
                DateModified = newPost.DateModified,

            };
        }
        public static CommentDto ToCommentDto(this Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                Author = comment.Author,
                PublishedOn = comment.PublishedOn,
                UpVoteCount = comment.UpVoteCount,
                DownVoteCount  = comment.DownVoteCount,
            };
        }
        public static TagDto ToTagDto(this Tag tag)
        {
            return new TagDto
            {
                Id = tag.TagId,
                Name = tag.Name,
            };
        
        }

    }

}
