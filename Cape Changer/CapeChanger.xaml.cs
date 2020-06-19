using Cape_Changer.Utils;
using Microsoft.Win32;
using System;
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

        public CapeChanger()
        {
            /*minecraftSkinPath = FindSkinPackPath();
            if (minecraftSkinPath == "") Process.GetCurrentProcess().Kill();*/

            InitializeComponent();

            foreach (string capePath in Directory.GetFiles(GetCapePath()))
            {
                AddCapeInDisplayedList(capePath);
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
                AddCapeInDisplayedList(finalPath);
            }
        }

        private void SelectCape(object sender, RoutedEventArgs e)
        {
            string pathGeometry = minecraftSkinPath + "/geometry.json";
            string pathSkins = minecraftSkinPath + "/skins.json";
            string localPathSkins = Directory.GetCurrentDirectory() + "/resource/internal/skins.json";

            if (!File.Exists(pathGeometry))
            {
                File.Copy(Directory.GetCurrentDirectory() + "/resource/internal/geometry.json", pathGeometry);
            }

            if (!File.Exists(pathSkins) || (new StreamReader(pathSkins)).ReadToEnd() != (new StreamReader(localPathSkins).ReadToEnd()))
            {
                File.Copy(localPathSkins, pathSkins);
            }

            restartMinecraft();
        }

        /// <summary>
        /// Add a cape in the interface.
        /// </summary>
        /// <param name="capePath">the cape path</param>
        private void AddCapeInDisplayedList(string capePath)
        {
            System.Windows.Controls.Image cape = new System.Windows.Controls.Image();

            BitmapImage bitmapImage = new BitmapImage(new Uri(capePath));
            //Image image = ImageUtils.CropImage(Image.FromFile(capePath), new Rectangle(0, 0, 24, 18));

            cape.Source = bitmapImage;
            cape.Width = 500;
            cape.Height = 200;
            cape.Margin = new Thickness(15);

            //cape.Source = ImageUtils.ImageToBitmapImage(ImageUtils.PixelateImage(image));
            /*cape.Source = ImageUtils.ImageToBitmapImage(ImageUtils.Pixelate
                (ImageUtils.ResizeImage(image, image.Width, image.Height),
                new Rectangle(0, 0, image.Width, image.Height), (int)(image.Width * .25 + image.Height * .25) / 2));*/

            CapesList.Children.Add(cape);
        }

        private void restartMinecraft()
        {
            Process[] processes = Process.GetProcessesByName("Minecraft.Windows");

            if (processes.Length > 0)
            {
                processes[0].Kill();
            }
            Thread.Sleep(1000);
            Process.Start("minecraft://");
        }

        /*private string FindSkinPackPath()
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
        }*/
    }
}
