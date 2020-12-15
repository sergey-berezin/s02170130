using Microsoft.EntityFrameworkCore;
using OnnxClassifier;
using System.IO;
using System.Numerics;
using System.Linq;
using System;
using System.Collections.Generic;

namespace API
{
    public class DataBaseUtils
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

            public IEnumerable<string> Statistics()
            {

                    var q = from item in this.Images
                            orderby item.Hash
                            select "Hash: " + item.Hash.ToString() + " Call: " + item.Call.ToString();
                    return q.ToArray();

                
            }

            public List<ImageString> GetAllImages()
            {
                var q = from item in this.Images
                        select new ImageString()
                        {
                            ImageBase64 = Convert.ToBase64String(item.ImageBytes.Bytes),
                            path = item.Path,
                            ClassImage = item.Class,
                            Probability = item.Prob
                        };
                return q.ToList();
            }

            public void ClearDataBase()
            {
                lock (this)
                {

                    Images.RemoveRange(Images);
                    Blobs.RemoveRange(Blobs);
                    SaveChanges();

                }

            }

            public void AddToDataBase(ImageString obj)
            {
                lock (this)
                {
                    byte[] BytesImage = Convert.FromBase64String(obj.ImageBase64);
                    Blob ImageBlob = new Blob { Bytes = BytesImage };
                    this.Blobs.Add(ImageBlob);
                    this.SaveChanges();

                    RecognitionImage elem = new RecognitionImage
                    {
                        Class = obj.ClassImage,
                        Prob = obj.Probability,
                        ImageBytes = ImageBlob,
                        Call = 0,
                        Hash = GetHashFromBytes(BytesImage),
                        Path = obj.path
                    };
                    this.Images.Add(elem);
                    this.SaveChanges();

                }
            }

            public ResultClassification FindInDataBase(ImageString obj)
            {

                byte[] array = Convert.FromBase64String(obj.ImageBase64);

                lock (this)
                {
                    var images = this.Images.Where(p => p.Hash == GetHashFromBytes(array)).Select(p => p).ToList();
                    foreach (var img in images)
                    {
                        this.Entry(img).Reference(p => p.ImageBytes).Load();
                        if (array.SequenceEqual(img.ImageBytes.Bytes))
                        {
                            img.Call += 1;
                            this.SaveChanges();
                            return new ResultClassification(img.Path, img.Class, img.Prob);
                        }
                    }
                }

                Console.WriteLine("NOT_FOUND");

                return null;
            }

            public byte[] ImageToByteArray(Avalonia.Media.Imaging.Bitmap image)
            {

                using (MemoryStream stream = new MemoryStream())
                {
                    image.Save(stream);
                    return stream.ToArray();
                }
            }

            static public Avalonia.Media.Imaging.Bitmap ByteArrayToImage(byte[] array)
            {
                using (MemoryStream stream = new MemoryStream(array))
                {
                    return new Avalonia.Media.Imaging.Bitmap(stream);
                }
            }

            public static int GetHashFromBytes(byte[] bytes)
            {
                return new BigInteger(bytes).GetHashCode();
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

    
}
