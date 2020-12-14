using Microsoft.EntityFrameworkCore;

namespace Task_3
{
    public class ApplicationContext : DbContext
    {
        public DbSet<RecognitionImage> Images { get; set; }
        public DbSet<Blob> Blobs { get; set; }
        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=images.db");

        public void ClearDataBase()
        {
            lock (this)
            {

                Images.RemoveRange(Images);
                Blobs.RemoveRange(Blobs);
                SaveChanges();

            }

        }
    }

    public class RecognitionImage
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public int Hash { get; set; }

        public float Prob { get; set; }
        public string Class { get; set; }
        public int Call { get; set; }
        public Blob ImageBytes { get; set; }

    }

    public class Blob
    {
        public int Id { get; set; }
        public byte[] Bytes { get; set; }

    }

    
}
