using System;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;


namespace OnnxClassifier
{
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

            PathImages = new ConcurrentQueue<string>(Directory.GetFiles(currentDirectory, "*.JPEG"));
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
