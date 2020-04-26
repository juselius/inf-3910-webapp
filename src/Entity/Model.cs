using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entity
{
    public class DataContext : DbContext
    {
        public DbSet<Person> People { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=data.db");
    }

    public class Person
    {
        [Key]
        public int PersonId { get; set; }
        public string First { get; set; }
        public string Last { get; set; }
        [MaxLength(32)]
        public string? Alias { get; set; }
        public int Age { get; set; }
        public int Height { get; set; }

        public Person()
        {
            First = "";
            Last = "";
            Age = 0;
            Height = 0;
        }
    }
}