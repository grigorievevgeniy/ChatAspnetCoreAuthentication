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
            : base(options)
        {
        }

        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatUser> ChatUsers { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ChatUser>().HasKey(table => new {
                table.ChatId,
                table.UserId
            });

            base.OnModelCreating(builder);
        }

        public void AddMessage(ChatMessage message)
        {
            ChatMessages.Add(message);
            SaveChanges();
        }

    }
}
