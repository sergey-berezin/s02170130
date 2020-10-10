using System;
using OnnxClassifier;

namespace Task_1
{

    class Program
    {
        static void RecognitionCompletedHandler(ResultClassification result)
        {
            Console.WriteLine(result);
        }

        static void Main(string[] args)
        {
            string directoryPath;

            if (args.Length == 0)
                directoryPath = "..//..//s02170130/ImageNetSample";
            else
                directoryPath = args[0];


            OnnxClassifier.OnnxClassifier onnxModel = new OnnxClassifier.OnnxClassifier();

            ThreadClassification task_1 = new ThreadClassification(directoryPath, onnxModel, RecognitionCompletedHandler);

            Console.WriteLine("Press any key to stop...");

            task_1.Run();

            Console.ReadKey(true);
            task_1.Stopper();


        }
    }
}
