using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
namespace Video
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(500);
            timer.Tick += new EventHandler(Timer_Tick);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            Seek.Value = media.Position.TotalSeconds;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            media.Play(); 
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            media.Pause();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            media.Stop();
            Seek.Value = 0;
        }

        private void Vol_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            media.Volume = (double)Vol.Value;
        }

        private void Seek_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (media.NaturalDuration.HasTimeSpan)
            {
                media.Position = TimeSpan.FromSeconds(Seek.Value);
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string fileName = (string)((DataObject)e.Data).GetFileDropList()[0];
            media.Source = new Uri(fileName);

            media.LoadedBehavior = MediaState.Manual;
            media.UnloadedBehavior = MediaState.Manual; 
            media.Volume = (double)(Vol.Value);
            media.Play();
        }

        private void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            TimeSpan ts = media.NaturalDuration.TimeSpan;
            Seek.Maximum = ts.TotalSeconds;
            timer.Start();
        }

        private void btnSelectVideo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video files (*.mp4, *.avi)|*.mp4;*.avi|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFileName = openFileDialog.FileName;
                media.Source = new Uri(selectedFileName);
                media.LoadedBehavior = MediaState.Manual;
                media.UnloadedBehavior = MediaState.Manual;
                media.Volume = (double)Vol.Value;
                media.Play();
            }
        }

        private void btnCutVideo_Click(object sender, RoutedEventArgs e)
        {
            if (media.Source != null)
            {
                // Tính toán thời lượng của video
                TimeSpan totalDuration = media.NaturalDuration.TimeSpan;
                double segmentDuration = totalDuration.TotalSeconds / 5; // Chia video thành 5 đoạn bằng nhau

                // Xóa bỏ các đoạn video cũ trong các Grid
                gridSegments1.Children.Clear();
                gridSegments2.Children.Clear();
                gridSegments3.Children.Clear();
                gridSegments4.Children.Clear();
                gridSegments5.Children.Clear();

                // Hiển thị 5 đoạn video lên màn hình
                for (int i = 0; i < 5; i++)
                {
                    double startTime = i * segmentDuration;
                    double endTime = startTime + segmentDuration;

                    // Tạo một đối tượng MediaElement mới để hiển thị đoạn video
                    MediaElement segmentMedia = new MediaElement();
                    segmentMedia.Source = media.Source;
                    segmentMedia.LoadedBehavior = MediaState.Manual;
                    segmentMedia.UnloadedBehavior = MediaState.Manual;
                    segmentMedia.Volume = media.Volume;

                    // Cắt đoạn video và thiết lập thời gian bắt đầu và kết thúc
                    segmentMedia.Position = TimeSpan.FromSeconds(startTime);
                    segmentMedia.Play();

                    // Thêm đoạn video vào Grid tương ứng
                    switch (i)
                    {
                        case 0:
                            gridSegments1.Children.Add(segmentMedia);
                            break;
                        case 1:
                            gridSegments2.Children.Add(segmentMedia);
                            break;
                        case 2:
                            gridSegments3.Children.Add(segmentMedia);
                            break;
                        case 3:
                            gridSegments4.Children.Add(segmentMedia);
                            break;
                        case 4:
                            gridSegments5.Children.Add(segmentMedia);
                            break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Chưa chọn video.");
            }
        }





    }
}