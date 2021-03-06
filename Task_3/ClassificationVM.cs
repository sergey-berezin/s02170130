﻿using System;
using OnnxClassifier;
using System.Collections.ObjectModel;
using System.Threading;
using Avalonia.Threading;
using System.Linq;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using System.Numerics;


namespace Task_3
{
    public class ClassificationVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public OnnxClassifier.ThreadClassification task;
        public OnnxClassifier.OnnxClassifier onnxModel;
        public ObservableCollection<Tuple<string, int>> Classes { get; set; }
        public ObservableCollection<ResultClassification> Result { get; set; }
        public ObservableCollection<RecognitionImage> SelectedResult { get; set; }
        

        private ObservableCollection<Tuple<int, int>> stat { get; set; } 

        private List<string> PathImages = new List<string>();
        private ConcurrentQueue<string> NewPathImages = new ConcurrentQueue<string>();
        private ApplicationContext db;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Tuple<string, int> selectedItem;
        public Tuple<string, int> SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;

                if (value != null)
                {
                    SelectedResult = new ObservableCollection<RecognitionImage>();
                    lock (db)
                    {
                        var images = db.Images.Where(p => p.Class == selectedItem.Item1).Select(p => p).ToList();
                        foreach (var img in images)
                            SelectedResult.Add(img);

                    }
                    OnPropertyChanged("SelectedResult");
                }
                
            }
        }
 

        public ClassificationVM()
        {
            Classes = new ObservableCollection<Tuple<string, int>>();
            Result = new ObservableCollection<ResultClassification>();
            SelectedResult = new ObservableCollection<RecognitionImage>();

            stat = new ObservableCollection<Tuple<int, int>>();

            db = new ApplicationContext();

            onnxModel = new OnnxClassifier.OnnxClassifier();

        }

        

        public void RecognitionCompletedHandler(ResultClassification elem)
        {
            CreateResults(elem);
            AddToDataBase(elem);
        }

        public void ExistsCompletedHandler(ResultClassification elem)
        {
            CreateResults(elem);
            
        }

        public void CreateResults(ResultClassification elem)
        {
            Dispatcher.UIThread.InvokeAsync(() => {
                Result.Add(elem);
                int index = CheckUniqueClass(elem._ClassImage);
                if (index == -1)
                    Classes.Add(new Tuple<string, int>(elem._ClassImage, 1));
                else
                    Classes[index] = new Tuple<string, int>(Classes[index].Item1, Classes[index].Item2 + 1);
            });
        }


        public void ClassificationImages(string currentDirectory)
        {
            Classes.Clear();

            PathImages = new List<string>(Directory.GetFiles(currentDirectory, "*.jpg"));

            Thread t = new Thread(() =>
            {
                foreach (string path in PathImages)
                {
                    ResultClassification imageDataBase = FindInDataBase(path);

                    if (imageDataBase != null)
                        ExistsCompletedHandler(imageDataBase);
                    else
                        NewPathImages.Enqueue(path);


                }

                task = new ThreadClassification(NewPathImages, onnxModel, RecognitionCompletedHandler);
                task.Run();
            });

            t.Start();

        }

        public void StopClassificationImages()
        {

            task.Stopper();

        }

        ResultClassification FindInDataBase(string path)
        {

            byte[] array = ImageToByteArray(new Avalonia.Media.Imaging.Bitmap(path));


            lock(db)
            {
                var images = db.Images.Where(p => p.Hash == GetHashFromBytes(array)).Select(p => p).ToList();
                foreach (var img in images)
                {
                    db.Entry(img).Reference(p => p.ImageBytes).Load();
                    if (array.SequenceEqual(img.ImageBytes.Bytes))
                    {
                        img.Call += 1;
                        db.SaveChanges();
                        return new ResultClassification(img.Path, img.Class, img.Prob);
                    }
                } 
            }
            

             return null; 
        }


        public void AddToDataBase(ResultClassification obj)
        {
            lock(db)
            {
                Avalonia.Media.Imaging.Bitmap BImage = new Avalonia.Media.Imaging.Bitmap(obj._PathImage);
                Blob ImageBlob = new Blob { Bytes = ImageToByteArray(BImage) };
                db.Blobs.Add(ImageBlob);
                db.SaveChanges();

                RecognitionImage elem = new RecognitionImage
                {
                    Class = obj._ClassImage,
                    Prob = obj.Probability,
                    ImageBytes = ImageBlob,
                    Call = 0,
                    Hash = GetHashFromBytes(ImageToByteArray(BImage))
                };
                db.Images.Add(elem);
                

                db.SaveChanges();

            }
        }

        public void ClearDataBase()
        {
            db.ClearDataBase();

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


        public void Statistics()
        {

            stat.Clear();
            lock(db)
            {
                var images = db.Images.Select(p => new
                {
                    Hash = p.Hash,
                    Call = p.Call,
                });

                foreach (var elem in images)
                    stat.Add(new Tuple<int, int>(elem.Hash, elem.Call));
            }


        }

        int CheckUniqueClass(string NewClass)
        {
            int index = -1;
            foreach (var item in Classes)
            {

                if (item.Item1.Equals(NewClass))
                {
                    return Classes.IndexOf(item);
                }
            }
            return index;
        }

        public static int GetHashFromBytes(byte[] bytes)
        {
            return new BigInteger(bytes).GetHashCode();
        }
    }
}
