using Cape_Changer.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Cape_Changer
{
    public partial class CapeChanger : Window
    {
        public string minecraftSkinPath;
        public string minecraftStartPath;
        public string selectedCape;

        public CapeChanger()
        {
            minecraftSkinPath = FindSkinPackPath();
            if (minecraftSkinPath == "") Process.GetCurrentProcess().Kill();

            InitializeComponent();

            selectedCape = GetCapePath() + "cape_zizi.png";
            foreach (string capePath in Directory.GetFiles(GetCapePath()))
            {
                AddCapeInDisplayedList(Image.FromFile(capePath));
            }
        }

        private string GetCapePath()
        {
            string path = Directory.GetCurrentDirectory();

            path = Path.Combine(path, @"capes\");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        private void AddCape(object sender, RoutedEventArgs e)
        {
            // Verify if the cape format is good :
            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Title = "Cape Texture";
            fileBrowser.Filter = "Image Files|*.jpg;*.jpeg;*.png;";
            
            if (fileBrowser.ShowDialog() == true)
            {
                // Copy the cape in cape list folder :
                string capePath = fileBrowser.FileName;
                string capeName = Path.GetFileName(capePath);

                string finalPath = Path.Combine(GetCapePath(), @capeName);

                File.Copy(capePath, finalPath);

                // Add cape in the displayed list :
                AddCapeInDisplayedList(Image.FromFile(capePath));
            }
        }

        private void SelectCape(object sender, RoutedEventArgs e)
        {
            string pathGeometry = minecraftSkinPath + "/geometry.json";
            string pathSkins = minecraftSkinPath + "/skins.json";
            string pathCape = minecraftSkinPath + "/cape.png";
            string localPathSkins = Directory.GetCurrentDirectory() + "/resource/internal/skins.json";

            if (!File.Exists(pathGeometry))
            {
                Debug.WriteLine(pathGeometry);
                Debug.WriteLine(Directory.GetCurrentDirectory());
                File.Copy(Directory.GetCurrentDirectory() + "/resource/internal/geometry.json", pathGeometry);
            }

            StreamReader stream1 = new StreamReader(pathSkins);
            StreamReader stream2 = new StreamReader(localPathSkins);

            if (!File.Exists(pathSkins))
            {
                File.Copy(localPathSkins, pathSkins);
            }
            else if(stream1.ReadToEnd() != stream2.ReadToEnd())
            {
                stream1.Close();
                stream2.Close();
                File.Delete(pathSkins);
                File.Copy(localPathSkins, pathSkins);
            }

            if(File.Exists(pathCape))
            {
                try
                {
                    File.Delete(pathCape);
                }
                catch(System.IO.IOException)
                {
                   List<Process> processes = FileUtil.WhoIsLocking(pathCape);
                    foreach(Process process in processes)
                    {
                        Debug.WriteLine(process);
                    }
                }
            }

            File.Copy(selectedCape, pathCape);
            RestartMinecraft();
        }

        /// <summary>
        /// Add a cape in the interface.
        /// </summary>
        /// <param name="capePath">the cape path</param>
        private void AddCapeInDisplayedList(Image image)
        {

            // Get the cape image section width and height :
            int width = image.Width * 24 / image.Width;
            int height = image.Height * 18 / image.Height;

            // Crop the image with section sizes :
            image = ImageUtils.CropImage(image, new Rectangle(0, 0, width, height));

            // Resize image :
            int multiplier = 16;

            int newWidth = width * multiplier - multiplier;
            int newHeight = height * multiplier - multiplier;

            Bitmap imageInBitmap = (Bitmap)image;
            Bitmap newImage = new Bitmap(newWidth, newHeight);

            for(int x = 0; x < newWidth; x++)
            {
                for(int y = 0; y < newHeight; y++)
                {
                    newImage.SetPixel(x, y, imageInBitmap.GetPixel(x >> (int) Math.Sqrt(multiplier), y >> (int) Math.Sqrt(multiplier) ));
                }
            }

            // Create the controle :
            //System.Windows.Controls.Button button = new System.Windows.Controls.Button();
            System.Windows.Controls.Image cape = new System.Windows.Controls.Image();

            cape.Source = ImageUtils.BitmapToBitmapImage(newImage);

            cape.Width = newWidth;
            cape.Height = newHeight;

            cape.Margin = new Thickness(15);

            // Add the controle in the cape list :
            CapesList.Children.Add(cape);
        }

        private void RestartMinecraft()
        {
            Process[] processes = Process.GetProcessesByName("Minecraft.Windows");

            if (processes.Length > 0)
            {
                processes[0].Kill();
            }
            Thread.Sleep(1000);
            Process.Start("minecraft://");
        }

        private string FindSkinPackPath()
        {
            Process[] processes = Process.GetProcessesByName("Minecraft.Windows");

            if (processes.Length > 0)
            {
                string fileName = processes[0].MainModule.FileName;
                int removePosition = fileName.Length - 21;
                minecraftStartPath = fileName;

                return Path.Combine(fileName.Remove(removePosition), @"data\skin_packs\vanilla");
            }
            else
            {
                MessageBox.Show("Start Minecraft, before Launching the application.");
                return "";
            }
        }
    }
}
