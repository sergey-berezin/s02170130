using System;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;


namespace OnnxClassifier
{
    public class ImageString
    {
        public string path { get; set; }
        public string ImageBase64 { get; set; }
        public string ClassImage { get; set; }
        public float Probability { get; set; }

    }

    public class ImageStringWeb
    {
        public string path { get; set; }
        public string ImageBase64 { get; set; }
        public string ClassImage { get; set; }

    }

    public class ResultClassification
    {
        private string PathImage;
        private string ClassImage;
        private float Probability;

        public ResultClassification(string path, string cl, float prob)
        {
            PathImage = path;
            ClassImage = cl;
            Probability = prob;
        }

        public string _ClassImage {
            set { ClassImage = value;  }
            get { return ClassImage; }

        }

        public string _PathImage
        {
            set { PathImage = value; }
            get { return PathImage; }

        }

        public float _Probability
        {
            set { Probability = value; }
            get { return Probability; }

        }



        public override string ToString()
        {
            return PathImage + ' ' + ClassImage + ' ' + Probability.ToString();
        }
    }



    public class ThreadClassification
    {

        Action<ResultClassification> ImageRecognitionCompleted;


        private ConcurrentQueue<string> PathImages = new ConcurrentQueue<string>();
        public ConcurrentQueue<ResultClassification> Result = new ConcurrentQueue<ResultClassification>();

        private CancellationTokenSource CancelThreads = new CancellationTokenSource(); //для пула потоков

        private OnnxClassifier Model;

        public ThreadClassification(string currentDirectory, OnnxClassifier onnxModel, Action<ResultClassification> handler)
        {
            Model = onnxModel;
            ImageRecognitionCompleted = handler;


            PathImages = new ConcurrentQueue<string>(Directory.GetFiles(currentDirectory).Where(s => s.EndsWith(".JPEG") || s.EndsWith(".jpg")));
        }

        public ThreadClassification(ConcurrentQueue<string> pathImages, OnnxClassifier onnxModel, Action<ResultClassification> handler)
        {
            Model = onnxModel;
            ImageRecognitionCompleted = handler;


            PathImages = pathImages;
        }

        public void Run()
        {

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                var th = new Thread(Worker);
                th.Name = $"Thread {i}";
                th.Start();
            }

            

        }

        public void Stopper()
        {
            CancelThreads.Cancel();

        }



        private void Worker()
        {

            while (!CancelThreads.Token.IsCancellationRequested && PathImages.TryDequeue(out string image))
            {
                ResultClassification result = Model.PredictModel(image);
                Result.Enqueue(result);
                ImageRecognitionCompleted(result);

            }

        }
    
    
    }
}
