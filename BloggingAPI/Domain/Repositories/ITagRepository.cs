using BloggingAPI.Domain.Entities;

namespace BloggingAPI.Domain.Repositories
{
    public interface ITagRepository
    {
        void CreateTag(Tag tag);
        void UpdateTag(Tag tag);
        void DeleteTag(Tag tag);
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task<Tag?> GetTagAsync(int tagId);
        Task<IEnumerable<Post?>> GetAllPostsForTagAsync(int tagId);
        IEnumerable<Tag?> GetTagsByIds(List<int> tagsIds);
        Tag? GetTagByName(string name);
    }

}
