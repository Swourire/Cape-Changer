using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace Cape_Changer
{
    /// <summary>
    /// Logique d'interaction pour CapeChanger.xaml
    /// </summary>
    public partial class CapeChanger : Window
    {
        public string minecraftSkinPath;
        public string minecraftStartPath;

        public CapeChanger()
        {
            minecraftSkinPath = FindSkinPackPath();
            if (minecraftSkinPath == "") Process.GetCurrentProcess().Kill();

            InitializeComponent();
        }

        private void AddCape(object sender, RoutedEventArgs e)
        {
            string callbackPath = "";
            OpenFileDialog fileBrowser = new OpenFileDialog();
            fileBrowser.Title = "Cape Texture";
            fileBrowser.Filter = "Image Files|*.jpg;*.jpeg;*.png;";
            if (fileBrowser.ShowDialog() == true)
            {
                callbackPath = fileBrowser.FileName;
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

        private string FindSkinPackPath()
        {
            Process[] processes = Process.GetProcessesByName("Minecraft.Windows");

            if (processes.Length > 0)
            {
                string fileName = processes[0].MainModule.FileName;
                int removePosition = fileName.Length - 21;
                minecraftStartPath = fileName;
                return fileName.Remove(removePosition) + @"data\skin_packs\vanilla";
            }
            else
            {
                MessageBox.Show("Start Minecraft, before Launching the application.");
                return "";
            }
        }
    }
}
