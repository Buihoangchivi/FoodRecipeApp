using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Configuration;
using System.Timers;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Media.Animation;

namespace FoodRecipeApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public string PicPath { get; set; }
        Random rng = new Random();

        public SplashScreen()
        {
            var value = ConfigurationManager.AppSettings["ShowSplashScreen"];
            var status = bool.Parse(value);


            if (status == false)
            {
                var screen = new MainWindow();
                screen.Show();
                this.Close();
            }
            InitializeComponent();
        }

        public string Select { get; set; }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            
            int sel = rng.Next(4);
            string sel_c = sel.ToString();
            var folder = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = $"{folder}data/splashInfo.txt";
            PicPath = $"{folder}images/splash_{sel_c}.jpg";
            this.DataContext = this;
            var splashInfo = File.ReadAllLines(filePath);
            AmazingInfoTextBlock.Text = splashInfo[sel];
        }


        private void NotShowSplashScreen_check(object sender, RoutedEventArgs e)
        {
            var config = ConfigurationManager.OpenExeConfiguration(
                 ConfigurationUserLevel.None);
            config.AppSettings.Settings["ShowSplashScreen"].Value = "false";
            config.Save(ConfigurationSaveMode.Minimal);
        }


        private void NotShowSplashScreen_uncheck(object sender, RoutedEventArgs e)
        {
            var config = ConfigurationManager.OpenExeConfiguration(
                 ConfigurationUserLevel.None);
            config.AppSettings.Settings["ShowSplashScreen"].Value = "true";
            config.Save(ConfigurationSaveMode.Minimal);
        }



        private void ContinueButton_Completed(object sender, EventArgs e)
        {
            var screen = new MainWindow();
            this.Close();
            screen.Show();
        }

    }
}
