using System;
using OnnxClassifier;
using System.Collections.ObjectModel;
using System.Threading;
using Avalonia.Threading;
using System.Linq;
using System.ComponentModel;




namespace Task_2
{
    public class ClassificationVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public OnnxClassifier.ThreadClassification task;
        public OnnxClassifier.OnnxClassifier onnxModel;
        public ObservableCollection<Tuple<string, int>> Classes {get; set;}
        public ObservableCollection<ResultClassification> Result { get; set; }
        public ObservableCollection<ResultClassification> SelectedResult { get; set; }

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
                    SelectedResult = 
                        new ObservableCollection<ResultClassification>(Result.Where(elem => elem._ClassImage == selectedItem.Item1));
                    OnPropertyChanged("SelectedResult");
                }
                
            }
        }

        

        public ClassificationVM()
        {
            Classes = new ObservableCollection<Tuple<string, int>>();
            Result = new ObservableCollection<ResultClassification>();
            SelectedResult = new ObservableCollection<ResultClassification>();

            onnxModel = new OnnxClassifier.OnnxClassifier();

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

        public void RecognitionCompletedHandler(ResultClassification elem)
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

        public void ClassificationImages(string path)
        {
            Classes.Clear();
            task = new ThreadClassification(path, onnxModel, RecognitionCompletedHandler);
            Thread t = new Thread(() => task.Run());
            t.Start();


        }

        public void StopClassificationImages()
        {
            
            task.Stopper();
            
        }
    }
}
