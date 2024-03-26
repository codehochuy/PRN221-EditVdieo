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
using System.IO;
using System.Diagnostics;
using IOPath = System.IO.Path;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Threading;




namespace Video
{
    public partial class MainWindow : Window
    {
        DispatcherTimer timer;
        private MediaElement mergedMedia;
        private string audioFilePath = "";
        private List<string> cutVideoPaths = new List<string>();
        private List<string> muteVideoPaths = new List<string>();
        private List<Grid> selectedGrids = new List<Grid>();

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(500);
            timer.Tick += new EventHandler(Timer_Tick);
            mergedMedia = new MediaElement();
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


        private double[] startTimes = new double[5];
        private double[] endTimes = new double[5];
        private void btnCutVideo_Click(object sender, RoutedEventArgs e)
        {
            if (media.Source != null)
            {
                TimeSpan totalDuration = media.NaturalDuration.TimeSpan;
                double segmentDuration = totalDuration.TotalSeconds / 5; 

                gridSegments1.Children.Clear();
                gridSegments2.Children.Clear();
                gridSegments3.Children.Clear();
                gridSegments4.Children.Clear();
                gridSegments5.Children.Clear();
                for (int i = 0; i < 5; i++)
                {

                    startTimes[i] = i * segmentDuration;
                    endTimes[i] = startTimes[i] + segmentDuration;
                    MediaElement segmentMedia = new MediaElement();
                    segmentMedia.Source = media.Source;
                    segmentMedia.LoadedBehavior = MediaState.Manual;
                    segmentMedia.UnloadedBehavior = MediaState.Manual;
                    segmentMedia.Volume = media.Volume;
                    segmentMedia.Position = TimeSpan.FromSeconds(startTimes[i]);
                    segmentMedia.Play();
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
                    string cutVideoPath = IOPath.Combine("D:\\Desktop\\Merge Video", $"segment_{i}.mp4");
                    cutVideoPaths.Add(cutVideoPath);
                    try
                    {
                        File.Copy(media.Source.OriginalString, cutVideoPath, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Không thể sao chép video: {ex.Message}");
                    }
                }
                CutVideos();
            }
            else
            {
                MessageBox.Show("Chưa chọn video.");
            }
       
        }
        private void CutVideos()
        {
            for (int i = 0; i < 5; i++)
            {
                string inputFilePath = IOPath.Combine("D:\\Desktop\\Merge Video", $"segment_{i}.mp4");
                string outputFilePath = IOPath.Combine("D:\\Desktop\\Merge Video", $"cut_segment_{i}.mp4");
                string startTime = TimeSpan.FromSeconds(startTimes[i]).ToString();
                string endTime = TimeSpan.FromSeconds(endTimes[i]).ToString();

                string arguments = $"-i \"{inputFilePath}\" -ss {startTime} -to {endTime} -c copy \"{outputFilePath}\"";
                /*string arguments = $"-i \"{inputFilePath}\" -ss {startTime} -to {endTime} -c:v copy -an \"{outputFilePath}\"";*/

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = @"D:\Downloads\ffmpeg2\bin\ffmpeg.exe"; 
                startInfo.Arguments = arguments;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;

                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();

                process.WaitForExit();
                muteVideoPaths.Add(outputFilePath);
            }
        }


        private void GridSegment_Click(object sender, MouseButtonEventArgs e)
        {
            Grid clickedGrid = (Grid)sender;
            int gridIndex = 0;

            // Determine the index of the clicked grid
            if (clickedGrid == gridSegments1)
                gridIndex = 0;
            else if (clickedGrid == gridSegments2)
                gridIndex = 1;
            else if (clickedGrid == gridSegments3)
                gridIndex = 2;
            else if (clickedGrid == gridSegments4)
                gridIndex = 3;
            else if (clickedGrid == gridSegments5)
                gridIndex = 4;

            /* if (selectedGrids.Contains(clickedGrid))
             {
                 selectedGrids.Remove(clickedGrid);
                 clickedGrid.Background = Brushes.Transparent;
             }*/
            if (selectedGrids.Contains(clickedGrid))
            {
                selectedGrids.Remove(clickedGrid);
                clickedGrid.Background = Brushes.Transparent;
                TextBlock textBlock = clickedGrid.Children.OfType<TextBlock>().FirstOrDefault();
                if (textBlock != null)
                {
                    clickedGrid.Children.Remove(textBlock);
                }
            }

            else
            {
                selectedGrids.Add(clickedGrid);
                clickedGrid.Background = Brushes.LightBlue;
                TextBlock textBlock = new TextBlock();
                textBlock.Text = (selectedGrids.Count).ToString();
                textBlock.Foreground = Brushes.Black;
                textBlock.FontSize = 12;
                textBlock.Margin = new Thickness(5, 5, 0, 0);
                clickedGrid.Children.Add(textBlock);
            }

            SaveSelectedVideos();
        }

        private void SaveSelectedVideos()
        {
            cutVideoPaths.Clear(); 
            foreach (var grid in selectedGrids)
            {
                int gridIndex = 0;
                if (grid == gridSegments1)
                    gridIndex = 0;
                else if (grid == gridSegments2)
                    gridIndex = 1;
                else if (grid == gridSegments3)
                    gridIndex = 2;
                else if (grid == gridSegments4)
                    gridIndex = 3;
                else if (grid == gridSegments5)
                    gridIndex = 4;

                string videoPath = muteVideoPaths[gridIndex];
                cutVideoPaths.Add(videoPath);
            }
        }
        private void btnAddAudio_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Audio files (*.mp3)|*.mp3|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                audioFilePath = openFileDialog.FileName;
                string destinationFilePath = @"D:\Desktop\Merge Video\audio.mp3";

                try
                {
                    File.Copy(audioFilePath, destinationFilePath, true);
                    MessageBox.Show("Tệp âm thanh đã được sao chép thành công đến " + destinationFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể sao chép tệp âm thanh: " + ex.Message);
                }
            }
        }
        private void btnMergeVideo_Click(object sender, RoutedEventArgs e)
        {
            if (selectedGrids.Count > 0)
            {
                try
                {
                    string mergedVideoPath = MergeVideos(cutVideoPaths);
                    OverlayAudio(mergedVideoPath, audioFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unhandled exception occurred: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Chưa có phân đoạn video nào được chọn.");
            }
        }
        private string OverlayAudio(string mergedVideoPath, string audioFilePath)
        {
            string outputDirectory = @"D:\Desktop\Merge Video";
            string outputFileName = "video_with_audio.mp4";
            string outputFilePath = IOPath.Combine(outputDirectory, outputFileName);

            string arguments = $"-i \"{mergedVideoPath}\" -i \"{audioFilePath}\" -c:v copy -c:a aac -strict experimental \"{outputFilePath}\"";
            string ffmpegPath = @"D:\Downloads\ffmpeg2\bin\ffmpeg.exe";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                    {
                        return outputFilePath;
                    }
                    else
                    {
                        throw new Exception("Đã xảy ra lỗi khi ghi đè âm thanh vào video.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi thực thi ffmpeg: " + ex.Message);
            }
        }


        private string MergeVideos(List<string> videoPaths)
        {
            string outputDirectory = @"D:\Desktop\Merge Video"; 
            string outputFileName = "merged_video.mp4"; 
            string outputFilePath = IOPath.Combine(outputDirectory, outputFileName);
            string tempFilePath = IOPath.Combine(outputDirectory, "temp.txt");
            string arguments = $"-f concat -safe 0 -i \"{tempFilePath}\" -c:v copy -an \"{outputFilePath}\"";
            string ffmpegPath = @"D:\Downloads\ffmpeg2\bin\ffmpeg.exe";
            using (StreamWriter writer = new StreamWriter(tempFilePath))
            {
                foreach (string videoPath in videoPaths)
                {
                    writer.WriteLine($"file '{videoPath}'");
                }
            }
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            StringBuilder outputBuilder = new StringBuilder();
            StringBuilder errorBuilder = new StringBuilder();
            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    process.OutputDataReceived += (sender, e) => { outputBuilder.AppendLine(e.Data); };
                    process.ErrorDataReceived += (sender, e) => { errorBuilder.AppendLine(e.Data); };
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    string logDirectory = @"D:\Desktop\Merge Video";
                    File.WriteAllText(IOPath.Combine(logDirectory, "output.log"), outputBuilder.ToString());
                    File.WriteAllText(IOPath.Combine(logDirectory, "error.log"), errorBuilder.ToString());
                    if (!process.WaitForExit(2000))
                    {
                        process.Kill();
                        throw new Exception("Quá trình chạy đã vượt quá thời gian cho phép.");
                    }

                    if (process.ExitCode == 0)
                    {
                        return outputFilePath;
                    }
                    else
                    {
                        throw new Exception("Đã xảy ra lỗi khi gộp video.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi thực thi ffmpeg: " + ex.Message);
            }
        }
    }
}

  
    
