﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BTPSKANPUR.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class BTPSKANPUREntities : DbContext
    {
        public BTPSKANPUREntities()
            : base("name=BTPSKANPUREntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<BoughtCours> BoughtCourses { get; set; }
        public virtual DbSet<CoursePayment> CoursePayments { get; set; }
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<forget> forgets { get; set; }
    }
}
