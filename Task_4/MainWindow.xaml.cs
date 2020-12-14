using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading;
using System.Text;
using System;
using OnnxClassifier;
using System.Linq;


namespace Task_4
{
    public class MainWindow : Window
    {


        public ClassificationVM model;
        public string path;
        private static readonly HttpClient client = new HttpClient();
        private static readonly string url = "http://localhost:5000/Recognition";
        private static CancellationTokenSource cts = new CancellationTokenSource();



        public MainWindow()
        {
            InitializeComponent();

            model = new ClassificationVM();
            this.DataContext = model;

            Thread tAll = new Thread(() => {
                try
                {
                    var httpResponse = client.GetAsync(url + "/AllInBase").Result;
                    ImageString[] allImages = JsonConvert.DeserializeObject<ImageString[]>(httpResponse.Content.ReadAsStringAsync().Result);
                    model.AllImages(allImages);


                }
                catch (AggregateException)
                {
                    Console.WriteLine("ERROR_STAT");
                }
            });
            tAll.Start();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


        public async void OpenClickHandler(object sender, RoutedEventArgs e)
        {

            var dialog = new OpenFolderDialog();
            dialog.Directory = Directory.GetCurrentDirectory();
            path = await dialog.ShowAsync(this);

            TextBlock textBlock = this.FindControl<TextBlock>("CurrentDirectory");
            textBlock.Text = path;


        }

        public void StartClickHandler(object sender, RoutedEventArgs e)
        {
            Classes.Clear();

            foreach (string pathImage in Directory.GetFiles(path).Where(s => s.EndsWith(".JPEG") || s.EndsWith(".jpg")))
            {
                ImageString obj = new ImageString()
                {
                    path = pathImage,
                    ImageBase64 = ImageToBase64(new Avalonia.Media.Imaging.Bitmap(pathImage)),
                    Probability = 0,
                    ClassImage = "Default"
                };
                Thread t = new Thread(new ParameterizedThreadStart(Post));
                t.Start(obj);

                
            }
            

        }
        private string ImageToBase64(Avalonia.Media.Imaging.Bitmap image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream);
                return Convert.ToBase64String(stream.ToArray());
            }


        }



        private async void Post(object obj)
        {
            var content = new StringContent(JsonConvert.SerializeObject((ImageString)obj), Encoding.UTF8, "application/json");

            HttpResponseMessage httpResponse;
            try
            {
        
                httpResponse = await client.PostAsync(url, content, cts.Token);

            }
            catch (HttpRequestException)
            {
                Console.WriteLine("ERROR");
                return;
            }

            if (httpResponse.IsSuccessStatusCode)
            {
                var item = JsonConvert.DeserializeObject<ResultClassification>(httpResponse.Content.ReadAsStringAsync().Result);
                model.CreateResults(item);
                    

            }
   
        }

      

        public void CancelClickHandler(object sender, RoutedEventArgs e)
        {

            cts.Cancel(false);
            cts.Dispose();
            cts = new CancellationTokenSource();

        }

        public void StatClickHandler(object sender, RoutedEventArgs e)
        {

            Thread t = new Thread(() => {
                try
                {
                    var httpResponse = client.GetAsync(url).Result;
                    string[] stats = JsonConvert.DeserializeObject<string[]>(httpResponse.Content.ReadAsStringAsync().Result);
                    model.Statistics(stats);
                    

                }
                catch (AggregateException)
                {
                    Console.WriteLine("ERROR_STAT");
                }
            });
            t.Start();

        }

        public void ClearClickHandler(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(() => {
                try
                {
                    var httpResponse = client.DeleteAsync(url).Result;
                    
                }
                catch (AggregateException)
                {
                    Console.WriteLine("ERROR_CLEAR");
                }
            });
            t.Start();

        }


    }
}