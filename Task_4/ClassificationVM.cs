using System;
using OnnxClassifier;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using System.ComponentModel;
using System.IO;
using System.Numerics;


namespace Task_4
{
    public class ClassificationVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public OnnxClassifier.ThreadClassification task;
        public OnnxClassifier.OnnxClassifier onnxModel;
        public ObservableCollection<Tuple<string, int>> Classes { get; set; }
        public ObservableCollection<ResultClassification> Result { get; set; }
        public ObservableCollection<ImageString> AllResult { get; set; }
        private ObservableCollection<string> stat { get; set; } 


        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public ClassificationVM()
        {
            Classes = new ObservableCollection<Tuple<string, int>>();
            Result = new ObservableCollection<ResultClassification>();

            stat = new ObservableCollection<string>();
            AllResult = new ObservableCollection<ImageString>();

        }

        public void AllImages(ImageString[] images)
        {
            Dispatcher.UIThread.InvokeAsync(() => {
                foreach (var img in images)
                {
                    AllResult.Add(img);
                }
            });
            
        }

        public void Statistics(string[] obj)
        {
            Dispatcher.UIThread.InvokeAsync(() => {
                stat.Clear();
                foreach (string st in obj)
                {
                    stat.Add(st);
                }
            });
            
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
