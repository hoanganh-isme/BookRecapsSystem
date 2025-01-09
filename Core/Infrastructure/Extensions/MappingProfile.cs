using AutoMapper;
using BusinessObject.Models;
using BusinessObject.ViewModels.Author;
using BusinessObject.ViewModels.Books;
using BusinessObject.ViewModels.Categories;
using BusinessObject.ViewModels.Recaps;
using BusinessObject.ViewModels.KeyIdea;
using BusinessObject.ViewModels.PlayLists;
using Core.Models.Personal;
using BusinessObject.ViewModels.Review;
using BusinessObject.ViewModels.Highlight;
using BusinessObject.ViewModels.Contents;
using BusinessObject.ViewModels.SystemSetting;
using BusinessObject.ViewModels.SupportTicket;
using BusinessObject.ViewModels.SubscriptionPackage;
using BusinessObject.ViewModels.Contracts;
using BusinessObject.ViewModels.ContractAttachments;
using BusinessObject.ViewModels.Subscription;
using BusinessObject.ViewModels.Viewtrackings;
using BusinessObject.ViewModels.ReviewNotes;
using BusinessObject.ViewModels.Appeals;
using BusinessObject.ViewModels.Publisher;
using static BusinessObject.ViewModels.ContributorPayouts.ContributorPayoutDTO;
using static BusinessObject.ViewModels.PublisherPayouts.PublisherPayoutDTO;
using BusinessObject.ViewModels.Withdrawal;

namespace Core.Infrastructure.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<Publisher, PublisherDto>().ReverseMap();
            CreateMap<User, UserDetailsDto>().ReverseMap();
            CreateMap<User, ContributorDto>().ReverseMap();
            CreateMap<User, ContributorWithdrawalDTO>().ReverseMap();
            CreateMap<User, ContributorWithdrawalDTO>()
    .ForMember(dest => dest.contributorId, opt => opt.MapFrom(src => src.Id)) // Map User.Id -> ContributorDto.Id
    .ForMember(dest => dest.ContributorName, opt => opt.MapFrom(src => src.FullName));
            CreateMap<User, ContributorDto>()
    .ForMember(dest => dest.contributorId, opt => opt.MapFrom(src => src.Id)) // Map User.Id -> ContributorDto.Id
    .ForMember(dest => dest.ContributorName, opt => opt.MapFrom(src => src.FullName));

            CreateMap<Subscription, ViewSubscription>();
            CreateMap<User, UserDetailsDto>()
                .ForMember(dest => dest.Subscriptions, opt => opt.MapFrom(src => src.Subscriptions));
            // Category
            CreateMap<CategoryCreateRequest, Category>().ReverseMap();
            CreateMap<CategoryUpdateRequest, Category>().ReverseMap();
            // Author
            CreateMap<AuthorCreateRequest,Author>().ReverseMap();
            CreateMap<AuthorUpdateRequest, Author>().ReverseMap();
            CreateMap<AuthorRequest, Author>()
    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? Guid.NewGuid()));

            // Book
            CreateMap<BookCreateRequest, Book>().ReverseMap();
            CreateMap<BookCreateRequest, Book>()
            .ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.Authors));
            CreateMap<BookUpdateRequest, Book>().ReverseMap(); 
            // Recap
            CreateMap<PrepareIdea, KeyIdea>().ReverseMap();
            CreateMap<UpdateKeyIdeaRequest, KeyIdea>().ReverseMap();
            CreateMap<CreateRecapRequest, Recap>().ReverseMap();
            CreateMap<ChooseVersion, Recap>().ReverseMap();
            CreateMap<CreatePrepareVersion, RecapVersion>().ReverseMap();
            CreateMap<UpdateRecapVersion, RecapVersion>().ReverseMap();
            CreateMap<CreateRecapVersion, RecapVersion>().ReverseMap();
            CreateMap<CreatePrepareVersion, RecapVersion>()
            .ForMember(dest => dest.RecapId, opt => opt.Ignore());
            // Playlist
            CreateMap<CreatePlayList, PlayList>();
            CreateMap<CreatePlayListItem, PlayListItem>();
            // Review
            CreateMap<CreateReviewRequest, Review>();
            CreateMap<Review, ReviewDTO>()
                    .ForMember(dest => dest.StaffName, opt => opt.MapFrom(src => src.Staff != null ? src.Staff.FullName : null));
            
            CreateMap<UpdateReviewRequest, Review>().ReverseMap();

            // ReviewNote
            CreateMap<CreateReviewNoteRequest, ReviewNote>().ReverseMap();
            CreateMap<UpdateReviewNoteRequest, ReviewNote>().ReverseMap();
            // Highlight 
            CreateMap<CreateHighlightRequest, Highlight>().ReverseMap();
            CreateMap<UpdateHighlightRequest, Highlight>().ReverseMap();
            // Appeal
            CreateMap<CreateAppealRequest, Appeal>().ReverseMap();
            CreateMap<UpdateAppealContributor, Appeal>().ReverseMap();
            CreateMap<UpdateAppealResponse, Appeal>().ReverseMap();
            CreateMap<UpdateAppealStatus, Appeal>().ReverseMap();
            //SystemSetting
            CreateMap<CreateSystemSetting, SystemSetting>().ReverseMap();
            CreateMap<SystemSettingUpdateRequest, SystemSetting>().ReverseMap();
            //SupportTicket
            CreateMap<CreateSupportTicketRequest, SupportTicket>().ReverseMap();
            CreateMap<UpdateSupportTicketRequest, SupportTicket>().ReverseMap();
            CreateMap<ResponseSupportTicket, SupportTicket>().ReverseMap();
            //Subscription Package
            CreateMap<CreateSubscriptionPackage, SubscriptionPackage>().ReverseMap();
            CreateMap<UpdateSubscriptionPackage, SubscriptionPackage>().ReverseMap();
            //Subscription
            CreateMap<CreateSubscription, Subscription>().ReverseMap();
            //Contract 
            CreateMap<CreateContract, Contract>().ReverseMap();
            CreateMap<CreatePrepareContract, Contract>().ReverseMap();
            CreateMap<UpdateContract, Contract>().ReverseMap();
            CreateMap<AddContractAttachment, Contract>().ReverseMap();
            CreateMap<AddBookToContract, Contract>().ReverseMap();
            CreateMap<ChangeStatusContract, Contract>().ReverseMap();
            //ContractAttachment
            CreateMap<CreateContractAttachment, ContractAttachment>().ReverseMap();
            CreateMap<UpdateContractAttachment, ContractAttachment>().ReverseMap();
            //Viewtracking
            CreateMap<CreateViewTracking, ViewTracking>().ReverseMap();
            //Publisher
            CreateMap<UpdatePublisher, Publisher>().ReverseMap();
            CreateMap<CreatePublisher, Publisher>().ReverseMap();
        }
    }
}