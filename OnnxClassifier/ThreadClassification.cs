using System;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;


namespace OnnxClassifier
{
    public class ThreadClassification
    {
        private AutoResetEvent wait = new AutoResetEvent(true);

        private ConcurrentQueue<string> pathImages = new ConcurrentQueue<string>();

        private CancellationTokenSource cancelThreads = new CancellationTokenSource();
        private CancellationTokenSource cancelRun = new CancellationTokenSource();

        private OnnxClassifier model;

        public ThreadClassification(string currentDirectory, OnnxClassifier onnxModel)
        {
            model = onnxModel;

            pathImages = new ConcurrentQueue<string>(Directory.GetFiles(currentDirectory, "*.JPEG"));
        }

        public void Run()
        {
            Console.WriteLine("Press Enter to stop");

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                var th = new Thread(Worker);
                th.Name = $"Thread {i}";
                th.Start();
            }

            while (!cancelRun.Token.IsCancellationRequested)
            {
                if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Enter)
                {
                    cancelThreads.Cancel();
                    break;
                }

            }

        }



        private void Worker()
        {
            while (!cancelThreads.Token.IsCancellationRequested && pathImages.TryDequeue(out string image))
            {

                string result = model.PredictModel(OnnxClassifier.PreprocImage(image));

                wait.WaitOne();
                Thread.Sleep(500);
                if (!cancelThreads.Token.IsCancellationRequested)
                    Console.WriteLine($"{Thread.CurrentThread.Name}: {result}");
                wait.Set();


            }
            cancelRun.Cancel();

            

        }
    
    
    }
}
