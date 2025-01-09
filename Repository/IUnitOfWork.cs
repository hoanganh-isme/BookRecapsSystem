using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IAppealRepository AppealRepository { get; }
        IAuthorRepository AuthorRepository { get; }
        IBookRepository BookRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        IRecapRepository RecapRepository { get; }
        IRecapVersionRepository RecapVersionRepository { get; }
        IHighlightRepository HighlightRepository { get; }
        IKeyIdeaRepository KeyIdeaRepository { get; }
        IPlayListItemRepository PlayListItemRepository { get; }
        IPlayListRepository PlayListRepository { get; }
        IReadingPositionRepository ReadingPositionRepository { get; }
        IReviewNoteRepository ReviewNoteRepository { get; }
        IReviewRepository ReviewRepository { get; }
        ISupportTicketRepository SupportTicketRepository { get; }
        IViewTrackingRepository ViewTrackingRepository { get; }
        IContractAttachmentRepository ContractAttachmentRepository { get; }
        IContractRepository ContractRepository { get; }
        IContributorWithdrawalRepository ContributorWithdrawalRepository { get; }
        IContributorPayoutRepository ContributorPayoutRepository { get; }
        IPublisherPayoutRepository PublisherPayoutRepository { get; }
        ISubscriptionRepository SubscriptionRepository { get; }
        ISubscriptionPackageRepository SubscriptionPackageRepository { get; }
        ISystemSettingRepository SystemSettingRepository { get; }
        ITransactionRepository TransactionRepository { get; }
        IPublisherRepository PublisherRepository { get; }
        ILikeRepository LikeRepository { get; }
        IRecapEarningRepository RecapEarningRepository { get; }
        IBookEarningRepository BookEarningRepository { get; }

        Task<bool> SaveChangesAsync();
    }
}
