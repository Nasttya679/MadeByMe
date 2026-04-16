using MadeByMe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MadeByMe.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            // 1. Категорії (2 приклади)
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Handmade Jewelry" },
                new Category { CategoryId = 2, Name = "Home Decor" },
                new Category { CategoryId = 3, Name = "Art"}
            );

            // 2. Користувачі (3 приклади)
            var userId1 = "11111111-1111-1111-1111-111111111111";
            var userId2 = "22222222-2222-2222-2222-222222222222";
            var userId3 = "33333333-3333-3333-3333-333333333333";

            // First, update all references to use the new user IDs
            modelBuilder.Entity<Post>().HasData(
                new Post
                {
                    Id = 1,
                    Title = "Срібна сережка",
                    Description = "Ручної роботи з натуральним каменем",
                    Price = 799.99m,
                    CategoryId = 1,
                    SellerId = userId2,
                    CreatedAt = new DateTime(2024, 1, 10, 12, 0, 0)
                },
                new Post
                {
                    Id = 2,
                    Title = "Декоративна ваза",
                    Description = "Керамічна ваза з українським орнаментом",
                    Price = 1200.50m,
                    CategoryId = 2,
                    SellerId = userId2,
                    CreatedAt = new DateTime(2024, 1, 5, 12, 0, 0)
                },
                new Post
                {
                    Id = 3,
                    Title = "Картина 'Сонячний день'",
                    Description = "Олія на полотні, 40x60 см",
                    Price = 2500.00m,
                    CategoryId = 3,
                    SellerId = userId2,
                    CreatedAt = new DateTime(2023, 12, 30, 12, 0, 0)
                }
            );

            modelBuilder.Entity<Comment>().HasData(
                new Comment
                {
                    CommentId = 1,
                    UserId = userId3,
                    PostId = 1,
                    Content = "Дуже гарна сережка! Якісне виконання.",
                    CreatedAt = new DateTime(2024, 1, 10, 10, 0, 0)
                },
                new Comment
                {
                    CommentId = 2,
                    UserId = userId1,
                    PostId = 3,
                    Content = "Чудова картина, автор - талановитий!",
                    CreatedAt = new DateTime(2024, 1, 9, 12, 0, 0)
                }
            );

            modelBuilder.Entity<Cart>().HasData(
                new Cart
                {
                    CartId = 1,
                    BuyerId = userId3
                },
                new Cart
                {
                    CartId = 2,
                    BuyerId = userId1
                }
            );

            modelBuilder.Entity<BuyerCart>().HasData(
                new BuyerCart
                {
                    CartItemId = 1,
                    CartId = 1,
                    PostId = 1,
                    Quantity = 2
                },
                new BuyerCart
                {
                    CartItemId = 2,
                    CartId = 1,
                    PostId = 3,
                    Quantity = 1
                },
                new BuyerCart
                {
                    CartItemId = 3,
                    CartId = 2,
                    PostId = 2,
                    Quantity = 1
                }
            );

            modelBuilder.Entity<SellerPost>().HasData(
                new SellerPost
                {
                    Id = 1,
                    SellerId = userId2,
                    PostId = 1
                },
                new SellerPost
                {
                    Id = 2,
                    SellerId = userId2,
                    PostId = 2
                },
                new SellerPost
                {
                    Id = 3,
                    SellerId = userId2,
                    PostId = 3
                }
            );

            // Finally, add the users
            modelBuilder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = userId1,
                    UserName = "admin",
                    Email = "admin@example.com",
                    NormalizedEmail = "ADMIN@EXAMPLE.COM",
                    PasswordHash = "AQAAAAIAAYagAAAAEEXivHFqQPnenCGcYWQxSSsPJodGdx5QOp7RutIpcF4XHrBMNdJS3RHWvJJmJvQm4w==",  // Admin_123
                    ProfilePicture = "/images/admin.jpg"
                },
                new ApplicationUser
                {
                    Id = userId2,
                    UserName = "artist123",
                    Email = "artist@example.com",
                    NormalizedEmail = "ARTIST@EXAMPLE.COM",
                    PasswordHash = "AQAAAAIAAYagAAAAEEXivHFqQPnenCGcYWQxSSsPJodGdx5QOp7RutIpcF4XHrBMNdJS3RHWvJJmJvQm4w==",  // Admin_123
                    ProfilePicture = "/images/artist.jpg"
                },
                new ApplicationUser
                {
                    Id = userId3,
                    UserName = "customer1",
                    Email = "customer@example.com",
                    NormalizedEmail = "CUSTOMER@EXAMPLE.COM",
                    PasswordHash = "AQAAAAIAAYagAAAAEEXivHFqQPnenCGcYWQxSSsPJodGdx5QOp7RutIpcF4XHrBMNdJS3RHWvJJmJvQm4w==",  // Admin_123
                    ProfilePicture = "/images/customer.jpg"
                }
            );

            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "role-admin",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "role-seller",
                    Name = "Seller",
                    NormalizedName = "SELLER"
                },
                new IdentityRole
                {
                    Id = "role-user",
                    Name = "User",
                    NormalizedName = "USER"
                }
            );


            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = userId1,   // admin
                    RoleId = "role-admin"
                },
                new IdentityUserRole<string>
                {
                    UserId = userId2,   // artist
                    RoleId = "role-seller"
                },
                new IdentityUserRole<string>
                {
                    UserId = userId3,   // customer
                    RoleId = "role-user"
                }
            );
        }
    }
}
