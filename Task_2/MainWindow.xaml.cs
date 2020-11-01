using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System.IO;
using System.Collections.ObjectModel;
using System;





namespace Task_2
{
    public class MainWindow : Window
    {


        public ClassificationVM model;
        string path;

        public MainWindow()
        {
            InitializeComponent();

            model = new ClassificationVM();
            this.DataContext = model;
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

            model.ClassificationImages(path);


        }

        public void CancelClickHandler(object sender, RoutedEventArgs e)
        {

            model.StopClassificationImages();
            
        }

        


    }
}