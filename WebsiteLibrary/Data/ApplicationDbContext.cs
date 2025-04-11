using Microsoft.EntityFrameworkCore;
using WebsiteLibrary.Models.Entities;

namespace WebsiteLibrary.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<LibraryCard> LibraryCards { get; set; }
        public DbSet<BorrowingSlip> BorrowingSlips { get; set; }
        public DbSet<BorrowingSlipDetail> BorrowingSlipDetails { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<CardRenewal> CardRenewals { get; set; }
        public DbSet<Reader> Readers { get; set; }
        public DbSet<Librarian> Librarians { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ImportReceipt> ImportReceipts { get; set; }
        public DbSet<ImportDetail> ImportDetails { get; set; }
        public DbSet<LoanRequest> LoanRequests { get; set; }
        public DbSet<LoanRequestDetail> LoanRequestDetails { get; set; }
        public DbSet<ReturnSlip> ReturnSlips { get; set; }
        public DbSet<ReturnSlipDetail> ReturnSlipDetails { get; set; }
        public DbSet<LibraryCardRequest> LibraryCardRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Book - BookCopy
            modelBuilder.Entity<BookCopy>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BookCopies)
                .HasForeignKey(bc => bc.OriginalBookID)
                .OnDelete(DeleteBehavior.NoAction);

            // Cấu hình bổ sung cho Price trong BookCopy (tùy chọn)
            modelBuilder.Entity<BookCopy>()
                .Property(bc => bc.Price)
                .IsRequired() // Giá không được null
                .HasDefaultValue(0); // Giá mặc định là 0 nếu không cung cấp

            // ImportDetail
            modelBuilder.Entity<ImportDetail>()
                .HasKey(i => new { i.ImportReceiptId, i.OriginalBookId });

            modelBuilder.Entity<ImportDetail>()
                .HasOne(i => i.ImportReceipt)
                .WithMany(ir => ir.ImportDetails)
                .HasForeignKey(i => i.ImportReceiptFK)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ImportDetail>()
                .HasOne(i => i.Book)
                .WithMany(b => b.ImportDetails)
                .HasForeignKey(i => i.OriginalBookFK)
                .OnDelete(DeleteBehavior.NoAction);

            // LoanRequestDetail
            modelBuilder.Entity<LoanRequestDetail>()
                .HasKey(l => new { l.RequestId, l.BookCopyId });

            modelBuilder.Entity<LoanRequestDetail>()
                .HasOne(l => l.LoanRequest)
                .WithMany(lr => lr.LoanRequestDetails)
                .HasForeignKey(l => l.RequestFK)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LoanRequestDetail>()
                .HasOne(l => l.BookCopy)
                .WithMany()
                .HasForeignKey(l => l.BookCopyFK)
                .OnDelete(DeleteBehavior.NoAction);

            // BorrowingSlipDetail
            modelBuilder.Entity<BorrowingSlipDetail>()
                .HasKey(b => new { b.BorrowingSlipId, b.BookCopyId });

            modelBuilder.Entity<BorrowingSlipDetail>()
                .HasOne(b => b.BorrowingSlip)
                .WithMany(bs => bs.BorrowingSlipDetails)
                .HasForeignKey(b => b.BorrowingSlipFK)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BorrowingSlipDetail>()
                .HasOne(b => b.BookCopy)
                .WithMany()
                .HasForeignKey(b => b.BookCopyFK)
                .OnDelete(DeleteBehavior.NoAction);

            // ReturnSlipDetail
            modelBuilder.Entity<ReturnSlipDetail>()
                .HasKey(r => new { r.ReturnSlipId, r.BookId });

            modelBuilder.Entity<ReturnSlipDetail>()
                .HasOne(r => r.ReturnSlip)
                .WithMany(rs => rs.ReturnSlipDetails)
                .HasForeignKey(r => r.ReturnSlipFK)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ReturnSlipDetail>()
                .HasOne(r => r.BookCopy)
                .WithMany()
                .HasForeignKey(r => r.BookCopyFK)
                .OnDelete(DeleteBehavior.NoAction);

            // Cấu hình inheritance cho Reader và Librarian
            modelBuilder.Entity<Account>()
                .HasDiscriminator<string>("Role")
                .HasValue<Reader>("Reader")
                .HasValue<Librarian>("Admin");

            // Thêm index để cải thiện hiệu suất
            modelBuilder.Entity<ImportDetail>()
                .HasIndex(i => i.OriginalBookFK);

            modelBuilder.Entity<BookCopy>()
                .HasIndex(bc => bc.OriginalBookID);

            modelBuilder.Entity<BorrowingSlipDetail>()
                .HasIndex(b => b.BookCopyFK);

            // Cấu hình cho LibraryCardRequest (bảng riêng)
            modelBuilder.Entity<LibraryCardRequest>()
                .HasKey(lcr => lcr.RequestID);

            modelBuilder.Entity<LibraryCardRequest>()
                .HasOne(lcr => lcr.Account)
                .WithMany()
                .HasForeignKey(lcr => lcr.AccountID)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình cho Payment
            modelBuilder.Entity<Payment>()
                .HasKey(p => p.PaymentID);

            modelBuilder.Entity<Payment>()
                .Property(p => p.CardID)
                .IsRequired(false); // Đảm bảo CardID có thể là NULL

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.LibraryCard)
                .WithMany()
                .HasForeignKey(p => p.CardID)
                .OnDelete(DeleteBehavior.SetNull); // Nếu LibraryCard bị xóa, CardID sẽ được đặt thành NULL
        }
    }
}