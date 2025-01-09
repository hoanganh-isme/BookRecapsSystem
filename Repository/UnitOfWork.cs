using BusinessObject.Data;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AppDbContext context;
        private readonly IAppealRepository appealRepository;
        private readonly IAuthorRepository authorRepository;
        private readonly IBookRepository bookRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly IRecapRepository recapRepository;
        private readonly IRecapVersionRepository recapVersionRepository;
        private readonly IHighlightRepository highlightRepository;
        private readonly IKeyIdeaRepository keyIdeaRepository;
        private readonly IPlayListItemRepository playListItemRepository;
        private readonly IPlayListRepository playListRepository;
        private readonly IReadingPositionRepository readingPositionRepository;
        private readonly IReviewNoteRepository reviewNoteRepository;
        private readonly IReviewRepository reviewRepository;
        private readonly ISubscriptionRepository subscriptionRepository;
        private readonly ISupportTicketRepository supportTicketRepository;
        private readonly IViewTrackingRepository viewTrackingRepository;
        private readonly IContractRepository contractRepository;
        private readonly IContractAttachmentRepository contractAttachmentRepository;
        private readonly IContributorPayoutRepository contributorPayoutRepository;
        private readonly IContributorWithdrawalRepository contributorWithdrawalRepository;
        private readonly IPublisherPayoutRepository publisherPayoutRepository;
        private readonly ISubscriptionPackageRepository subscriptionPackageRepository;
        private readonly ISystemSettingRepository systemSettingRepository;
        private readonly ITransactionRepository transactionRepository;
        private readonly IPublisherRepository publisherRepository;
        private readonly ILikeRepository likeRepository;
        private readonly IRecapEarningRepository recapEarningRepository;
        private readonly IBookEarningRepository bookEarningRepository;

        public UnitOfWork (AppDbContext context,
            IAppealRepository appealRepository,
            IAuthorRepository authorRepository, 
            IBookRepository bookRepository, 
            ICategoryRepository categoryRepository, 
            IRecapRepository recapRepository, 
            IRecapVersionRepository recapVersionRepository, 
            IHighlightRepository highlightRepository, 
            IKeyIdeaRepository keyIdeaRepository, 
            IPlayListItemRepository playListItemRepository, 
            IPlayListRepository playListRepository, 
            IReadingPositionRepository readingPositionRepository, 
            IReviewNoteRepository reviewNoteRepository, 
            IReviewRepository reviewRepository, 
            ISubscriptionRepository subscriptionRepository, 
            ISupportTicketRepository supportTicketRepository,  
            IViewTrackingRepository viewTrackingRepository,
            IContractRepository contractRepository,
            IContractAttachmentRepository contractAttachmentRepository,
            IContributorWithdrawalRepository contributorWithdrawalRepository,
            IContributorPayoutRepository contributorPayoutRepository,
            IPublisherPayoutRepository publisherPayoutRepository,
            ISubscriptionPackageRepository subscriptionPackageRepository,
            ISystemSettingRepository systemSettingRepository,
            ITransactionRepository transactionRepository,
            IPublisherRepository publisherRepository,
            ILikeRepository likeRepository,
            IRecapEarningRepository recapEarningRepository,
            IBookEarningRepository bookEarningRepository)
        {
            this.context = context;
            this.appealRepository = appealRepository;
            this.authorRepository = authorRepository;
            this.bookRepository = bookRepository;
            this.categoryRepository = categoryRepository;
            this.recapRepository = recapRepository;
            this.recapVersionRepository = recapVersionRepository;
            this.highlightRepository = highlightRepository;
            this.keyIdeaRepository = keyIdeaRepository;
            this.playListItemRepository = playListItemRepository;
            this.playListRepository = playListRepository;
            this.readingPositionRepository = readingPositionRepository;
            this.reviewNoteRepository = reviewNoteRepository;
            this.reviewRepository = reviewRepository;
            this.subscriptionRepository = subscriptionRepository;
            this.supportTicketRepository = supportTicketRepository;
            this.viewTrackingRepository = viewTrackingRepository;
            this.contractRepository = contractRepository;
            this.contractAttachmentRepository = contractAttachmentRepository;
            this.subscriptionPackageRepository = subscriptionPackageRepository;
            this.contributorPayoutRepository = contributorPayoutRepository;
            this.contributorWithdrawalRepository = contributorWithdrawalRepository;
            this.publisherPayoutRepository = publisherPayoutRepository;
            this.systemSettingRepository = systemSettingRepository;
            this.transactionRepository = transactionRepository;
            this.publisherRepository = publisherRepository;
            this.likeRepository = likeRepository;
            this.bookEarningRepository = bookEarningRepository;
            this.recapEarningRepository = recapEarningRepository;
        }

        //public GenericRepository<Department> DepartmentRepository
        //{
        //    get
        //    {

        //        if (this.departmentRepository == null)
        //        {
        //            this.departmentRepository = new GenericRepository<Department>(context);
        //        }
        //        return departmentRepository;
        //    }
        //}

        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                return await context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        private bool disposed = false;

        public IAppealRepository  AppealRepository => appealRepository;
        public IAuthorRepository  AuthorRepository => authorRepository;
        public IBookRepository BookRepository => bookRepository;
        public ICategoryRepository  CategoryRepository => categoryRepository;

        public IRecapRepository  RecapRepository => recapRepository;

        public IRecapVersionRepository  RecapVersionRepository => recapVersionRepository;
  
        public IHighlightRepository  HighlightRepository => highlightRepository;

        public IKeyIdeaRepository  KeyIdeaRepository => keyIdeaRepository;

        public IPlayListItemRepository  PlayListItemRepository => playListItemRepository;
 
        public IPlayListRepository  PlayListRepository => playListRepository;
 
        public IReadingPositionRepository  ReadingPositionRepository => readingPositionRepository;
    
        public IReviewNoteRepository  ReviewNoteRepository => reviewNoteRepository;
    
        public IReviewRepository  ReviewRepository => reviewRepository;
               
        public ISupportTicketRepository  SupportTicketRepository => supportTicketRepository;
        
        public IViewTrackingRepository  ViewTrackingRepository => viewTrackingRepository;
        public IContractRepository ContractRepository => contractRepository;

        public IContractAttachmentRepository ContractAttachmentRepository => contractAttachmentRepository;

        public IContributorWithdrawalRepository ContributorWithdrawalRepository => contributorWithdrawalRepository;

        public IContributorPayoutRepository ContributorPayoutRepository => contributorPayoutRepository;

        public IPublisherPayoutRepository PublisherPayoutRepository => publisherPayoutRepository;

        public ISubscriptionPackageRepository SubscriptionPackageRepository => subscriptionPackageRepository;

        public ISystemSettingRepository SystemSettingRepository => systemSettingRepository;
        public ITransactionRepository TransactionRepository => transactionRepository;   
        public ISubscriptionRepository SubscriptionRepository => subscriptionRepository;
        public IPublisherRepository PublisherRepository => publisherRepository; 
        public ILikeRepository LikeRepository => likeRepository;
        public IBookEarningRepository BookEarningRepository => bookEarningRepository;
        public IRecapEarningRepository RecapEarningRepository => recapEarningRepository;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}

