using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace FoodRecipeApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{

		//---------------------------------------- Khai báo các biến toàn cục --------------------------------------------//

		public event PropertyChangedEventHandler PropertyChanged;
		private Button clickedControlButton;
		private List<FoodInfomation> ListFoodInfo = new List<FoodInfomation>();	//Danh sách thông tin tất cả các món ăn
		BindingList<Step> ListStep = new BindingList<Step>();					//Danh sách các bước của món ăn mới được thêm
		FoodInfomation newFood;													//Món ăn mới được thêm
		private CollectionView view;
		BindingList<Dish> ListDish = new BindingList<Dish>();
		private List<FoodInfomation> FoodOnScreen;                              //Danh sách food để hiện trên màn hình
		private Condition FilterCondition = new Condition { Favorite = false, Type = "" };
		private Regex YouTubeURLIDRegex = new Regex(@"[\?&]v=(?<v>[^&]+)");

		private bool checkFavoriteIsClicked, isMinimizeMenu;
		private int FoodperPage = 12;			//Số món ăn mỗi trang
		private int _totalPage = 0;				//Tổng số trang
		public int TotalPage
		{
			get
			{
				return _totalPage;
			}
			set
			{
				_totalPage = value;
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("TotalPage"));
				}
			}
		}
		private int _currentPage = 1;			//Trang hiện tại
		public int CurrentPage
		{
			get
			{
				return _currentPage;
			}
			set
			{
				_currentPage = value;
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("CurrentPage"));
				}
			}
		}
		private int _totalStep = 0;             //Tổng số bước
		public int TotalStep
		{
			get
			{
				return _totalStep;
			}
			set
			{
				_totalStep = value;
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("TotalStep"));
				}
			}
		}
		private int _currentStep = 0;           //Bước hiện tại
		public int CurrentStep
		{
			get
			{
				return _currentStep;
			}
			set
			{
				_currentStep = value;
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("CurrentStep"));
				}
			}
		}
		private string _totalItem = "0 item";   //Tổng số món ăn theo filter hiện tại
		public string TotalItem
		{
			get
			{
				return _totalItem;
			}
			set
			{
				_totalItem = value;
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("TotalItem"));
				}
			}
		}

		private string _colorScheme = "";           //Trang hiện tại
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

		private int CurrentElementIndex = 0;



		//---------------------------------------- Khai báo các class --------------------------------------------//

		//Class lưu trữ đường dẫn của từng ảnh của 1 bước khi thực hiện nấu món ăn
		public class ImagePerStep
		{
			public string ImagePath { get; set; }
		}

		//Class điều kiện để filter
		class Condition
		{
			public bool Favorite;
			public string Type;
		}

		//Class nguyên liệu cần chuẩn bị cho một món ăn
		public class Ingredient
		{
			public string IngredientName { get; set; }
			public bool IsDone { get; set; }
		}

		//Class các món ăn cần mua nguyên liệu
		public class Dish
		{
			public string DishName { get; set; }                        //Tên danh sách nguyên liệu cần mua
			public BindingList<Ingredient> GroceriesList { get; set; }  //Tên các nguyên liệu trong danh sách trên
		}

		//Class các bước thực hiện trong một món ăn
		public class Step : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			public int Order { get; set; }							//Thứ tự bước
			public string Content { get; set; }						//Mô tả bước

			private BindingList<ImagePerStep> _imagesPathPerStep;	//Đường dẫn các ảnh của bước
			public BindingList<ImagePerStep> ImagesPathPerStep
			{
				get
				{
					return _imagesPathPerStep;
				}
				set
				{
					_imagesPathPerStep = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs("ImagesPathPerStep"));
					}
				}
			}
		}

		//Class thông tin món ăn
		public partial class FoodInfomation : INotifyPropertyChanged
		{
			public int ID { get; set; }             //ID món ăn 
			public string Name { get; set; }        //Tên món ăn
			public string Ingredients { get; set; }	//Danh sách nguyên liệu
			public bool IsFavorite { get; set; }    //Món yêu thích
			public string DateAdd { get; set; }     //Ngày thêm
			public string VideoLink { get; set; }   //Link youtube
			public string Level { get; set; }       //Mức độ khó
			public string Type { get; set; }        //Loại đồ ăn
			public string Discription { get; set; } //Mô tả khái quát món ăn

			private string _primaryImagePath;       //Đường dẫn ảnh chính
			public string PrimaryImagePath
			{
				get
				{
					return _primaryImagePath;
				}
				set
				{
					_primaryImagePath = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs("PrimaryImagePath"));
					}
				}
			}
			public BindingList<Step> Steps { get; set; }	//Các bước thực hiện của món ăn

			public event PropertyChangedEventHandler PropertyChanged;

		}

		public MainWindow()
		{
			InitializeComponent();

			checkFavoriteIsClicked = false;
			isMinimizeMenu = false;
			//Default buttons
			//clickedTypeButton = AllButton;
			//clickedTypeButton.Background = Brushes.LightSkyBlue;
			clickedControlButton = HomeButton;
			//clickedControlButton.Background = Brushes.LightSkyBlue;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			ColorScheme = "ForestGreen";

			//Đọc dữ liệu các món ăn từ data
			XmlSerializer xsFood = new XmlSerializer(typeof(List<FoodInfomation>));
			using (var reader = new StreamReader(@"data\Food.xml"))
			{
				ListFoodInfo = (List<FoodInfomation>)xsFood.Deserialize(reader);
			}
			FoodOnScreen = ListFoodInfo;

			XmlSerializer xsDish = new XmlSerializer(typeof(BindingList<Dish>));
			using (var reader = new StreamReader(@"data\Dish.xml"))
			{
				ListDish = (BindingList<Dish>)xsDish.Deserialize(reader);
			}

			//Khởi tạo phân trang
			TotalPage = (ListFoodInfo.Count - 1) / FoodperPage + 1;
			TotalItem = ListFoodInfo.Count.ToString();
			if (FoodOnScreen.Count > 1)
			{
				TotalItem += " items";
			}
			else
			{
				TotalItem += " item";
			}
			UpdatePageButtonStatus();

			//Binding Số trang
			SearchComboBox.ItemsSource = ListFoodInfo;
			this.DataContext = this;

			/*Lấy danh sách food*/
			var foods = FoodOnScreen.Take(FoodperPage);
			foodButtonItemsControl.ItemsSource = foods;
			view = (CollectionView)CollectionViewSource.GetDefaultView(ListFoodInfo);
			DishListItemsControl.ItemsSource = ListDish;
		}



		//---------------------------------------- Xử lý cửa sổ --------------------------------------------//

		//Cài đặt nút đóng cửa sổ
		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			SaveListFood();
			SaveListDish();
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


		//---------------------------------------- Xử lý các nút bấm --------------------------------------------//

		private void changeClickedTypeButton_Click(object sender, RoutedEventArgs e)
		{
			//clickedTypeButton.Background = Brushes.DarkSlateGray;
			//clickedTypeButton = button;
			//button.Background = Brushes.LightSkyBlue;

			var button = (Button)sender;

			//Hiển thị các món ăn thuộc loại thức ăn được chọn
			if (button == AllButton)
			{
				FilterCondition.Type = "";
			}
			else if (button == FoodButton)
			{
				FilterCondition.Type = "Food";
			}
			else if (button == DrinksButton)
			{
				FilterCondition.Type = "Drinks";
			}
			else
			{
				//Do nothing
			}

			//Cập nhật lại giao diện
			UpdateUIFromData();
		}

		private void changeClickedControlButton_Click(object sender, RoutedEventArgs e)
		{
			//clickedControlButton.Background = Brushes.SlateGray;
			//clickedControlButton = button;
			//button.Background = Brushes.LightSkyBlue;

			var button = (Button)sender;

			if (button != clickedControlButton)
			{
				//Đóng giao diện cũ trước khi nhấn nút
				if (clickedControlButton == HomeButton || clickedControlButton == FavoriteButton)
				{
					TypeBar.Visibility = Visibility.Collapsed;
					foodButtonItemsControl.Visibility = Visibility.Collapsed;
					PaginationBar.Visibility = Visibility.Collapsed;
				}
				else if (clickedControlButton == AddDishButton)
				{
					AddFood.Visibility = Visibility.Collapsed;
					AddFood.DataContext = null;
					AddFoodScrollViewer.ScrollToHome();
				}
				else if (clickedControlButton == DishButton)
				{
					DishList.Visibility = Visibility.Collapsed;
				}
				else
				{
					//Do nothing
				}

				//Xóa màu của thanh đang được chọn
				var wrapPanel = (WrapPanel)clickedControlButton.Content;
				var collection = wrapPanel.Children;
				var block = (TextBlock)collection[0];
				var text = (TextBlock)collection[2];
				block.Background = Brushes.Transparent;
				text.Foreground = Brushes.Black;

				//Mở giao diện mới sau khi nhấn nút
				if (button == HomeButton)
				{
					FilterCondition.Favorite = false;
					TypeBar.Visibility = Visibility.Visible;
					foodButtonItemsControl.Visibility = Visibility.Visible;
					PaginationBar.Visibility = Visibility.Visible;
				}
				else if (button == FavoriteButton)
				{
					FilterCondition.Favorite = true;
					TypeBar.Visibility = Visibility.Visible;
					foodButtonItemsControl.Visibility = Visibility.Visible;
					PaginationBar.Visibility = Visibility.Visible;
				}
				else if (button == AddDishButton)
				{
					SortFoodList();
					var index = GetMinID();
					newFood = new FoodInfomation() { ID = index, VideoLink = "" };
					AddFood.Visibility = Visibility.Visible;
					AddFood.DataContext = newFood;
					ListStep = new BindingList<Step>();
					ImageStepItemsControl.ItemsSource = ListStep;

					//Thay đổi màu chữ cho các tiêu đề trong món ăn
					AddFood_TitleTextBlock.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(ColorScheme);
					AddFood_LinkVideoTextBlock.Foreground = AddFood_TitleTextBlock.Foreground;
					AddFood_LevelTextBlock.Foreground = AddFood_TitleTextBlock.Foreground;
					AddFood_TypeTextBlock.Foreground = AddFood_TitleTextBlock.Foreground;
					AddFood_PhotosTextBlock.Foreground = AddFood_TitleTextBlock.Foreground;
					AddFood_DescriptionTextBlock.Foreground = AddFood_TitleTextBlock.Foreground;
					AddFood_IngredientsTextBlock.Foreground = AddFood_TitleTextBlock.Foreground;
					AddFood_DirectionsTextBlock.Foreground = AddFood_TitleTextBlock.Foreground;
				}
				else if (button == DishButton)
				{
					DishList.Visibility = Visibility.Visible;
				}
				else
				{
					//Do nothing
				}

				//Cập nhật lại nút được chọn
				clickedControlButton = button;

				//Hiển thị màu của thanh mới vừa được chọn
				wrapPanel = (WrapPanel)clickedControlButton.Content;
				collection = wrapPanel.Children;
				block = (TextBlock)collection[0];
				text = (TextBlock)collection[2];
				block.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(ColorScheme);
				text.Foreground = block.Background;

				//Cập nhật lại giao diện
				UpdateUIFromData();
			}
			else
			{
				//Do nothing
			}
		}

		private void MenuButton_Click(object sender, RoutedEventArgs e)
		{
			if (isMinimizeMenu == false)
			{
				ControlStackPanel.Width = 47;
				FoodperPage = 15;
				UpdateFoodStatus();
				isMinimizeMenu = true;
			}
			else
			{
				ControlStackPanel.Width = 250;
				FoodperPage = 12;
				UpdateFoodStatus();
				isMinimizeMenu = false;
			}
		}

		private void BackButton_Click(object sender, RoutedEventArgs e)
		{
			BackButton.Visibility = Visibility.Collapsed;
			FoodDetailScrollViewer.Visibility = Visibility.Collapsed;
			TypeBar.Visibility = Visibility.Visible;
			foodButtonItemsControl.Visibility = Visibility.Visible;
			PaginationBar.Visibility = Visibility.Visible;
		}

		private void FoodImage_Click(object sender, RoutedEventArgs e)
		{
			if (checkFavoriteIsClicked == false)
			{
				//Đóng màn hình trước
				foodButtonItemsControl.Visibility = Visibility.Collapsed;
				TypeBar.Visibility = Visibility.Collapsed;
				PaginationBar.Visibility = Visibility.Collapsed;

				//Lấy chỉ số của hình ảnh món ăn được nhấn
				CurrentElementIndex = GetElementIndexInArray((Button)sender);

				//Binding dữ liệu để hiển thị chi tiết món ăn
				FoodDetailGrid.DataContext = ListFoodInfo[CurrentElementIndex];

				//Thay đổi màu chữ cho tiêu đề thông tin chi tiết món ăn
				FoodInfo_NameTextBlock.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(ColorScheme);
				FoodInfo_IngredientsTextBlock.Foreground = FoodInfo_NameTextBlock.Foreground;
				FoodInfo_DirectionsTextBlock.Foreground = FoodInfo_NameTextBlock.Foreground;
				FoodInfo_VideoTextBlock.Foreground = FoodInfo_NameTextBlock.Foreground;

				//Binding dữ liệu các hình ảnh của bước hiện tại đang được hiển thị trên màn hình
				if (ListFoodInfo[CurrentElementIndex].Steps.Count > 0)
				{
					StepsGrid.DataContext = ListFoodInfo[CurrentElementIndex].Steps[0];
					ImagesPerStepItemsControl.ItemsSource = ListFoodInfo[CurrentElementIndex].Steps[0].ImagesPathPerStep;
					//Hiển thị các bước nếu số bước lớn hơn 0
					//Đếm số bước bắt đầu từ 1
					CurrentStep = 1;
					DirectionsGrid.Visibility = Visibility.Visible;
				}
				else
				{
					//Không hiển thị các bước nếu số bước bằng 0
					DirectionsGrid.Visibility = Visibility.Collapsed;
				}

				//Hiển thị thanh phân trang cho số bước
				TotalStep = ListFoodInfo[CurrentElementIndex].Steps.Count;

				//Chức năng lùi về bước trước bị vô hiệu hóa khi đang ở bước đầu tiên
				PreviousStepButton.IsEnabled = false;
				PreviousStepButton.Foreground = Brushes.Black;

				//Binding dữ liệu cho phân trang các bước của món ăn
				StepPaginationBar.DataContext = this;

				//Chức năng tiến lên bước sau bị vô hiệu hóa khi đang ở bước cuối cùng
				if (CurrentStep == TotalStep)
				{
					NextStepButton.IsEnabled = false;
					NextStepTextBlock.Foreground = Brushes.Black;
				}
				else
				{
					//Do nothing
				}

				//Hiển thị video mô tả món ăn
				Display(ListFoodInfo[CurrentElementIndex].VideoLink);

				//Hiển thị màn hình chi tiết món ăn
				FoodDetailScrollViewer.Visibility = Visibility.Visible;

				//Hiển thị nút quay lại
				BackButton.Visibility = Visibility.Visible;
			}
			else
			{
				int index = GetElementIndexInArray((Button)sender);

				//Nếu chưa yêu thích thì chuyển sang ảnh yêu thích và thêm vào danh sách yêu thích
				if (ListFoodInfo[index].IsFavorite == true)
				{
					ListFoodInfo[index].IsFavorite = false;
					//imgName = "Images/unloved.png";
				}
				else //Nếu yêu thích rồi chuyển sang ảnh chưa yêu thích và xóa khỏi danh sách yêu thích
				{
					ListFoodInfo[index].IsFavorite = true;
					//imgName = "Images/favorite.png";
				}

				////Lấy nguồn ảnh
				//Image img = new Image
				//{
				//	Source = new BitmapImage(new Uri(
				//			imgName,
				//			UriKind.Relative)
				//	)
				//};

				////Thiết lập ảnh chất lượng cao
				//RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);

				////Thay đổi ảnh
				//ChangeFavoriteState((Button)sender, img);
				checkFavoriteIsClicked = false;

				//Cập nhật lại giao diện
				UpdateFoodStatus();
			}
		}

		private void AddStepButton_Click(object sender, RoutedEventArgs e)
		{
			if (ListStep.Count == 0)
			{
				var newStep = new Step() { Order = 1, ImagesPathPerStep = new BindingList<ImagePerStep>() };
				ListStep.Add(newStep);
			}
			else
			{
				var newStep = new Step()
				{
					Order = ListStep[ListStep.Count - 1].Order + 1,
					ImagesPathPerStep = new BindingList<ImagePerStep>()
				};
				ListStep.Add(newStep);
			}
		}

		private void AddPrimaryFoodPhoto_Click(object sender, RoutedEventArgs e)
		{
			var fileDialog = new OpenFileDialog();
			fileDialog.Filter = "Image Files(*.JPG*)|*.JPG";
			fileDialog.Title = "Select Image";

			if (fileDialog.ShowDialog() == true)
			{
				string filePath = fileDialog.FileName;
				newFood.PrimaryImagePath = filePath;
			}
			else
			{
				//Do nothing
			}

		}

		private void AddStepFoodPhoto_Click(object sender, RoutedEventArgs e)
		{
			var fileDialog = new OpenFileDialog();
			fileDialog.Multiselect = true;
			fileDialog.Filter = "Image Files(*.JPG*)|*.JPG";
			fileDialog.Title = "Select Image";
			
			if (fileDialog.ShowDialog() == true)
			{
				var container = FindParent<StackPanel>(sender as DependencyObject);
				var grid = FindParent<Grid>(sender as DependencyObject);
				if (container != null)
				{
					var currData = container.DataContext as Step;
					var fileNames = fileDialog.FileNames;
					currData.ImagesPathPerStep.Clear();
					for (int i = 0; i < fileNames.Length; i++)
					{
						currData.ImagesPathPerStep.Add(new ImagePerStep() { ImagePath = fileNames[i] });
					}
					if (grid != null)
					{
						var subItemsControl = GetChildOfType<ItemsControl>(grid);
						subItemsControl.ItemsSource = currData.ImagesPathPerStep;
					}
				}
			}
		}

		private void SaveFood_Click(object sender, RoutedEventArgs e)
		{
			newFood.DateAdd = DateTime.Now.ToString();
			var comboBoxItem = (ComboBoxItem)LevelComboBox.SelectedItem;
			newFood.Level = (string)comboBoxItem.Content;
			comboBoxItem = (ComboBoxItem)TypeComboBox.SelectedItem;
			newFood.Type = (string)comboBoxItem.Content;
			string newFolder = GetAppDomain();
			string foodPicName = $"{newFood.ID}.jpg";
			string newPath = $"{newFolder}\\images\\{foodPicName}";

			if (newFood.PrimaryImagePath != null)
			{
				File.Copy(newFood.PrimaryImagePath, newPath);
				newFood.PrimaryImagePath = $"images\\{foodPicName}";
			}
			else
			{
				//Do nothing
			}

			SaveStepsImages();

			//SaveStepData();

			newFood.Steps = ListStep;

			ListFoodInfo.Add(newFood);
			//_foodStepsList.Add(ListStep);
			//foodButtonItemsControl.ItemsSource = ListFoodInfo;
			//view = (CollectionView)CollectionViewSource.GetDefaultView(ListFoodInfo);
			UpdateUIFromData();
		}

		private void Favorite_Click(object sender, RoutedEventArgs e)
		{
			checkFavoriteIsClicked = true;
		}

		private void NextPageButton_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentPage < TotalPage)
			{
				var foods = FoodOnScreen.Skip(CurrentPage * FoodperPage).Take(FoodperPage);
				CurrentPage++;
				foodButtonItemsControl.ItemsSource = foods;
			}
			UpdatePageButtonStatus();
		}

		/*Về trang trước*/
		private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentPage > 1)
			{
				CurrentPage--;
				var foods = FoodOnScreen.Skip((CurrentPage - 1) * FoodperPage).Take(FoodperPage);
				foodButtonItemsControl.ItemsSource = foods;
			}
			UpdatePageButtonStatus();
		}

		private void EditDishInListButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void DishListName_Click(object sender, RoutedEventArgs e)
		{
			var button = (Button)sender;
			var content = (string)button.Content;
			for (int i = 0; i < ListDish.Count; i++)
			{
				if (ListDish[i].DishName == content)
				{
					DishInListItemsControl.ItemsSource = ListDish[i].GroceriesList;
					DishListNameTextBlock.DataContext = ListDish[i];
					break;
				}
			}
		}

		//Xử lý khi nhấn nút thêm 1 List nguyên liệu mới
		private void AddDishList_Click(object sender, RoutedEventArgs e)
		{
			var text = DishListTextBox.Text;
			if (text != "")
			{
				var isExist = false;
				for (int i = 0; i < ListDish.Count; i++)
				{
					if (text == ListDish[i].DishName)
					{
						isExist = true;
						break;
					}
				}

				//Tên đã tồn tại, người dùng phải nhập lại
				if (isExist == true)
				{
					MessageBox.Show(
					"List name already exists, please enter another name!",
					"Announcement",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
				}
				else
				{
					ListDish.Add(new Dish { DishName = text, GroceriesList = new BindingList<Ingredient>() });
				}
			}
			else
			{
				//Do nothing
			}
		}

		//Xử lý khi nhấn nút thêm 1 nguyên liệu mới cho List đang được chọn
		private void AddDishInListItem_Click(object sender, RoutedEventArgs e)
		{
			var text = DishInListTextBox.Text;
			if (text != "")
			{
				var DishListName = DishListNameTextBlock.Text;
				if (DishListName == "")
				{
					MessageBox.Show(
					"Please select a list to add Dishs!",
					"Announcement",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
				}
				else
				{
					for (int i = 0; i < ListDish.Count; i++)
					{
						if (ListDish[i].DishName == DishListName)
						{
							ListDish[i].GroceriesList.Add(new Ingredient { IngredientName = text, IsDone = false });
							break;
						}
					}
				}
			}
			else
			{
				//Do nothing
			}
		}

		private void CancelFood_Click(object sender, RoutedEventArgs e)
		{

		}

		private void FirstPageButton_Click(object sender, RoutedEventArgs e)
		{
			CurrentPage = 1;
			var foods = FoodOnScreen.Skip((CurrentPage - 1) * FoodperPage).Take(FoodperPage);
			foodButtonItemsControl.ItemsSource = foods;
			UpdatePageButtonStatus();
		}

		private void LastPageButton_Click(object sender, RoutedEventArgs e)
		{
			CurrentPage = TotalPage;
			var foods = FoodOnScreen.Skip((CurrentPage - 1) * FoodperPage).Take(FoodperPage);
			foodButtonItemsControl.ItemsSource = foods;
			UpdatePageButtonStatus();
		}

		private void PreviousStepButton_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentStep == 2)
			{
				PreviousStepButton.IsEnabled = false;
				PreviousStepTextBlock.Foreground = Brushes.Black;
			}
			else if (NextStepButton.IsEnabled == false)
			{
				NextStepButton.IsEnabled = true;
				NextStepTextBlock.Foreground = Brushes.White;
			}

			//Lùi về bước trước
			CurrentStep--;

			//Binging dữ liệu
			StepsGrid.DataContext = ListFoodInfo[CurrentElementIndex].Steps[CurrentStep - 1];
			ImagesPerStepItemsControl.ItemsSource = ListFoodInfo[CurrentElementIndex].Steps[CurrentStep - 1].ImagesPathPerStep;
		}

		private void NextStepButton_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentStep == TotalStep - 1)
			{
				NextStepButton.IsEnabled = false;
				NextStepTextBlock.Foreground = Brushes.Black;
			}
			else if (PreviousStepButton.IsEnabled == false)
			{
				PreviousStepButton.IsEnabled = true;
				PreviousStepTextBlock.Foreground = Brushes.White;
			}
			//Tiến tới bước tiếp theo
			CurrentStep++;

			//Binging dữ liệu
			StepsGrid.DataContext = ListFoodInfo[CurrentElementIndex].Steps[CurrentStep - 1];
			ImagesPerStepItemsControl.ItemsSource = ListFoodInfo[CurrentElementIndex].Steps[CurrentStep - 1].ImagesPathPerStep;
		}



		//---------------------------------------- Các hàm sắp xếp --------------------------------------------//

		private void SortFoodList()
		{
			FoodInfomation temp;
			for (int i = 0; i < ListFoodInfo.Count - 1; i++)
			{
				for (int j = i + 1; j < ListFoodInfo.Count; j++)
				{
					if (ListFoodInfo[i].ID > ListFoodInfo[j].ID)
					{
						temp = ListFoodInfo[i];
						ListFoodInfo[i] = ListFoodInfo[j];
						ListFoodInfo[j] = temp;
					}
				}
			}
		}

		private bool Filter(object item)
		{
			bool result;
			var foodInfo = (FoodInfomation)item;
			if (FilterCondition.Favorite == true && foodInfo.IsFavorite == false)
			{
				result = false;
			}
			else if (FilterCondition.Type != "" && FilterCondition.Type != foodInfo.Type)
			{
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}




		//---------------------------------------- Các hàm lấy phần tử cha và phần tử con --------------------------------------------//

		private static T FindParent<T>(DependencyObject dependencyObject) where T : DependencyObject
		{
			var parent = VisualTreeHelper.GetParent(dependencyObject);

			if (parent == null) return null;

			var parentT = parent as T;
			return parentT ?? FindParent<T>(parent);
		}

		public static T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
		{
			if (depObj == null) return null;

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
			{
				var child = VisualTreeHelper.GetChild(depObj, i);

				var result = (child as T) ?? GetChildOfType<T>(child);
				if (result != null) return result;
			}
			return null;
		}



		//---------------------------------------- Các hàm Get --------------------------------------------//

		private int GetMinID()
		{
			int result = 1;
			for (int i = 0; i < ListFoodInfo.Count; i++)
			{
				if (result < ListFoodInfo[i].ID)
				{
					break;
				}
				else
				{
					result++;
				}
			}
			return result;
		}

		private int GetElementIndexInArray(Button button)
		{
			var wrapPanel = (WrapPanel)button.Content;
			var curFood = wrapPanel.DataContext as FoodInfomation;
			var result = 0;
			for (int i = 0; i < ListFoodInfo.Count; i++)
			{
				if (curFood == ListFoodInfo[i])
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

		/* Get current app domain*/
		public static string GetAppDomain()
		{
			string absolutePath;
			absolutePath = AppDomain.CurrentDomain.BaseDirectory;
			return absolutePath;
		}

		/*Lấy danh sách món ăn của view*/
		private void GetFilterList()
		{
			FoodOnScreen = new List<FoodInfomation>();
			foreach (var food in view)
			{
				FoodOnScreen.Add((FoodInfomation)food);
			}
		}



		//---------------------------------------- Các hàm xử lý sự kiện --------------------------------------------//

		private void Cb_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			ComboBox cmb = (ComboBox)sender;

			cmb.IsDropDownOpen = true;

			if (!string.IsNullOrEmpty(cmb.Text))
			{
				string fullText = cmb.Text.Insert(GetChildOfType<TextBox>(cmb).CaretIndex, e.Text);
				cmb.ItemsSource = ListFoodInfo.Where(s => s.Name.IndexOf(fullText, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
			}
			else if (!string.IsNullOrEmpty(e.Text))
			{
				cmb.ItemsSource = ListFoodInfo.Where(s => s.Name.IndexOf(e.Text, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
			}
			else
			{
				cmb.ItemsSource = ListFoodInfo;
			}
		}

		private void PreviewKeyUp_EnhanceComboSearch(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Back || e.Key == Key.Delete)
			{
				ComboBox cmb = (ComboBox)sender;

				cmb.IsDropDownOpen = true;

				if (!string.IsNullOrEmpty(cmb.Text))
				{
					cmb.ItemsSource = ListFoodInfo.Where(s => s.Name.IndexOf(cmb.Text, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
				}
				else
				{
					cmb.ItemsSource = ListFoodInfo;
				}
			}
		}

		private void Pasting_EnhanceComboSearch(object sender, DataObjectPastingEventArgs e)
		{
			ComboBox cmb = (ComboBox)sender;

			cmb.IsDropDownOpen = true;

			string pastedText = (string)e.DataObject.GetData(typeof(string));
			string fullText = cmb.Text.Insert(GetChildOfType<TextBox>(cmb).CaretIndex, pastedText);

			if (!string.IsNullOrEmpty(fullText))
			{
				cmb.ItemsSource = ListFoodInfo.Where(s => s.Name.IndexOf(fullText, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
			}
			else
			{
				cmb.ItemsSource = ListFoodInfo;
			}
		}

		private void dockingContentControl_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Down)
			{
				SearchComboBox.SelectedIndex++;
			}
		}

		private void DishListTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var textBox = (TextBox)sender;
			if (textBox.Text != "" && AddDishList.Visibility == Visibility.Collapsed)
			{
				AddDishList.Visibility = Visibility.Visible;
			}
			else if (textBox.Text == "" && AddDishList.Visibility == Visibility.Visible)
			{
				AddDishList.Visibility = Visibility.Collapsed;
			}

		}

		private void DishInListTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var textBox = (TextBox)sender;
			if (textBox.Text != "" && AddDishInListItem.Visibility == Visibility.Collapsed)
			{
				AddDishInListItem.Visibility = Visibility.Visible;
			}
			else if (textBox.Text == "" && AddDishInListItem.Visibility == Visibility.Visible)
			{
				AddDishInListItem.Visibility = Visibility.Collapsed;
			}
		}

		private void searchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Down)
			{
				SearchComboBox.Focus();

				SearchComboBox.SelectedIndex = 0;
				SearchComboBox.IsDropDownOpen = true;
			}
			if (e.Key == Key.Escape)
			{
				SearchComboBox.IsDropDownOpen = false;
			}
		}

		//Cài đặt để có thể di chuyển cửa sổ khi nhấn giữ chuột và kéo Title Bar
		private void DockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var move = sender as System.Windows.Controls.DockPanel;
			var win = Window.GetWindow(move);
			win.DragMove();
		}



		//---------------------------------------- Các hàm lưu trữ dữ liệu --------------------------------------------//

		//Lưu lại danh sách món ăn
		private void SaveListFood()
		{
			XmlSerializer xs = new XmlSerializer(typeof(List<FoodInfomation>));
			TextWriter writer = new StreamWriter(@"data\Food.xml");
			xs.Serialize(writer, ListFoodInfo);
			writer.Close();
		}

		private void SaveListDish()
		{
			XmlSerializer xs = new XmlSerializer(typeof(BindingList<Dish>));
			TextWriter writer = new StreamWriter(@"data\Dish.xml");
			xs.Serialize(writer, ListDish);
			writer.Close();
		}

		private void SaveStepsImages()
		{
			string newFolder = GetAppDomain();

			foreach (var step in ListStep)
			{
				for (int i = 0; i < step.ImagesPathPerStep.Count; i++)
				{
					string stepPicName = $"{newFood.ID}_Step{step.Order}_Image{i + 1}.jpg";
					string newPath = $"{newFolder}\\images\\{stepPicName}";
					File.Copy(step.ImagesPathPerStep[i].ImagePath, newPath);
					step.ImagesPathPerStep[i].ImagePath = $"images\\{stepPicName}";
				}
			}
		}



		//---------------------------------------- Các hàm xử lý cập nhật giao diện --------------------------------------------//

		//Cập nhật trạng thái của nút phân trang trong các trường hợp
		private void UpdatePageButtonStatus()
		{
			//Vô hiệu hóa nút quay về trang trước và quay về trang đầu khi trang hiện tại là trang 1
			if (CurrentPage == 1)
			{
				PreviousPageButton.IsEnabled = false;
				PreviousPageTextBlock.Foreground = Brushes.Black;

				FirstPageButton.IsEnabled = false;
				FirstPageTextBlock.Foreground = Brushes.Black;
			}
			else if (PreviousPageButton.IsEnabled == false)
			{
				PreviousPageButton.IsEnabled = true;
				PreviousPageTextBlock.Foreground = Brushes.White;

				FirstPageButton.IsEnabled = true;
				FirstPageTextBlock.Foreground = Brushes.White;
			}

			//Vô hiệu hóa nút đi tới trang sau và đi tới trang cuối khi trang hiện tại là trang cuối
			if (CurrentPage == TotalPage)
			{
				NextPageButton.IsEnabled = false;
				NextPageTextBlock.Foreground = Brushes.Black;

				LastPageButton.IsEnabled = false;
				LastPageTextBlock.Foreground = Brushes.Black;
			}
			else if (NextPageButton.IsEnabled == false)
			{
				NextPageButton.IsEnabled = true;
				NextPageTextBlock.Foreground = Brushes.White;

				LastPageButton.IsEnabled = true;
				LastPageTextBlock.Foreground = Brushes.White;
			}
		}

		//Cập nhật lại thay đổi từ dữ liệu lên màn hình
		private void UpdateUIFromData()
		{
			view.Filter = Filter;

			/*Lấy danh sách thức ăn đã được lọc để khởi tạo lại số trang */
			GetFilterList();
			TotalPage = ((FoodOnScreen.Count - 1) / FoodperPage) + 1;
			CurrentPage = 1;

			var Foods = FoodOnScreen.Take(FoodperPage);
			foodButtonItemsControl.ItemsSource = Foods;

			TotalItem = FoodOnScreen.Count.ToString();
			if (FoodOnScreen.Count > 1)
			{
				TotalItem += " items";
			}
			else
			{
				TotalItem += " item";
			}
			UpdatePageButtonStatus();
		}

		/*Cập nhật lại danh sách món ăn trên màn hình sau khi nhấn thích*/
		private void UpdateFoodStatus()
		{
			view.Filter = Filter;
			GetFilterList();
			TotalPage = ((FoodOnScreen.Count - 1) / FoodperPage) + 1;
			if (CurrentPage > TotalPage)
			{
				CurrentPage--;
			}
			/*Lấy danh sách thức ăn đã được lọc để khởi tạo lại số trang */
			var Foods = FoodOnScreen.Skip((CurrentPage - 1) * FoodperPage).Take(FoodperPage);
			foodButtonItemsControl.ItemsSource = Foods;

			TotalItem = FoodOnScreen.Count.ToString();
			if (FoodOnScreen.Count > 1)
			{
				TotalItem += " items";
			}
			else
			{
				TotalItem += " item";
			}
			UpdatePageButtonStatus();
		}



		//---------------------------------------- Các hàm xử lý khác --------------------------------------------//

		public void Display(string url)
		{
			Match m = YouTubeURLIDRegex.Match(url);
			String id = m.Groups["v"].Value;
			string url1 = "http://www.youtube.com/embed/" + id;
			string page =
				 "<html>"
				+ "<head><meta http-equiv='X-UA-Compatible' content='IE=11' />"
				+ "<body>" + "\r\n"
				+ "<iframe src=\"" + url1 + "\" width=\"700\" height=\"400\" frameborder=\"0\" allowfullscreen></iframe>"
				+ "</body></html>";
			VideoPlayer.NavigateToString(page);
		}

		//private void SaveFoodData()
		//{
		//	string folderPath = GetAppDomain();
		//	string filePath = $"{folderPath}data\\Food.xml";
		//	XDocument doc = XDocument.Load(filePath);
		//	doc.Element("ArrayOfFoodInfomation").Add(
		//			new XElement
		//			(
		//					"FoodInfomation",
		//					new XElement("ID", newFood.ID),
		//					new XElement("Name", newFood.Name),
		//					new XElement("Dishs", newFood.Dishs),
		//					new XElement("IsFavorite", newFood.IsFavorite),
		//					new XElement("DateAdd", newFood.DateAdd),
		//					new XElement("ImagePath", newFood.ImagePath),
		//					new XElement("VideoLink", newFood.VideoLink),
		//					new XElement("Type", newFood.Type),
		//					new XElement("Level", newFood.Level)
		//			)
		//	);
		//	doc.Save(filePath);
		//}

		//private void SaveStepData()
		//{
		//	string folderPath = GetAppDomain();
		//	string filePath = $"{folderPath}data\\Step.xml";
		//	XDocument doc = XDocument.Load(filePath);
		//	foreach (Step step in ListStep)
		//	{
		//		doc.Element("ArrayOfStep").Add(
		//				new XElement
		//				(
		//						"Step",
		//						new XElement("ID", step.ID),
		//						new XElement("Order", step.Order),
		//						new XElement("StepImagePath", step.StepImagePath),
		//						new XElement("Content", step.Content)
		//				)
		//		);
		//	}
		//	doc.Save(filePath);
		//}
	}
}
