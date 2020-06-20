using Cape_Changer.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cape_Changer
{
    public partial class CapeChanger : Window
    {
        public string minecraftSkinPath;
        public string minecraftStartPath;
        public string selectedCape;
        public string selectedSkin;

        public CapeChanger()
        {
            minecraftSkinPath = FindSkinPackPath();
            if (minecraftSkinPath == "") Process.GetCurrentProcess().Kill();

            selectedSkin = Path.Combine(minecraftSkinPath, @"\steve.png");
            InitializeComponent();

            // Add all capes in the list :
            foreach (string capePath in Directory.GetFiles(GetCapePath()))
            {
                AddCapeInDisplayedList(Image.FromFile(capePath), capePath);
            }

            // Define the update button color :
            ChangeUpdateButtonColor();

            // Display the skin head :
            DisplaySkin();
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

        public void RemoveCape(object sender, RoutedEventArgs e)
        {
            foreach(System.Windows.Controls.Button button in CapesList.Children)
            {
                if(button.Uid == selectedCape)
                {
                    button.Visibility = Visibility.Collapsed;

                    File.Delete(selectedCape);
                }
            }
        }

        private void AddCape(object sender, RoutedEventArgs e)
        {
            // Verify if the cape format is good :
            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Title = "Cape Texture";
            fileBrowser.Filter = "Image Files|*.jpg;*.jpeg;*.png;";
            
            if (fileBrowser.ShowDialog() == true)
            {
                // Verify if the file size is valid :
                Image image = Image.FromFile(fileBrowser.FileName);

                if (image.Width != 64 || image.Height != 32)
                {
                    MessageBox.Show("Please put a valid cape format.");
                    return;
                }

                // Copy the cape in cape list folder :
                string capePath = fileBrowser.FileName;
                string capeName = Path.GetFileName(capePath);

                string finalPath = Path.Combine(GetCapePath(), @capeName);

                File.Copy(capePath, finalPath);

                // Add cape in the displayed list :
                AddCapeInDisplayedList(Image.FromFile(capePath), capePath);
            }
        }

        private void SelectCape(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = (System.Windows.Controls.Button)sender;

            selectedCape = button.Uid;

            foreach (System.Windows.Controls.Button buttonOfList in CapesList.Children)
            {
                if (buttonOfList.Uid == selectedCape)
                {
                    buttonOfList.Background = new SolidColorBrush((System.Windows.Media.Color)System.
                            Windows.Media.ColorConverter.ConvertFromString("#55FF55"));
                }
                else
                {
                    buttonOfList.Background = new SolidColorBrush((System.Windows.Media.Color)System.
                            Windows.Media.ColorConverter.ConvertFromString("#FF5555"));
                }
            }

            ChangeUpdateButtonColor();
        }

        private void SelectSkin(object sender, RoutedEventArgs e)
        {
            File.Delete(selectedSkin);

            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Title = "Skin Texture";
            fileBrowser.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

            if (fileBrowser.ShowDialog() == true)
            {
                // Add the skin in capes list and interface :
                File.Copy(fileBrowser.FileName, selectedSkin);

                DisplaySkin();
            }
        }

        private void Update(object sender, RoutedEventArgs e)
        {
            string pathGeometry = minecraftSkinPath + "/geometry.json";
            string pathSkins = minecraftSkinPath + "/skins.json";
            string pathCape = minecraftSkinPath + "/cape.png";
            string localPathSkins = Directory.GetCurrentDirectory() + "/resource/internal/skins.json";

            if (!File.Exists(pathGeometry))
            {
                File.Copy(Directory.GetCurrentDirectory() + "/resource/internal/geometry.json", pathGeometry);
            }

            StreamReader stream1 = new StreamReader(pathSkins);
            StreamReader stream2 = new StreamReader(localPathSkins);

            if (!File.Exists(pathSkins))
            {
                File.Copy(localPathSkins, pathSkins);
            }
            else if (stream1.ReadToEnd() != stream2.ReadToEnd())
            {
                stream1.Close();
                stream2.Close();
                File.Delete(pathSkins);
                File.Copy(localPathSkins, pathSkins);
            }

            if (File.Exists(pathCape))
            {
                try
                {
                    File.Delete(pathCape);
                }
                catch (IOException)
                {
                    List<Process> processes = FileUtil.WhoIsLocking(pathCape);
                    foreach (Process process in processes)
                    {
                        Debug.WriteLine(process);
                    }
                }
            }

            File.Copy(selectedCape, pathCape);
            RestartMinecraft();
        }

        private void DisplaySkin()
        {
            BitmapImage bitmapImage;

            if (File.Exists(selectedSkin))
            {
                bitmapImage = ImageUtils.ImageToBitmapImage(ImageUtils.CropImage(Image.FromFile(selectedSkin), new Rectangle(8, 8, 8, 8)));
            } 
            else
            {
                Bitmap bitmap = new Bitmap(8, 8);

                for (int x = 0; x < bitmap.Width; x++) for (int y = 0; y < bitmap.Height; y++) 
                        bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(255, 255, 85));

                bitmapImage = ImageUtils.BitmapToBitmapImage(bitmap);
            }

            System.Windows.Controls.Image image = new System.Windows.Controls.Image();

            image.Source = bitmapImage;

            image.HorizontalAlignment = HorizontalAlignment.Right;
            image.VerticalAlignment = VerticalAlignment.Center;

            image.Margin = new Thickness(0, 0, 150, 0);

            Skin.Children.Add(image);
        }

        private void AddCapeInDisplayedList(Image image, string imagePath)
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
                    newImage.SetPixel(x, y, imageInBitmap.GetPixel(x >> (int) Math.Sqrt(multiplier), y >> (int) Math.Sqrt(multiplier)));
                }
            }

            // Create image controle :
            System.Windows.Controls.Image cape = new System.Windows.Controls.Image();

            cape.Source = ImageUtils.BitmapToBitmapImage(newImage);

            // Create the button controle and add image in his content :
            System.Windows.Controls.Button button = new System.Windows.Controls.Button();

            button.Content = cape;

            button.Width = newWidth;
            button.Height = newHeight;

            button.Margin = new Thickness(0);
            button.Padding = new Thickness(16);

            button.Foreground = null;
            button.BorderBrush = null;

            button.Click += SelectCape;

            button.Background = new SolidColorBrush((System.Windows.Media.Color)System.
                Windows.Media.ColorConverter.ConvertFromString("#FF5555"));

            button.Uid = imagePath;

            // Add the controle in the cape list :
            CapesList.Children.Add(button);
        }

        private void ChangeUpdateButtonColor()
        {
            string color;

            if (File.Exists(selectedCape))
            {
                color = "#55FF55";
            }
            else
            {
                color = "#FF5555";
            }

            UpdateButton.Background = new SolidColorBrush((System.Windows.Media.Color)System.
                            Windows.Media.ColorConverter.ConvertFromString(color));
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
