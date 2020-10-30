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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FoodRecipeApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Button clickedControlButton;
		private bool checkFavoriteIsClicked, isMinimizeMenu;
		private BindingList<FoodInfomation> _list = new BindingList<FoodInfomation>();

		class FoodInfomation
		{
			public string Name { get; set; }
			public string Image { get; set; }
			public bool Favorite { get; set; }
		}

		class FoodImageDAO
		{
			public static BindingList<FoodInfomation> GetAll()
			{
				var result = new BindingList<FoodInfomation>()
				{
					new FoodInfomation() { Name="Chu Tùng Nhân", Image="Images/playerImage01.jpg", Favorite=false },
					new FoodInfomation() { Name="Nguyen Ánh Du", Image="Images/playerImage02.jpg", Favorite=false },
					new FoodInfomation() { Name="Lều Bách Khánh", Image="Images/playerImage03.jpg", Favorite=false },
					new FoodInfomation() { Name="Thiều Duy Hành", Image="Images/playerImage04.jpg", Favorite=false },
					new FoodInfomation() { Name="Nhiệm Băng Đoan", Image="Images/playerImage05.jpg", Favorite=false },
					new FoodInfomation() { Name="Mang Đình Từ", Image="Images/playerImage06.jpg", Favorite=false },
					new FoodInfomation() { Name="Bùi Tuyền", Image="Images/playerImage07.jpg", Favorite=false },
					new FoodInfomation() { Name="Triệu Triều Hải", Image="Images/playerImage08.jpg", Favorite=false },
					new FoodInfomation() { Name="Tạ Đoan Huệ", Image="Images/playerImage09.jpg", Favorite=false },
					new FoodInfomation() { Name="Đào Sương Thư", Image="Images/playerImage10.jpg", Favorite=false }
				};

				return result;
			}

			public static BindingList<FoodInfomation> GetHalf()
			{
				var result = new BindingList<FoodInfomation>()
				{
					new FoodInfomation() { Name="Chu Tùng Nhân", Image="Images/playerImage01.jpg", Favorite=false },
					new FoodInfomation() { Name="Nguyen Ánh Du", Image="Images/playerImage02.jpg", Favorite=false },
					new FoodInfomation() { Name="Lều Bách Khánh", Image="Images/playerImage03.jpg", Favorite=false },
					new FoodInfomation() { Name="Thiều Duy Hành", Image="Images/playerImage04.jpg", Favorite=false },
					new FoodInfomation() { Name="Nhiệm Băng Đoan", Image="Images/playerImage05.jpg", Favorite=false }
				};

				return result;
			}

			public static BindingList<FoodInfomation> GetLast()
			{
				var result = new BindingList<FoodInfomation>()
				{
					new FoodInfomation() { Name="Mang Đình Từ", Image="Images/playerImage06.jpg", Favorite=false },
					new FoodInfomation() { Name="Bùi Tuyền", Image="Images/playerImage07.jpg", Favorite=false },
					new FoodInfomation() { Name="Triệu Triều Hải", Image="Images/playerImage08.jpg", Favorite=false },
					new FoodInfomation() { Name="Tạ Đoan Huệ", Image="Images/playerImage09.jpg", Favorite=false },
					new FoodInfomation() { Name="Đào Sương Thư", Image="Images/playerImage10.jpg", Favorite=false }
				};

				return result;
			}
		}

		//Cài đặt nút đóng cửa sổ
		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		//Cài đặt nút phóng to/ thu nhỏ cửa sổ
		private void MaximizeButton_Click(object sender, RoutedEventArgs e)
		{
			AdjustWindowSize();
		}

		//Cài đặt nút ẩn cửa sổ
		private void MinimizeButton_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}

		//Thay đổi kích thước cửa sổ
		//Nếu đang ở trạng thái phóng to thì thu nhỏ và ngược lại
		private void AdjustWindowSize()
		{
			var imgName = "";

			if (this.WindowState == WindowState.Maximized)
			{
				this.WindowState = WindowState.Normal;
				imgName = "Images/maximize.png";
			}
			else
			{
				this.WindowState = WindowState.Maximized;
				imgName = "Images/restoreDown.png";
			}

			//Lấy nguồn ảnh
			Image img = new Image
			{
				Source = new BitmapImage(new Uri(
						imgName,
						UriKind.Relative)
				)
			};

			//Thiết lập ảnh chất lượng cao
			RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);

			//Thay đổi icon
			MaxButton.Content = img;
		}

		//Cài đặt để có thể di chuyển cửa sổ khi nhấn giữ chuột và kéo Title Bar
		private void DockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var move = sender as System.Windows.Controls.DockPanel;
			var win = Window.GetWindow(move);
			win.DragMove();
		}

		public MainWindow()
		{
			InitializeComponent();


			//imageFoodArray = new Rectangle[] {
			//	Image_0, Image_1, Image_2, Image_3,
			//	Image_4, Image_5, Image_6, Image_7,
			//	Image_8, Image_9, Image_10, Image_11,
			//	Image_12, Image_13, Image_14};

			//foodNameArray = new TextBlock[] {
			//	FoodName_0, FoodName_1, FoodName_2, FoodName_3,
			//	FoodName_4, FoodName_5, FoodName_6, FoodName_7,
			//	FoodName_8, FoodName_9, FoodName_10, FoodName_11,
			//	FoodName_12, FoodName_13, FoodName_14};

			checkFavoriteIsClicked = false;
			isMinimizeMenu = false;

			//Default buttons
			//clickedTypeButton = AllButton;
			//clickedTypeButton.Background = Brushes.LightSkyBlue;
			clickedControlButton = HomeButton;
			//clickedControlButton.Background = Brushes.LightSkyBlue;
		}

		private void changeClickedTypeButton(object sender, RoutedEventArgs e)
		{
			//clickedTypeButton.Background = Brushes.DarkSlateGray;
			//clickedTypeButton = button;
			//button.Background = Brushes.LightSkyBlue;

			var button = (Button)sender;

			//Hiển thị các món ăn thuộc loại thức ăn được chọn
			if (button == AllButton)
			{
				_list = FoodImageDAO.GetAll();
				foodButtonItemsControl.ItemsSource = _list;
			}
			else if (button == FoodButton)
			{
				_list = FoodImageDAO.GetHalf();
				foodButtonItemsControl.ItemsSource = _list;
			}
			else if (button == DrinksButton)
			{
				_list = FoodImageDAO.GetLast();
				foodButtonItemsControl.ItemsSource = _list;
			}
			else
			{
				//Do nothing
			}
		}

		private void changeClickedControlButton(object sender, RoutedEventArgs e)
		{
			//clickedControlButton.Background = Brushes.SlateGray;
			//clickedControlButton = button;
			//button.Background = Brushes.LightSkyBlue;

			var button = (Button)sender;
			
			if (button != clickedControlButton)
			{
				//Đóng giao diện cũ trước khi nhấn nút
				if (clickedControlButton == HomeButton)
				{
					TypeBar.Visibility = Visibility.Collapsed;
					foodButtonItemsControl.Visibility = Visibility.Collapsed;
				}
				else if (clickedControlButton == FavoriteButton)
				{

				}
				else if (clickedControlButton == AddDishButton)
				{

				}
				else if (clickedControlButton == IngredientButton)
				{

				}
				else
				{
					//Do nothing
				}

				//Mở giao diện mới sau khi nhấn nút
				if (button == HomeButton)
				{
					TypeBar.Visibility = Visibility.Visible;
					foodButtonItemsControl.Visibility = Visibility.Visible;
				}
				else if (button == FavoriteButton)
				{

				}
				else if (button == AddDishButton)
				{

				}
				else if (button == IngredientButton)
				{

				}
				else
				{
					//Do nothing
				}

				//Cập nhật lại nút được chọn
				clickedControlButton = button;
			}
			else
			{
				//Do nothing
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_list = FoodImageDAO.GetAll();
			foodButtonItemsControl.ItemsSource = _list;
		}

		private void Menu_Click(object sender, RoutedEventArgs e)
		{
			if (isMinimizeMenu == false)
			{
				ControlStackPanel.Width = 47;
				//FoodUniformGrid.Columns = 5;
				//for (int i = 0; i < imageFoodArray.Length - 3; i++)
				//{
				//	imageFoodArray[i].Width = 190;
				//	foodNameArray[i].Width = 165;
				//}
				//Food_12.Visibility = Visibility.Visible;
				//Food_13.Visibility = Visibility.Visible;
				//Food_14.Visibility = Visibility.Visible;
				//Image_0_0.Fill = new ImageBrush(new BitmapImage(
				//	new Uri("Images/playerImage04.jpg", UriKind.Relative)));
				isMinimizeMenu = true;
			}
			else
			{
				ControlStackPanel.Width = 250;
				//FoodUniformGrid.Columns = 4;
				//for (int i = 0; i < imageFoodArray.Length - 3; i++)
				//{
				//	imageFoodArray[i].Width = 202;
				//	foodNameArray[i].Width = 177;
				//}
				//Food_12.Visibility = Visibility.Collapsed;
				//Food_13.Visibility = Visibility.Collapsed;
				//Food_14.Visibility = Visibility.Collapsed;
				isMinimizeMenu = false;
			}
		}

		private int GetElementIndexInArray(Button button)
		{
			var wrapPanel = (WrapPanel)button.Content;
			var collection = wrapPanel.Children;
			var rectangle = (Rectangle)collection[0];
			var imageBrush = (ImageBrush)rectangle.Fill;
			var imageSource = imageBrush.ImageSource;
			var imageSourceString = imageSource.ToString();
			var imageName = imageSourceString.Substring(23);
			var result = 0;
			for (int i = 0; i < _list.Count; i++)
			{
				if (imageName == _list[i].Image)
				{
					result = i;
					break;
				}
				else
				{
					//Do nothing
				}
			}
			return result;
		}
		private void ChangeFavoriteState(Button button, Image image)
		{
			var wrapPanel = (WrapPanel)button.Content;
			var collection = wrapPanel.Children;
			var favoriteButton = (Button)collection[2];
			favoriteButton.Content = image;
		}

		private void BackButton_Click(object sender, RoutedEventArgs e)
		{
			detailFoodGrid.Visibility = Visibility.Collapsed;
			TypeBar.Visibility = Visibility.Visible;
			foodButtonItemsControl.Visibility = Visibility.Visible;
			BackButton.Visibility = Visibility.Collapsed;
		}

		private void FoodImage_Click(object sender, RoutedEventArgs e)
		{
			if (checkFavoriteIsClicked == false)
			{
				foodButtonItemsControl.Visibility = Visibility.Collapsed;
				TypeBar.Visibility = Visibility.Collapsed;
				detailFoodGrid.Visibility = Visibility.Visible;
			
				var index = GetElementIndexInArray((Button)sender);
				var bitmap = new BitmapImage(
					new Uri(
						_list[index].Image,
						UriKind.Relative)
				);

				//Thiết lập ảnh chất lượng cao
				RenderOptions.SetBitmapScalingMode(bitmap, BitmapScalingMode.HighQuality);

				//Hiển thị ảnh chính của món ăn
				SelectedFoodImage.Source = bitmap;

				//Hiển thị tên món ăn
				SelectedFoodName.Text = _list[index].Name;

				//Hiển thị các ảnh khác của món ăn
				foodImageListView.ItemsSource = _list;

				//Hiển thị nút quay lại
				BackButton.Visibility = Visibility.Visible;
			}
			else
			{
				string imgName;
				int index = GetElementIndexInArray((Button)sender);

				//Nếu chưa yêu thích thì chuyển sang ảnh yêu thích và thêm vào danh sách yêu thích
				if (_list[index].Favorite == true)
				{
					_list[index].Favorite = false;
					imgName = "Images/unloved.png";
				}
				else //Nếu yêu thích rồi chuyển sang ảnh chưa yêu thích và xóa khỏi danh sách yêu thích
				{
					_list[index].Favorite = true;
					imgName = "Images/favorite.png";
				}

				//Lấy nguồn ảnh
				Image img = new Image
				{
					Source = new BitmapImage(new Uri(
							imgName,
							UriKind.Relative)
					)
				};

				//Thiết lập ảnh chất lượng cao
				RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);

				//Thay đổi ảnh
				ChangeFavoriteState((Button)sender, img);
				checkFavoriteIsClicked = false;
			}
		}

		private void Favorite_Click(object sender, RoutedEventArgs e)
		{
			checkFavoriteIsClicked = true;
		}
	}
}
