using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiType.AppData;

namespace MultiType.App_Data
{
    public class MultiTypeContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Race> Races { get; set; }
        public DbSet<KeyAccuracy> KeyAccuracys { get; set; }
        public DbSet<Lesson> Lessons { get; set; }

        static MultiTypeContext()
        {
            Database.SetInitializer(new ContextInitializer());
            using (var db = new MultiTypeContext())
                db.Database.Initialize(false);
        }

        public MultiTypeContext() : base("MultiTypeConnectionString")
        {
        }
    }

    class ContextInitializer : CreateDatabaseIfNotExists<MultiTypeContext>
    {
        protected override void Seed(MultiTypeContext context)
        {
            context.Users.Add(new User
            {
                UserId = 1,
                FirstName = "Isaac",
                LastName = "Hermens",
                Password = "I9her3sm"
            });
            base.Seed(context);
        }
    }
}
