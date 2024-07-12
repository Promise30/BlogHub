using BloggingAPI.Data;
using BloggingAPI.Domain.Repository.Interface;

namespace BloggingAPI.Domain.Repository.Implementation
{
    public class RepositoryManager : IRepositoryManager
    {
        private ApplicationDbContext _applicationDbContext;
        private IPostRepository _postRepository;
        private ICommentRepository _commentRepository;

        public RepositoryManager(ApplicationDbContext applicationDbContext)
        {   
            _applicationDbContext = applicationDbContext;
        }
        public IPostRepository Post
        {
            get
            {
                if (_postRepository == null)
                {
                    _postRepository = new PostRepository(_applicationDbContext);
                }
                return _postRepository;
            }
        }
        public ICommentRepository Comment
        {
            get
            {
                if (_commentRepository == null)
                {
                    _commentRepository = new CommentRepository(_applicationDbContext);
                }

                return _commentRepository;
            }
        }
        
        public Task SaveAsync() => _applicationDbContext.SaveChangesAsync();
    }
}
