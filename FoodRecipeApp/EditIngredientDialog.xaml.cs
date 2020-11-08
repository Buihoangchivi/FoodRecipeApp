using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace FoodRecipeApp
{
    /// <summary>
    /// Interaction logic for EditIngredientDialog.xaml
    /// </summary>
    public partial class EditIngredientDialog : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public EditIngredientDialog()
        {
            InitializeComponent();
        }
        /*Lấy màu từ MainWindow*/
        private string _colorScheme;
        public string ColorScheme
        {
            get
            {
                return _colorScheme;
            }
            set
            {
                _colorScheme = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ColorScheme"));
                }
            }
        }

        
        private string _ingredientName;
        public string IngredientName
        {
            get
            {
                return _ingredientName;
            }
            set
            {
                _ingredientName = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IngredientName"));
                }
            }
        }

        private string _ingredientType;
        public string IngredientType
        {
            get
            {
                return _ingredientType;
            }
            set
            {
                _ingredientType = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IngredientType"));
                }
            }
        }

        public EditIngredientDialog(string colorScheme, MainWindow.Ingredient ingredient)
        {
            ColorScheme = colorScheme;
            IngredientName = ingredient.IngredientName as string;
            IngredientType = ingredient.Type as string;
            InitializeComponent();
            for(int i = 0; i < IngredientTypeCombobox.Items.Count; i++)
            {
                var curItem = IngredientTypeCombobox.Items[i] as ComboBoxItem;
                
                if ((string)curItem.Content == IngredientType)
                {
                    IngredientTypeCombobox.SelectedIndex = i;
                    break;
                }
            }
            this.DataContext = this;

        }

        private void DeleteIngredientButton_Click(object sender, RoutedEventArgs e)
        {
            //Nếu nhấn delete thì Dialog Result bằng false báo cho Mainwindow xoá
            this.DialogResult = false;
            this.Close();
        }

        private void SaveChangeIngredientButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedType = IngredientTypeCombobox.SelectedItem as ComboBoxItem;
            IngredientType = selectedType.Content as string;
            this.DialogResult = true;
            this.Close();
        }
    }
}
