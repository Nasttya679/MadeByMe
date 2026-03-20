using MadeByMe.Models;
using Microsoft.EntityFrameworkCore;

namespace MadeByMe.Data
{
    public static class DataSeeder
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            var userId1 = "11111111-1111-1111-1111-111111111111";
            var userId2 = "22222222-2222-2222-2222-222222222222";
            modelBuilder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = userId1,
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    Email = "admin@example.com",
                    NormalizedEmail = "ADMIN@EXAMPLE.COM",
                    PasswordHash = "AQAAAAIAAYagAAAAEEZ6hGJ4hQz2b6J6B2VZqk1vRkXlY7TJi+W7Xq3X9kKJ9pL3h8pZ1Xy9jW8w1g==",
                    ProfilePicture = "/images/admin.jpg"
                },
                new ApplicationUser
                {
                    Id = userId2,
                    UserName = "artist123",
                    NormalizedUserName = "ARTIST123",
                    Email = "artist@example.com",
                    NormalizedEmail = "ARTIST@EXAMPLE.COM",
                    PasswordHash = "AQAAAAIAAYagAAAAEFz7Oj7hQz2b6J6B2VZqk1vRkXlY7TJi+W7Xq3X9kKJ9pL3h8pZ1Xy9jW8w1g==",
                    ProfilePicture = "/images/artist.jpg"
                }
            );
        }
    }
}