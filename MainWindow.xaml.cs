using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfThreadExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private static readonly string[] Extensions =
        { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

        private void Button_Click(object sender, RoutedEventArgs e)
        { 
            var dialog = new OpenFolderDialog();

            if (dialog.ShowDialog() != true)
                return;

            string folder = dialog.FolderName;
            PathText.Text = folder;

            //Запуск задачі за допомогою класу Task замість ручного створення потоку Thread
            Task.Run(() => MyUploadImagesFolder(folder));
        }

        //Метод виконується асинхронно в фоновому потоці з ThreadPool
        private void MyUploadImagesFolder(string folder)
        {
            //Повертаємо усі файли із папки, які є фотографіями
            var files = Directory.GetFiles(folder)
                .Where(f => Extensions.Contains(Path.GetExtension(f).ToLower()));

            var images = new List<BitmapImage>();
            foreach (var file in files)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(file);
                bitmap.DecodePixelWidth = 300; //Економія пам'яті
                bitmap.CacheOption = BitmapCacheOption.OnLoad; //Щоб не блокувати файл зображення
                bitmap.EndInit();
                bitmap.Freeze(); //Для того, щоб зображення можна було безпечно передати в інший потік
                images.Add(bitmap);
            }

            //Оновлення інтерфейсу користувача з фонового потоку через Dispatcher
            Dispatcher.Invoke(() =>
            {
                GalleryList.ItemsSource = images;
            });
        }
    }
}