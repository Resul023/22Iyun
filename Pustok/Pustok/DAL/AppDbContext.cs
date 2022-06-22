﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pustok.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pustok.DAL
{
    public class AppDbContext:IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {

        }
        public DbSet<Genres> Genres { get; set; }
        public DbSet<Authors> Authors { get; set; }
        public DbSet<Books> Book { get; set; }
        public DbSet<Slider> Slider { get; set; }
        public DbSet<Featured> Featured { get; set; }
        public DbSet<Promotions> Promotion { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<BookTag> BookTags { get; set; }
        public DbSet<Setting> Settings { get; set; }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<BookComment> BookComments { get; set; }

    }
}
