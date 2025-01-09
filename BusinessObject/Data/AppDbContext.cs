using BusinessObject.Auditing;
using BusinessObject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BusinessObject.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public AppDbContext()
        {
        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Audit> AuditLogs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Recap> Recaps { get; set; }
        public DbSet<RecapVersion> RecapVersions { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewNote> ReviewNotes { get; set; }
        public DbSet<Highlight> Highlights { get; set; }
        public DbSet<KeyIdea> KeyIdeas { get; set; }
        public DbSet<PlayList> PlayLists { get; set; }
        public DbSet<PlayListItem> PlayListItems { get; set; }
        public DbSet<ReadingPosition> ReadingPositions { get; set; }
        public DbSet<ViewTracking> ViewTrackings { get; set; }
        public DbSet<ContractAttachment> ContractAttachments { get; set; }
        public DbSet<ContributorPayout> ContributorPayouts { get; set; }
        public DbSet<ContributorWithdrawal> ContributorWithdrawals { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Appeal> Appeals { get; set; }
        public DbSet<PublisherPayout> PublisherPayouts { get; set; }
        public DbSet<SubscriptionPackage> SubscriptionPackages { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<BookEarning> BookEarnings { get; set; }
        public DbSet<RecapEarning> RecapEarnings { get;set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Token> Tokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                base.OnConfiguring(optionsBuilder);
                optionsBuilder.UseSqlServer(ConnectionString());
            }
        }
        public string ConnectionString()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            return builder.Build().GetConnectionString("DefaultConnection");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
    .HasMany(u => u.FollowedBooks)
    .WithMany(b => b.Users)
    .UsingEntity<Dictionary<string, object>>(
        "UserBookFollows",
        j => j
            .HasOne<Book>() // Đảm bảo rằng có một thực thể Book
            .WithMany()
            .HasForeignKey("BookId")
            .OnDelete(DeleteBehavior.Restrict), // Chỉ định hành vi xóa cho Foreign Key của Book
        j => j
            .HasOne<User>() // Đảm bảo rằng có một thực thể User
            .WithMany()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Restrict), // Chỉ định hành vi xóa cho Foreign Key của User
        j =>
        {
            j.HasKey("UserId", "BookId"); // Khóa chính cho bảng UserBookFollows
            j.ToTable("UserBookFollows"); // Đặt tên bảng
        });

            modelBuilder.Entity<IdentityRole<Guid>>(entity =>
            {
                entity.ToTable(name: "Roles");
            });

            modelBuilder.Entity<User>()
            .HasMany(u => u.FollowedCategories)
            .WithMany(c => c.Users)
            .UsingEntity(j => j.ToTable("UserCategoryFollows"));

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // SubscriptionPackage - Transaction (1-n)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.SubscriptionPackage)
                .WithMany(sp => sp.Transactions)
                .HasForeignKey(t => t.SubscriptionPackageId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - Subscription (1-n)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // SubscriptionPackage - Subscription (1-n)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.SubscriptionPackage)
                .WithMany(sp => sp.Subscriptions)
                .HasForeignKey(s => s.SubscriptionPackageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction - Subscription (1-1, optional)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Transaction)
                .WithOne(t => t.Subscription)
                .HasForeignKey<Subscription>(s => s.TransactionId)
                .OnDelete(DeleteBehavior.SetNull);
            // User - Author nhiều-nhiều
            modelBuilder.Entity<User>()
                .HasMany(u => u.FollowedAuthors)
                .WithMany(a => a.Users)
                .UsingEntity(j => j.ToTable("UserAuthorFollows"));

            modelBuilder.Entity<CategoryStaff>()
                .HasKey(cs => new { cs.StaffId, cs.CategoryId });

            // Cấu hình mối quan hệ giữa CategoryStaff và User (Staff)
            modelBuilder.Entity<CategoryStaff>()
                .HasOne(cs => cs.Staff)
                .WithMany()  // Users có thể thuộc nhiều Categories
                .HasForeignKey(cs => cs.StaffId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ContributorPayout>()
                .HasOne(cp => cp.Contributor)
                .WithMany(u => u.ContributorPayouts)
                .HasForeignKey(cp => cp.UserId);
            // Cấu hình mối quan hệ giữa CategoryStaff và Category
            modelBuilder.Entity<CategoryStaff>()
                .HasOne(cs => cs.Category)
                .WithMany()  // Categories có thể có nhiều Users
                .HasForeignKey(cs => cs.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            // User - Playlist one-to-many relationship
            modelBuilder.Entity<PlayList>()
                .HasOne(pl => pl.User)
                .WithMany(u => u.PlayLists)
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - Recap one-to-many relationship
            modelBuilder.Entity<Recap>()
                .HasOne(c => c.Contributor)
                .WithMany(u => u.Recaps)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupportTicket>()
                .HasOne(c => c.Recaps)
                .WithMany(u => u.SupportTicketItems)
                .HasForeignKey(c => c.RecapId)
                .OnDelete(DeleteBehavior.Restrict);

            // Recap - RecapVersion one-to-many relationship
            modelBuilder.Entity<RecapVersion>()
                .HasOne(cv => cv.Recap)
                .WithMany(c => c.RecapVersions)
                .HasForeignKey(cv => cv.RecapId)
                .OnDelete(DeleteBehavior.Restrict);

            // PlaylistItem - Playlist one-to-many relationship
            modelBuilder.Entity<PlayListItem>()
                .HasOne(pli => pli.PlayList)
                .WithMany(pl => pl.PlayListItems)
                .HasForeignKey(pli => pli.PlayListId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Recap>()
                .HasMany(r => r.PlayListItems)
                .WithOne(p => p.Recap)
                .HasForeignKey(p => p.RecapId);

            // PlaylistItem - Recap one-to-one relationship
            modelBuilder.Entity<PlayListItem>()
                .HasOne(pli => pli.Recap)
                .WithMany()
                .HasForeignKey(pli => pli.RecapId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - Like one-to-many relationship
            modelBuilder.Entity<Like>()
                .HasOne(w => w.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - SupportTicket one-to-many relationship
            modelBuilder.Entity<SupportTicket>()
                .HasOne(st => st.User)
                .WithMany(u => u.SupportTickets)
                .HasForeignKey(st => st.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - Subscription one-to-many relationship
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - Appeal relationships

            modelBuilder.Entity<Appeal>()
                .HasOne(a => a.Staff)
                .WithMany()
                .HasForeignKey(a => a.StaffId)
                .OnDelete(DeleteBehavior.Restrict);


            // KeyIdea - RecapVersion one-to-many relationship
            modelBuilder.Entity<KeyIdea>()
                .HasOne(ki => ki.RecapVersion)
                .WithMany(cv => cv.KeyIdeas)
                .HasForeignKey(ki => ki.RecapVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Highlight - KeyIdea one-to-many relationship
            modelBuilder.Entity<Highlight>()
                .HasOne(h => h.RecapVersions)
                .WithMany(ki => ki.Highlights)
                .HasForeignKey(h => h.RecapVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Highlight - Staff (User) one-to-many relationship
            modelBuilder.Entity<Highlight>()
                .HasOne(h => h.User)
                .WithMany(u => u.Highlights)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ReadingPosition - KeyIdea one-to-many relationship
            modelBuilder.Entity<ReadingPosition>()
                .HasOne(rp => rp.RecapVersions)
                .WithMany(ki => ki.ReadingPositions)
                .HasForeignKey(rp => rp.RecapVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ReadingPosition - User one-to-many relationship
            modelBuilder.Entity<ReadingPosition>()
                .HasOne(rp => rp.User)
                .WithMany(u => u.ReadingPositions)
                .HasForeignKey(rp => rp.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            // Recap - Book one-to-many relationship

            modelBuilder.Entity<Recap>()
                .HasOne(c => c.Book)
                .WithMany(b => b.Recaps)
                .HasForeignKey(c => c.BookId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Recap>()
                .HasOne(c => c.CurrentVersion)
                .WithMany()
                .HasForeignKey(c => c.CurrentVersionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Staff) // Một Review có một Staff
                .WithMany() // Không cần định nghĩa Collection nếu không dùng ở Staff
                .HasForeignKey(r => r.StaffId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Review>()
                .HasMany(r => r.ReviewNotes)
                .WithOne(rn => rn.Review)
                .HasForeignKey(rn => rn.ReviewId)
                .OnDelete(DeleteBehavior.Restrict);
            // User - Publisher: Một người dùng có thể là một nhà xuất bản
            modelBuilder.Entity<User>()
                .HasOne(u => u.Publisher)
                .WithOne(p => p.User)
                .HasForeignKey<Publisher>(p => p.UserId);

            // Publisher - Contract: Một nhà xuất bản có thể có nhiều hợp đồng
            modelBuilder.Entity<Publisher>()
                .HasMany(p => p.Contracts)
                .WithOne(c => c.Publisher)
                .HasForeignKey(c => c.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Publisher)
                .WithMany(p => p.Books)
                .HasForeignKey(b => b.PublisherId);
            // Book - Contract: Một sách có thể có nhiều hợp đồng
            modelBuilder.Entity<Book>()
             .HasMany(b => b.Contracts)
             .WithMany(c => c.Books);
            // BookEarning
            modelBuilder.Entity<BookEarning>()
                .HasOne(re => re.Book)
                .WithMany(r => r.BookEarnings)
                .HasForeignKey(re => re.BookId);

            // RecapEarning
            modelBuilder.Entity<RecapEarning>()
                .HasOne(re => re.Recap)
                .WithMany(r => r.RecapEarnings)
                .HasForeignKey(re => re.RecapId);
            modelBuilder.Entity<ViewTracking>()
                .HasOne(vt => vt.Subscription)
                .WithMany(s => s.ViewTrackings)
                .HasForeignKey(vt => vt.SubscriptionId)
                .OnDelete(DeleteBehavior.Restrict); // Quyết định cách xử lý khi Subscription bị xóa

            // Thiết lập mối quan hệ giữa ViewTracking và User
            modelBuilder.Entity<ViewTracking>()
                .HasOne(vt => vt.User)
                .WithMany(u => u.ViewTrackings)
                .HasForeignKey(vt => vt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Thiết lập mối quan hệ giữa ViewTracking và Recap
            modelBuilder.Entity<ViewTracking>()
                .HasOne(vt => vt.Recap)
                .WithMany(r => r.ViewTrackings)
                .HasForeignKey(vt => vt.RecapId)
                .OnDelete(DeleteBehavior.Cascade);

            // Thiết lập mối quan hệ giữa Review và Appeal
            modelBuilder.Entity<Review>()
                .HasMany(r => r.Appeals)            // Một Review có nhiều Appeal
                .WithOne(a => a.Review)              // Một Appeal thuộc về một Review
                .HasForeignKey(a => a.ReviewId)      // Sử dụng ReviewId làm khóa ngoại
                .OnDelete(DeleteBehavior.Restrict);  // Không xóa các Appeal khi xóa Review

            // Add any additional configurations here.
            modelBuilder.Entity<Notification>()
               .Property(b => b.Title)
               .HasMaxLength(256);
            modelBuilder.Entity<Notification>()
                .Property(b => b.Message)
                .HasMaxLength(2048);
            modelBuilder.Entity<Notification>()
                .Property(b => b.Label)
                .HasConversion<string>()
                .HasColumnType("varchar(50)");
            modelBuilder.Entity<Notification>()
            .Property(b => b.IsRead)
            .HasDefaultValue(false);
            modelBuilder.Entity<Notification>()
                .Property(b => b.Url)
                .HasMaxLength(2048);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }



        }


        // Audit
        public virtual async Task<int> SaveChangesAsync(Guid userId)
        {
            OnBeforeSaveChanges(userId);
            var result = await base.SaveChangesAsync();
            return result;
        }

        private void OnBeforeSaveChanges(Guid userId)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;
                var auditEntry = new AuditEntry(entry);
                auditEntry.TableName = entry.Entity.GetType().Name;
                auditEntry.UserId = userId;
                auditEntries.Add(auditEntry);
                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = Enums.AuditType.Create;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.AuditType = Enums.AuditType.Delete;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = Enums.AuditType.Update;
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }
            foreach (var auditEntry in auditEntries)
            {
                AuditLogs.Add(auditEntry.ToAudit());
            }
        }
    }
}