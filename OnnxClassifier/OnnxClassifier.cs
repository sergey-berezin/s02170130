using System;
using System.IO;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp.Processing;
using Microsoft.ML.OnnxRuntime;

using System.Linq;


namespace OnnxClassifier
{
    public class OnnxClassifier
    {
        
        public InferenceSession session;
        const int TargetWidth = 224;
        const int TargetHeight = 224;

        public OnnxClassifier(string modelFilePath = "/Users/alexandra/Desktop/s02170130/OnnxClassifier/resnet50-v2-7.onnx")
        {
            session = new InferenceSession(modelFilePath);

        }

        static private DenseTensor<float> PreprocImage(string imageFilePath)
        {
            using Image<Rgb24> image = Image.Load<Rgb24>(imageFilePath);

            using Stream imageStream = new MemoryStream();

            image.Mutate(x =>
            {
                x.Resize(new ResizeOptions
                {
                    Size = new Size(TargetWidth, TargetHeight),
                    Mode = ResizeMode.Crop
                });
            });


            DenseTensor<float> input = new DenseTensor<float>(new[] { 1, 3, TargetWidth, TargetHeight });
            var mean = new[] { 0.485f, 0.456f, 0.406f };
            var stddev = new[] { 0.229f, 0.224f, 0.225f };
            for (int y = 0; y < image.Height; y++)
            {
                Span<Rgb24> pixelSpan = image.GetPixelRowSpan(y);
                for (int x = 0; x < image.Width; x++)
                {
                    input[0, 0, y, x] = ((pixelSpan[x].R / 255f) - mean[0]) / stddev[0];
                    input[0, 1, y, x] = ((pixelSpan[x].G / 255f) - mean[1]) / stddev[1];
                    input[0, 2, y, x] = ((pixelSpan[x].B / 255f) - mean[2]) / stddev[2];
                }
            }

            return input;
        }



        public ResultClassification PredictModel(string imageFilePath)
        {
            DenseTensor<float> TensorImage = OnnxClassifier.PreprocImage(imageFilePath);

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(session.InputMetadata.Keys.First(), TensorImage)
            };

            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);

            var output = results.First().AsEnumerable<float>().ToArray();
            float sum = output.Sum(x => (float)Math.Exp(x));
     
            var softmax = output.Select(x => (float)Math.Exp(x) / sum).ToList();

            string cl = LabelMap.Labels[softmax.IndexOf(softmax.Max())];
            ResultClassification result = new ResultClassification(imageFilePath, cl, softmax.Max());

            return result;


        }

   

        
    }
}
