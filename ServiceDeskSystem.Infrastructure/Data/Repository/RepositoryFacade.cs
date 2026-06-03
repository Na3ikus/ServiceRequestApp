using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Infrastructure.Data.Repository
{
    public sealed class RepositoryFacade : IRepositoryFacade
    {
        private readonly BugTrackerDbContext _context;
        private readonly Lazy<TicketRepository> _ticketRepository;
        private readonly Lazy<CommentRepository> _commentRepository;
        private readonly Lazy<TechStackRepository> _techStackRepository;
        private readonly Lazy<ProductRepository> _productRepository;
        private readonly Lazy<UserRepository> _userRepository;
        private readonly Lazy<PersonRepository> _personRepository;
        private readonly Lazy<ContactTypeRepository> _contactTypeRepository;
        private readonly Lazy<ContactInfoRepository> _contactInfoRepository;
        private readonly Lazy<AuditLogRepository> _auditLogRepository;
        private readonly Lazy<NotificationRepository> _notificationRepository;

        public RepositoryFacade(IDbContextFactory<BugTrackerDbContext> contextFactory)
        {
            this._context = contextFactory.CreateDbContext();

            this._ticketRepository = new Lazy<TicketRepository>(() => new TicketRepository(this._context));
            this._commentRepository = new Lazy<CommentRepository>(() => new CommentRepository(this._context));
            this._techStackRepository = new Lazy<TechStackRepository>(() => new TechStackRepository(this._context));
            this._productRepository = new Lazy<ProductRepository>(() => new ProductRepository(this._context));
            this._userRepository = new Lazy<UserRepository>(() => new UserRepository(this._context));
            this._personRepository = new Lazy<PersonRepository>(() => new PersonRepository(this._context));
            this._contactTypeRepository = new Lazy<ContactTypeRepository>(() => new ContactTypeRepository(this._context));
            this._contactInfoRepository = new Lazy<ContactInfoRepository>(() => new ContactInfoRepository(this._context));
            this._auditLogRepository = new Lazy<AuditLogRepository>(() => new AuditLogRepository(this._context));
            this._notificationRepository = new Lazy<NotificationRepository>(() => new NotificationRepository(this._context));
            this.UnitOfWork = new ServiceDeskSystem.Infrastructure.Data.UnitOfWork(this._context);
        }

        public ITicketRepository Tickets => this._ticketRepository.Value;
        public ICommentRepository Comments => this._commentRepository.Value;
        public ITechStackRepository TechStacks => this._techStackRepository.Value;
        public IProductRepository Products => this._productRepository.Value;
        public IUserRepository Users => this._userRepository.Value;
        public IPersonRepository People => this._personRepository.Value;
        public IContactTypeRepository ContactTypes => this._contactTypeRepository.Value;
        public IContactInfoRepository ContactInfos => this._contactInfoRepository.Value;
        public IAuditLogRepository AuditLogs => this._auditLogRepository.Value;
        public INotificationRepository Notifications => this._notificationRepository.Value;
        public IUnitOfWork UnitOfWork { get; }

        public async ValueTask DisposeAsync()
        {
            await this._context.DisposeAsync().ConfigureAwait(false);
        }
    }
}

