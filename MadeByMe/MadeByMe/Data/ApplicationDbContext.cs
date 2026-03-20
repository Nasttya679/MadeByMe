using System.Reflection;
using MadeByMe.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MadeByMe.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet для кожної моделі
        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<BuyerCart> BuyerCarts { get; set; }
        public DbSet<SellerPost> SellerPosts { get; set; }
        public DbSet<Photo> Photos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Складений ключ для BuyerCart
            modelBuilder.Entity<BuyerCart>()
                .HasKey(bc => new { bc.CartId, bc.PostId });

            modelBuilder.Entity<BuyerCart>()
                .HasOne(bc => bc.Cart)
                .WithMany(c => c.BuyerCarts)
                .HasForeignKey(bc => bc.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BuyerCart>()
                .HasOne(bc => bc.Post)
                .WithMany()
                .HasForeignKey(bc => bc.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Складений ключ для SellerPost
            modelBuilder.Entity<SellerPost>()
                .HasKey(sp => new { sp.SellerId, sp.PostId });

            modelBuilder.Entity<SellerPost>()
                .HasOne(sp => sp.Seller)
                .WithMany()
                .HasForeignKey(sp => sp.SellerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SellerPost>()
                .HasOne(sp => sp.Post)
                .WithMany()
                .HasForeignKey(sp => sp.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Налаштування Post
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Seller)
                .WithMany()
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. Налаштування Comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.CommentsList)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // 5. Налаштування Cart
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Buyer)
                .WithMany()
                .HasForeignKey(c => c.BuyerId)
                .OnDelete(DeleteBehavior.SetNull);

            // 6. Налаштування Photo
            modelBuilder.Entity<Photo>()
                .HasOne(p => p.Post)
                .WithMany(p => p.Photos)
                .HasForeignKey(p => p.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}