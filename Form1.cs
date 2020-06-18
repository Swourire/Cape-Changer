using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Cape_Setter
{
    public partial class Form1 : Form
    {
        public JsonObj jsonDataObj;

        public Form1()
        {
            InitializeComponent();

            string path = Directory.GetCurrentDirectory() + "/settings.json";

            if (File.Exists(path))
            {
                jsonDataObj = JsonConvert.DeserializeObject<JsonObj>(File.ReadAllText(path));
                if(jsonDataObj is null)
                {
                    jsonDataObj = new JsonObj();
                } else label2.Text = "Vanilla Skin Pack Path: " + jsonDataObj.vanillaPath;
            }
            else
            {
                File.Create(path);
                jsonDataObj = new JsonObj();
            }       
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string callbackPath = "";
            using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog() { SelectedPath= @"C:\Program Files\WindowsApps" })
            {
                if (folderBrowser.ShowDialog() == DialogResult.OK) callbackPath = folderBrowser.SelectedPath;
            }

            jsonDataObj.vanillaPath = callbackPath;

            label2.Text = "Vanilla Skin Pack Path: " + callbackPath;

            string path = Directory.GetCurrentDirectory() + "/settings.json";

            if (File.Exists(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                using (StreamWriter sw = new StreamWriter(path))
                    serializer.Serialize(sw, jsonDataObj);
            }
        }

        private void applyChange_Click(object sender, EventArgs e)
        {
            if(!File.Exists(jsonDataObj.vanillaPath + "geometry.json"))
            {
                File.Copy(Directory.GetCurrentDirectory() + "geometry.json", jsonDataObj.vanillaPath);
            }
        }
    }

    public class JsonObj
    {
        public string vanillaPath;
    }
}
