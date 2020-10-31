using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using Microsoft.Win32;
using System.Drawing;
using System.IO;
using System.Xml.Linq;

namespace FoodRecipeApp
{
    /// <summary>
    /// Interaction logic for AddRecipeWindow.xaml
    /// </summary>
    public partial class AddRecipeWindow : Window
    {

        class Step : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public int ID { get; set; }                 //ID món ăn
            public int Order { get; set; }              //thứ tự bước

            private string _imagePath;                  //đường dẫn ảnh của bước
            public string ImagePath
            {
                get
                {
                    return _imagePath;
                }
                set
                {
                    _imagePath = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("ImagePath"));
                    }
                }
            }
            public string Content { get; set; }         //mô tả bước
        }

        BindingList<Step> ListSteps;
        public MainWindow.Food newFood;                     //Món ăn thêm

        public AddRecipeWindow()
        {
            InitializeComponent();
            ListSteps = new BindingList<Step>();
        }

        public AddRecipeWindow(MainWindow.Food food)
        {

            InitializeComponent();
            ListSteps = new BindingList<Step>();
            newFood = food;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            newFood = new MainWindow.Food() { ID = newFood.ID, Level = "Dễ" };
            this.DataContext = newFood;
            lvSteps.ItemsSource = ListSteps;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }


        private void btnAddStep_Click(object sender, RoutedEventArgs e)
        {
            if (ListSteps.Count == 0)
            {
                var newStep = new Step() { ID = newFood.ID, Order = 1 };
                ListSteps.Add(newStep);
            }
            else
            {
                var newStep = new Step() { ID = newFood.ID, Order = ListSteps[ListSteps.Count - 1].Order + 1 };
                ListSteps.Add(newStep);
            }

        }

        private void btnAddFoodPic_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image Files(*.JPG*)|*.JPG";
            fileDialog.Title = "Select Image";

            if (fileDialog.ShowDialog() == true)
            {
                string filePath = fileDialog.FileName;
                newFood.ImagePath = filePath;
            }
        }

        private void btnAddStepPic_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image Files(*.JPG*)|*.JPG";
            fileDialog.Title = "Select Image";
            if (fileDialog.ShowDialog() == true)
            {
                var listViewItem = FindParent<ListViewItem>(sender as DependencyObject);
                string filePath = fileDialog.FileName;
                if (listViewItem != null)
                {
                    var currData = listViewItem.DataContext as Step;
                    currData.ImagePath = filePath;
                }
            }
        }
        private static T FindParent<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject);

            if (parent == null) return null;

            var parentT = parent as T;
            return parentT ?? FindParent<T>(parent);
        }

        /* Get current app domain*/
        public static string GetAppDomain()
        {
            string absolutePath;
            absolutePath = AppDomain.CurrentDomain.BaseDirectory;
            return absolutePath;
        }

        private void SaveStepsImages()
        {
            string newPath;
            string newFolder = GetAppDomain();
            foreach (Step step in ListSteps)
            {
                string stepPicName = $"{step.ID}_{step.Order.ToString()}.jpg";
                newPath = $"{newFolder}\\images\\{stepPicName}";
                File.Copy(step.ImagePath, newPath);
                step.ImagePath = $"images\\{stepPicName}";
            }
        }

        private void SaveFoodData()
        {
            string folderPath = GetAppDomain();
            string filePath = $"{folderPath}data\\Food.xml";
            XDocument doc = XDocument.Load(filePath);
            doc.Element("ArrayOfFood").Add(
                    new XElement
                    (
                            "Food",
                            new XElement("ID", newFood.ID),
                            new XElement("Name", newFood.Name),
                            new XElement("Ingredients", newFood.Ingredients),
                            new XElement("IsFavorite", newFood.IsFavorite),
                            new XElement("DateAdd", newFood.DateAdd),
                            new XElement("ImagePath", newFood.ImagePath),
                            new XElement("VideoLink", newFood.VideoLink)
                    )
            );
            doc.Save(filePath);
        }

        private void SaveStepData()
        {
            string folderPath = GetAppDomain();
            string filePath = $"{folderPath}data\\Step.xml";
            XDocument doc = XDocument.Load(filePath);
            foreach (Step step in ListSteps)
            {
                doc.Element("ArrayOfStep").Add(
                        new XElement
                        (
                                "Step",
                                new XElement("ID", step.ID),
                                new XElement("Order", step.Order),
                                new XElement("ImagePath", step.ImagePath),
                                new XElement("Content", step.Content)
                        )
                );
            }
            doc.Save(filePath);
        }

        private void btnSaveFood_Click(object sender, RoutedEventArgs e)
        {
            newFood.DateAdd = DateTime.Now;

            string newFolder = GetAppDomain();
            string foodPicName = $"{newFood.ID}.jpg";
            string newPath = $"{newFolder}\\images\\{foodPicName}";

            File.Copy(newFood.ImagePath, newPath);
            newFood.ImagePath = $"images\\{foodPicName}";

            SaveFoodData();

            SaveStepsImages();

            SaveStepData();

            this.DialogResult = true;
            this.Close();
        }
    }
}
