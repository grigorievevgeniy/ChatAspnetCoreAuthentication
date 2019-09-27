using System;
using System.Collections.Generic;
using System.Text;
using ChatAspnetCoreAuthentication.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatAspnetCoreAuthentication.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatUser> ChatUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            try
            {
                builder.Entity<ChatUser>().HasKey(table => new
                {
                    table.ChatId,
                    table.UserId
                });

                //builder.Entity<ChatRoom>().HasKey(table => new
                //{
                //    table.RoomId
                //});

                //builder.Entity<ChatRoom>().HasData(new ChatRoom { RoomName = "Simber", OwnerId = "admin" });

                //builder.Entity<ChatRoom>().HasData
                //    (new ChatRoom { RoomId = "1", RoomName = "Simber", OwnerId = "admin" });

                base.OnModelCreating(builder);

            }
            catch (Exception ex)
            {

                string x = ex.Message;
            }
        }
    }
}
