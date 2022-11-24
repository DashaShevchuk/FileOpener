using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Xps.Packaging;
using PptApplication = Microsoft.Office.Interop.PowerPoint.Application;
using DocAplication = Microsoft.Office.Interop.Word.Application;
using DataFormats = System.Windows.Forms.DataFormats;
using MessageBox = System.Windows.Forms.MessageBox;
using TextRange = System.Windows.Documents.TextRange;
using MenuItem = System.Windows.Controls.MenuItem;
using System.Windows.Threading;
using System.Linq;
using Inline = System.Windows.Documents.Inline;
using Microsoft.Office.Interop.Word;
using Window = System.Windows.Window;

namespace WpfApp1
{
    public partial class HomePage : Window
    {
        DispatcherTimer timer;
        public HomePage()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += new EventHandler(timer_Tick);
            LoadManu();
        }

        private void LoadManu()
        {
            string directoriesPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Документи";
            string[] directories = Directory.GetDirectories(directoriesPath).Select(Path.GetFileName).ToArray();
            foreach(var directori in directories)
            {
                MenuItem manuItem = new MenuItem();
                if(directori.Length <= 22)
                {
                    manuItem.Template = (ControlTemplate)FindResource("Menu_SubMenu_Template");
                    manuItem.Header = directori;
                }
                else
                {
                    string[] words = directori.Split(' ');
                    string header = @"";
                    foreach(var word in words)
                    {
                        header += word + " ";
                        if(header.Length >= 15 && header.Length <= 22)
                        {
                            break;
                        }
                    }
                    header += '\n';
                    foreach(var word in words)
                    {
                        if (!header.Contains(word))
                        {
                            header += " " + word;
                        }
                    }
                    manuItem.Header = header;
                    manuItem.Template = (ControlTemplate)FindResource("Menu_SubMenu_Template2");
                }
                manu.Items.Add(manuItem);
                string filesPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Документи\\" + directori;
                string[] files = Directory.GetFiles(filesPath).Select(Path.GetFileName).ToArray();
                foreach (var file in files)
                {
                    MenuItem subManuItem = new MenuItem();
                    subManuItem.Header = file.ToString();
                    subManuItem.Template = (ControlTemplate)FindResource("Item_Template");
                    string fullFilePath = filesPath + "\\" + file;
                    subManuItem.Click += (sender, e) => ManuItem_Click(sender, e, fullFilePath) ;
                    manuItem.Items.Add(subManuItem);
                }
            }
        }

        private void ManuItem_Click(object sender, EventArgs e, string path)
        {
            string fileExtn = Path.GetExtension(path);
            switch (fileExtn)
            {
                case ".docx":
                    OpenFile(path);
                    break;
                case ".pptx":
                    OpenPresentation(path);
                    break;
                case ".mp4":
                    OpenVideo(path);
                    break;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            video.Value = player.Position.TotalSeconds;
        }

        private static XpsDocument ConvertPowerPointToXps(string pptFilename, string xpsFilename)
        {
            var pptApp = new PptApplication();

            var presentation = pptApp.Presentations.Open(pptFilename, MsoTriState.msoTrue, MsoTriState.msoFalse, MsoTriState.msoFalse);

            try
            {
                presentation.ExportAsFixedFormat(xpsFilename, PpFixedFormatType.ppFixedFormatTypeXPS);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to export to XPS format: " + ex);
            }
            finally
            {
                presentation.Close();
                pptApp.Quit();
            }
            
            return new XpsDocument(xpsFilename, FileAccess.Read);
        }

        private static XpsDocument ConvertDocToXps(string docFileName, string xpsFilename)
        {
            var docApp = new DocAplication();

            var document = docApp.Documents.Open(docFileName, MsoTriState.msoTrue, MsoTriState.msoFalse, MsoTriState.msoFalse);

            try
            {
                document.ExportAsFixedFormat(xpsFilename, WdExportFormat.wdExportFormatXPS);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to export to XPS format: " + ex);
            }
            finally
            {
                document.Close();
                docApp.Quit();
            }

            return new XpsDocument(xpsFilename, FileAccess.Read);
        }

        private void OpenFile(string path)
        {
            docViewer.Visibility = Visibility.Visible;
            plusScale.IsEnabled = true;
            minusScale.IsEnabled = true;
            scale.IsEnabled = true;
            closeFile.IsEnabled = true;
            var xpsFile = Path.GetTempPath() + Guid.NewGuid() + ".xps";
            var xpsDocument = ConvertDocToXps(path, xpsFile);
            docViewer.Document = xpsDocument.GetFixedDocumentSequence();
        }

        private void OpenPresentation(string path)
        {
            docViewer.Visibility = Visibility.Visible;
            plusScale.IsEnabled = true;
            scale.IsEnabled = true;
            minusScale.IsEnabled = true;
            closeFile.IsEnabled = true;
            var xpsFile = Path.GetTempPath() + Guid.NewGuid() + ".xps";
            var xpsDocument = ConvertPowerPointToXps(path, xpsFile);
            docViewer.Document = xpsDocument.GetFixedDocumentSequence();
        }

        private void OpenVideo(string path)
        {
            docViewer.Visibility = Visibility.Visible;
            videoPlayer.Visibility = Visibility.Visible;
            closeFile.IsEnabled = true;
            player.Source = new Uri(path);
            player.LoadedBehavior = MediaState.Manual;
            player.UnloadedBehavior = MediaState.Manual;
            player.Volume = (double)volume.Value;
            player.Play();
        }

        private void scale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (docViewer.Visibility == Visibility.Visible)
            {
                docViewer.Zoom = scale.Value;
            }
        }

        private void plusScale_Click(object sender, RoutedEventArgs e)
        {
            scale.Value += 0.5;
        }

        private void minusScale_Click(object sender, RoutedEventArgs e)
        {
            scale.Value -= 0.5;
        }

        private void closeFile_Click(object sender, RoutedEventArgs e)
        {
            if(docViewer.Visibility == Visibility.Visible)
            {
                docViewer.Document = null;
                docViewer.Visibility = Visibility.Hidden;
            }
            if(videoPlayer.Visibility == Visibility.Visible)
            {
                player.Stop();
                player.ClearValue(MediaElement.SourceProperty);
                videoPlayer.Visibility = Visibility.Hidden;
            }
            closeFile.IsEnabled = false;
        }

        private void play_Click(object sender, RoutedEventArgs e)
        {
            player.Play();
        }

        private void pause_Click(object sender, RoutedEventArgs e)
        {
            player.Pause();
        }

        private void restart_Click(object sender, RoutedEventArgs e)
        {
            player.Stop();
        }

        private void volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.Volume = (double)volume.Value;
        }

        private void video_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.Position = TimeSpan.FromSeconds(video.Value);
        }

        private void player_MediaOpened(object sender, RoutedEventArgs e)
        {
            TimeSpan timeSpan = player.NaturalDuration.TimeSpan;
            video.Maximum = timeSpan.TotalSeconds;
            timer.Start();
        }

        private void plusVolume_Click(object sender, RoutedEventArgs e)
        {
            volume.Value += 0.1;
        }

        private void minusVolume_Click(object sender, RoutedEventArgs e)
        {
            volume.Value -= 0.1;
        }
    }
}
