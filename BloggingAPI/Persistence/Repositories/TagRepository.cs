using BloggingAPI.Domain.Entities;
using BloggingAPI.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BloggingAPI.Persistence.Repositories
{
    public class TagRepository : BaseRepository<Tag>, ITagRepository
    {
        public TagRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
        public void CreateTag(Tag tag) => Create(tag);
        public void DeleteTag(Tag tag) => Delete(tag);

        public async Task<IEnumerable<Post?>> GetAllPostsForTagAsync(int tagId)
        {
            var tag = await FindByCondition(t=> t.TagId == tagId)
                                .Include(t=> t.PostLinks)
                                .ThenInclude(p=> p.Post)
                                .FirstOrDefaultAsync();
            return tag.PostLinks.Select(p => p.Post).ToList();
        }
        public  IEnumerable<Tag?> GetTagsByIds(List<int> tagsIds) => tagsIds.Select(tagId => FindByCondition(tag => tag.TagId == tagId).FirstOrDefault()).ToList(); 
        public async Task<IEnumerable<Tag>> GetAllTagsAsync() => await GetAll().ToListAsync();
        public async Task<Tag?> GetTagAsync(int tagId) => await FindByCondition(t => t.TagId == tagId).FirstOrDefaultAsync();
        public Tag? GetTagByName(string name) => FindByCondition(t => t.Name.ToLower() == name.ToLower()).FirstOrDefault();
        public void UpdateTag(Tag tag) => Update(tag);
        
    }
}
