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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FoodRecipeApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Button clickedTypeButton, clickedControlButton;
		private bool isFavorite, checkFavoriteIsClicked, isMinimizeMenu;
		private Rectangle[] imageFoodArray;
		private TextBlock[] foodNameArray;

		public MainWindow()
		{
			InitializeComponent();


			imageFoodArray = new Rectangle[] {
				Image_0, Image_1, Image_2, Image_3,
				Image_4, Image_5, Image_6, Image_7,
				Image_8, Image_9, Image_10, Image_11,
				Image_12, Image_13, Image_14};

			foodNameArray = new TextBlock[] {
				FoodName_0, FoodName_1, FoodName_2, FoodName_3,
				FoodName_4, FoodName_5, FoodName_6, FoodName_7,
				FoodName_8, FoodName_9, FoodName_10, FoodName_11,
				FoodName_12, FoodName_13, FoodName_14};

			isFavorite = false;
			checkFavoriteIsClicked = false;
			isMinimizeMenu = false;

			//Default buttons
			clickedTypeButton = AllButton;
			clickedTypeButton.Background = Brushes.LightSkyBlue;
			clickedControlButton = HomeButton;
			clickedControlButton.Background = Brushes.LightSkyBlue;
		}

		private void changeClickedTypeButton(Button button)
		{
			//clickedTypeButton.Background = Brushes.LightGray;
			//clickedTypeButton = button;
			//button.Background = Brushes.LightSkyBlue;
		}

		private void changeClickedControlButton(Button button)
		{
			//clickedControlButton.Background = Brushes.LightGray;
			//clickedControlButton = button;
			//button.Background = Brushes.LightSkyBlue;
		}

		private void HomeButton_Click(object sender, RoutedEventArgs e)
		{
			changeClickedControlButton(HomeButton);
		}

		private void FavoriteButton_Click(object sender, RoutedEventArgs e)
		{
			changeClickedControlButton(FavoriteButton);
		}

		private void AddDishButton_Click(object sender, RoutedEventArgs e)
		{
			changeClickedControlButton(AddDishButton);
		}

		private void IngredientButton_Click(object sender, RoutedEventArgs e)
		{
			changeClickedControlButton(IngredientButton);
		}

		private void AllButton_Click(object sender, RoutedEventArgs e)
		{
			changeClickedTypeButton(AllButton);
		}

		private void FoodButton_Click(object sender, RoutedEventArgs e)
		{
			changeClickedTypeButton(FoodButton);
		}
		private void DrinksButton_Click(object sender, RoutedEventArgs e)
		{
			changeClickedTypeButton(DrinksButton);
		}

		private void Menu_Click(object sender, RoutedEventArgs e)
		{
			if (isMinimizeMenu == false)
			{
				ControlStackPanel.Width = 47;
				FoodUniformGrid.Columns = 5;
				for (int i = 0; i < imageFoodArray.Length - 3; i++)
				{
					imageFoodArray[i].Width = 190;
					foodNameArray[i].Width = 165;
				}
				Food_12.Visibility = Visibility.Visible;
				Food_13.Visibility = Visibility.Visible;
				Food_14.Visibility = Visibility.Visible;
				//Image_0_0.Fill = new ImageBrush(new BitmapImage(
				//	new Uri("Images/playerImage04.jpg", UriKind.Relative)));
				isMinimizeMenu = true;
			}
			else
			{
				ControlStackPanel.Width = 200;
				FoodUniformGrid.Columns = 4;
				for (int i = 0; i < imageFoodArray.Length - 3; i++)
				{
					imageFoodArray[i].Width = 202;
					foodNameArray[i].Width = 177;
				}
				Food_12.Visibility = Visibility.Collapsed;
				Food_13.Visibility = Visibility.Collapsed;
				Food_14.Visibility = Visibility.Collapsed;
				isMinimizeMenu = false;
			}
		}

		private void FoodImage_Click(object sender, RoutedEventArgs e)
		{
			if (checkFavoriteIsClicked == false)
			{
				//var bitmap = new BitmapImage(
				//	new Uri(
				//		"Images/playerImage04.jpg",
				//		UriKind.Relative)
				//);
				//Button button = (Button)sender;
				//string btnName = button.Name;
				//int index = (btnName[btnName.Length - 3] - '0') * 4 + btnName[btnName.Length - 1] - '0';
				//imageFoodArray[index].ImageSource = bitmap;
			}
			else
			{
				checkFavoriteIsClicked = false;
			}
		}

		private void Favorite_Click(object sender, RoutedEventArgs e)
		{
			checkFavoriteIsClicked = true;
			string imgName;
			//Nếu chưa yêu thích thì chuyển sang ảnh yêu thích và thêm vào danh sách yêu thích
			if (isFavorite == true)
			{
				imgName = "Images/unloved.png";
				isFavorite = false;
			}
			else //Nếu yêu thích rồi chuyển sang ảnh chưa yêu thích và xóa khỏi danh sách yêu thích
			{
				imgName = "Images/favorite.png";
				isFavorite = true;
			}

			//Lấy nguồn ảnh
			Image img = new Image
			{
				Source = new BitmapImage(new Uri(
						imgName,
						UriKind.Relative)
				)
			};

			RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
			//Thay đổi ảnh
			Button button = (Button)sender;
			button.Content = img;
		}
	}
}
