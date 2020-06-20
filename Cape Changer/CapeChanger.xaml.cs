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

            selectedSkin = minecraftSkinPath + @"\steve.png";
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
            string pathResourcePack = minecraftStartPath + @"\data\resource_packs\vanilla\ui";

            string localPathSkins = Directory.GetCurrentDirectory() + "/resource/internal/skins.json";
            string localUiDefs = Directory.GetCurrentDirectory() + "/resource/internal/_ui_defs.json";
            string distantUiDefs = pathResourcePack + "/_ui_defs.json";
            string localStartScreen = Directory.GetCurrentDirectory() + "/resource/internal/start_screen.json";
            string distantStartScreen = pathResourcePack + "/start_screen.json";
            
            if (!File.Exists(pathGeometry))
            {
                File.Copy(Directory.GetCurrentDirectory() + "/resource/internal/geometry.json", pathGeometry);
            }

            if (!File.Exists(pathSkins))
            {
                File.Copy(localPathSkins, pathSkins);
            }
            else if (CompareTwoFiles(localPathSkins, pathSkins))
            {
                File.Delete(pathSkins);
                File.Copy(localPathSkins, pathSkins);
            }

            if (!File.Exists(distantUiDefs))
            {
                File.Copy(localUiDefs, distantUiDefs);
            }else if (CompareTwoFiles(localUiDefs, distantUiDefs))
            {
                File.Delete(distantUiDefs);
                File.Copy(localUiDefs, distantUiDefs);
            }

            if (!File.Exists(distantStartScreen))
            {
                File.Copy(localStartScreen, distantStartScreen);
            }
            else if(CompareTwoFiles(localStartScreen, distantStartScreen))
            {
                File.Delete(distantStartScreen);
                File.Copy(localStartScreen, distantStartScreen);
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

            MessageBox.Show("The skin is available in your default skin list on the game, you can change it.");
        }

        private bool CompareTwoFiles(string path1, string path2)
        {
            StreamReader stream1 = new StreamReader(path1);
            StreamReader stream2 = new StreamReader(path2);
            bool returnVal = stream1.ReadToEnd() != stream2.ReadToEnd();
            stream1.Close();
            stream2.Close();

            return returnVal;
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
            image = ImageUtils.ReziseImage(image, 16, 16);

            // Create image controle :
            System.Windows.Controls.Image cape = new System.Windows.Controls.Image();

            cape.Source = ImageUtils.ImageToBitmapImage(image);

            // Create the button controle and add image in his content :
            System.Windows.Controls.Button button = new System.Windows.Controls.Button();

            button.Content = cape;

            button.Width = image.Width;
            button.Height = image.Height;

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
                minecraftStartPath = fileName.Remove(removePosition);

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
