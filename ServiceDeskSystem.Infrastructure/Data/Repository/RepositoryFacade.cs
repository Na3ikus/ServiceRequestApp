using Microsoft.EntityFrameworkCore;

namespace ServiceDeskSystem.Infrastructure.Data.Repository
{
    public sealed class RepositoryFacade : IAsyncDisposable
    {
        private readonly BugTrackerDbContext _context;
        private readonly Lazy<TicketRepository> _ticketRepository;
        private readonly Lazy<CommentRepository> _commentRepository;
        private readonly Lazy<TechStackRepository> _techStackRepository;
        private readonly Lazy<ProductRepository> _productRepository;
        private readonly Lazy<UserRepository> _userRepository;

        public RepositoryFacade(IDbContextFactory<BugTrackerDbContext> contextFactory)
        {
            this._context = contextFactory.CreateDbContext();

            this._ticketRepository = new Lazy<TicketRepository>(() => new TicketRepository(this._context));
            this._commentRepository = new Lazy<CommentRepository>(() => new CommentRepository(this._context));
            this._techStackRepository = new Lazy<TechStackRepository>(() => new TechStackRepository(this._context));
            this._productRepository = new Lazy<ProductRepository>(() => new ProductRepository(this._context));
            this._userRepository = new Lazy<UserRepository>(() => new UserRepository(this._context));
        }

        public TicketRepository Tickets => this._ticketRepository.Value;
        public CommentRepository Comments => this._commentRepository.Value;
        public TechStackRepository TechStacks => this._techStackRepository.Value;
        public ProductRepository Products => this._productRepository.Value;
        public UserRepository Users => this._userRepository.Value;

        public async Task<int> SaveChangesAsync()
        {
            return await this._context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await this._context.DisposeAsync().ConfigureAwait(false);
        }
    }
}
