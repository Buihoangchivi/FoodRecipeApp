using System;
using System.Collections;
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
		private Button clickedControlButton, clickedTypeButton, clickedDishButton;
		private List<FoodInfomation> ListFoodInfo = new List<FoodInfomation>(); //Danh sách thông tin tất cả các món ăn
		BindingList<Step> ListStep = new BindingList<Step>();                   //Danh sách các bước của món ăn mới được thêm
		FoodInfomation newFood;                                                 //Món ăn mới được thêm
		private CollectionView view;
		BindingList<Dish> ListDish = new BindingList<Dish>();
		private List<FoodInfomation> FoodOnScreen;                              //Danh sách food để hiện trên màn hình
		private Condition FilterCondition = new Condition { Favorite = false, Type = "" };
		private Regex YouTubeURLIDRegex = new Regex(@"[\?&]v=(?<v>[^&]+)");
		private List<ColorSetting> ListColor;
		private Stack<List<object>> windowsStack = new Stack<List<object>>();
		private CollectionView DishIngredientView;
		private bool isEditMode;

		private bool checkFavoriteIsClicked, isMinimizeMenu;
		private int FoodperPage = 12;           //Số món ăn mỗi trang
		private int _totalPage = 0;             //Tổng số trang
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
		private int _currentPage = 1;           //Trang hiện tại
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
			public event PropertyChangedEventHandler PropertyChanged;
			private string _ImagePath;
			public string ImagePath
			{
				get
				{
					return _ImagePath;
				}
				set
				{
					_ImagePath = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs("ImagePath"));
					}
				}
			}
		}

		//Class lưu trữ màu trong Color setting
		public class ColorSetting
		{
			public string Color { get; set; }
		}

		//Class điều kiện để filter
		class Condition
		{
			public bool Favorite;
			public string Type;
		}

		//Class nguyên liệu cần chuẩn bị cho một món ăn
		public class Ingredient : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;
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
			public bool IsDone { get; set; }


			private string _type;
			public string Type
			{
				get
				{
					return _type;
				}
				set
				{
					_type = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs("Type"));
					}
				}
			}
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

			private int _Order;   //Thứ tự bước
			public int Order
			{
				get
				{
					return _Order;
				}
				set
				{
					_Order = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs("Order"));
					}
				}
			}
			private string _Content;   //Mô tả bước
			public string Content
			{
				get
				{
					return _Content;
				}
				set
				{
					_Content = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs("Content"));
					}
				}
			}

			private BindingList<ImagePerStep> _imagesPathPerStep;   //Đường dẫn các ảnh của bước
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
			private int _ID;       //ID món ăn 
			public int ID
			{
				get
				{
					return _ID;
				}
				set
				{
					_ID = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs("ID"));
					}
				}
			}
			private string _Name;       //Tên món ăn
			public string Name
			{
				get
				{
					return _Name;
				}
				set
				{
					_Name = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs("Name"));
					}
				}
			}
			private string _Ingredients;       //Danh sách nguyên liệu
			public string Ingredients
			{
				get
				{
					return _Ingredients;
				}
				set
				{
					_Ingredients = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs("Ingredients"));
					}
				}
			}
			private bool _IsFavorite;       //Món yêu thích
			public bool IsFavorite
			{
				get
				{
					return _IsFavorite;
				}
				set
				{
					_IsFavorite = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs("IsFavorite"));
					}
				}
			}
			private string _DateAdd;       //Ngày thêm
			public string DateAdd
			{
				get
				{
					return _DateAdd;
				}
				set
				{
					_DateAdd = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs("DateAdd"));
					}
				}
			}
			private string _VideoLink;       //Link youtube
			public string VideoLink
			{
				get
				{
					return _VideoLink;
				}
				set
				{
					_VideoLink = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs("VideoLink"));
					}
				}
			}
			private string _Level;        //Mức độ khó
			public string Level
			{
				get
				{
					return _Level;
				}
				set
				{
					_Level = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs("Level"));
					}
				}
			}
			private string _Type;        //Loại đồ ăn
			public string Type
			{
				get
				{
					return _Type;
				}
				set
				{
					_Type = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs("Type"));
					}
				}
			}
			private string _Discription;        //Mô tả khái quát món ăn
			public string Discription
			{
				get
				{
					return _Discription;
				}
				set
				{
					_Discription = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs("Discription"));
					}
				}
			}
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
			private BindingList<Step> _Steps;       //Các bước thực hiện của món ăn
			public BindingList<Step> Steps
			{
				get
				{
					return _Steps;
				}
				set
				{
					_Steps = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, new PropertyChangedEventArgs("Steps"));
					}
				}
			}
			public event PropertyChangedEventHandler PropertyChanged;

		}

		public MainWindow()
		{
			InitializeComponent();

			checkFavoriteIsClicked = false;
			isMinimizeMenu = false;

			ColorScheme = "ForestGreen";

			//Default buttons
			clickedTypeButton = AllButton;
			clickedTypeButton.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(ColorScheme);
			clickedControlButton = HomeButton;
			clickedDishButton = null;
			isEditMode = false;

			//Thêm màn hình Home vào stack
			List<object> list = new List<object>
			{
				PaginationBar,
				TypeBar,
				foodButtonItemsControl,
				clickedControlButton
			};
			windowsStack.Push(list);
			//clickedControlButton.Background = Brushes.LightSkyBlue;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
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
			searchComboBox.ItemsSource = ListFoodInfo;
			this.DataContext = this;

			//Lấy danh sách food
			var foods = FoodOnScreen.Take(FoodperPage);
			foodButtonItemsControl.ItemsSource = foods;
			view = (CollectionView)CollectionViewSource.GetDefaultView(ListFoodInfo);
			DishListItemsControl.ItemsSource = ListDish;

			//Tạo dữ liệu màu cho ListColor
			ListColor = new List<ColorSetting>
			{
				new ColorSetting { Color = "#FFCA5010"}, new ColorSetting { Color = "#FFFF8C00"}, new ColorSetting { Color = "#FFE81123"}, new ColorSetting { Color = "#FFD13438"}, new ColorSetting { Color = "#FFFF4081"},
				new ColorSetting { Color = "#FFC30052"}, new ColorSetting { Color = "#FFBF0077"}, new ColorSetting { Color = "#FF9A0089"}, new ColorSetting { Color = "#FF881798"}, new ColorSetting { Color = "#FF744DA9"},
				new ColorSetting { Color = "#FF4CAF50"}, new ColorSetting { Color = "#FF10893E"}, new ColorSetting { Color = "#FF018574"}, new ColorSetting { Color = "#FF03A9F4"}, new ColorSetting { Color = "#FF304FFE"},
				new ColorSetting { Color = "#FF0063B1"}, new ColorSetting { Color = "#FF6B69D6"}, new ColorSetting { Color = "#FF8E8CD8"}, new ColorSetting { Color = "#FF8764B8"}, new ColorSetting { Color = "#FF038387"},
				new ColorSetting { Color = "#FF525E54"}, new ColorSetting { Color = "#FF7E735F"}, new ColorSetting { Color = "#FF9E9E9E"}, new ColorSetting { Color = "#FF515C6B"}, new ColorSetting { Color = "#FF000000"}
			};

			//Binding dữ liệu màu cho Setting Color Table
			SettingColorItemsControl.ItemsSource = ListColor;
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
			clickedTypeButton.Foreground = Brushes.Gray;

			var button = (Button)sender;
			clickedTypeButton = button;
			button.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(ColorScheme);

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
			var button = (Button)sender;

			if (button != clickedControlButton)
			{

				//Đóng giao diện cũ trước khi nhấn nút
				if (!isEditMode &&( clickedControlButton == HomeButton || clickedControlButton == FavoriteButton))
				{
					//if (FoodDetailGrid.Visibility == Visibility.Collapsed)
					//{
					var listStack = windowsStack.Pop();
					var condition = new Condition { Favorite = FilterCondition.Favorite, Type = FilterCondition.Type };
					listStack.Insert(listStack.Count - 1, condition);
					windowsStack.Push(listStack);
					//}
					//else //Nếu đang ở màn hình chi tiết món ăn thì đóng màn hình chi tiết món ăn lại
					//{
					//	FoodDetailGrid.Visibility = Visibility.Collapsed;
					//}
				}
				else if (clickedControlButton == AddDishButton)
				{
					AddFood.DataContext = null;
					DefaultLevelComboxBoxItem.IsSelected = true;
					DefaultTypeComboxBoxItem.IsSelected = true;
					SaveOrDiscardBorder.Visibility = Visibility.Collapsed;
					EnterFoodNameTextBlock.Visibility = Visibility.Collapsed;
					ControlStackPanel.Visibility = Visibility.Visible;
					AddFoodAnhDishScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
				}
				else if (clickedControlButton == DishButton)
				{
					if (clickedDishButton != null)
					{
						clickedDishButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("LightGray");
						var text = GetChildOfType<TextBlock>(clickedDishButton);
						text.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("Black");
					}

					DishListNameTextBlock.DataContext = null;
					DishInListItemsControl.ItemsSource = null;
				}
				else
				{
					//Do nothing
				}

				//Đóng giao diện Panel hiện tại
				ProcessPanelVisible(Visibility.Collapsed);

				//Nếu nhấn sang cửa sổ thứ 2 thì hiển thị nút Back
				if (windowsStack.Count == 1)
				{
					BackButton.Visibility = Visibility.Visible;
				}
				else
				{
					//Do nothing
				}

				List<object> list = new List<object>();

				//Mở giao diện mới sau khi nhấn nút
				if (button == HomeButton)
				{
					FilterCondition.Favorite = false;

					//Xóa hết lịch sử các cửa sổ khác khi nhấn nút Home
					while (windowsStack.Count > 0)
					{
						windowsStack.Pop();
					}
					//Thêm màn hình Favorite vào stack
					list.Add(PaginationBar);
					list.Add(TypeBar);
					list.Add(foodButtonItemsControl);
					list.Add(FilterCondition);

					//Nếu nhấn sang nút Home thì không còn trang nào phía trước
					BackButton.Visibility = Visibility.Collapsed;
				}
				else if (button == FavoriteButton)
				{
					FilterCondition.Favorite = true;

					//Thêm màn hình Favorite vào stack
					list.Add(PaginationBar);
					list.Add(TypeBar);
					list.Add(foodButtonItemsControl);
					list.Add(FilterCondition);
				}
				else if (button == AddDishButton)
				{
					AddFoodAnhDishScrollViewer.ScrollToHome();
					ControlStackPanel.Visibility = Visibility.Collapsed;
					AddFoodAnhDishScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
					SortFoodList();
					ListStep = new BindingList<Step>();
					if (isEditMode == false)
					{
						//AddFood.DataContext = null;
						//DefaultLevelComboxBoxItem.IsSelected = true;
						//DefaultTypeComboxBoxItem.IsSelected = true;
						//SaveOrDiscardBorder.Visibility = Visibility.Collapsed;
						//EnterFoodNameTextBlock.Visibility = Visibility.Collapsed;

						var index = GetMinID();
						newFood = new FoodInfomation() { ID = index, VideoLink = "", Steps= new BindingList<Step>()};
					}
					else
					{
						var food = ListFoodInfo[CurrentElementIndex];
						newFood = food;
						//newFood = new FoodInfomation()
						//{
						//	PrimaryImagePath = food.PrimaryImagePath,
						//	DateAdd = food.DateAdd,
						//	Discription = food.Discription,
						//	ID = food.ID,
						//	Ingredients = food.Ingredients,
						//	IsFavorite = food.IsFavorite,
						//	Level = food.Level,
						//	Name = food.Name,
						//	Steps = food.Steps,
						//	Type = food.Type,
						//	VideoLink = food.VideoLink
						//};
						////AddFood.DataContext = ListFoodInfo[CurrentElementIndex];
						////ImageStepItemsControl.ItemsSource = ListFoodInfo[CurrentElementIndex].Steps;
						//foreach (var step in food.Steps)
						//{
						//	ListStep.Add(step);
						//}
					}

					AddFood.DataContext = newFood;
					ImageStepItemsControl.ItemsSource = newFood.Steps;
					//ListStep = newFood.Steps;

					//Thêm màn hình Add vào stack
					list.Add(AddFood);

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
					//Thêm màn hình Note vào stack
					list.Add(DishList);
				}
				else if (button == SettingButton)
				{
					list.Add(SettingStackPanel);
				}
				else
				{
					//Do nothing
				}

				//Cập nhật lại nút được chọn
				clickedControlButton = button;

				//Mở giao diện Panel vừa được chọn
				list.Add(clickedControlButton);
				windowsStack.Push(list);
				ProcessPanelVisible(Visibility.Visible);

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
			if (isEditMode == true)
			{
				isEditMode = false;
				//CheckAndReplaceTempImageFile();
				ControlStackPanel.Visibility = Visibility.Visible;
				AddFoodAnhDishScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
			}
			else
			{
				//Do nothing
			}

			//Đóng giao diện hiện tại
			ProcessPanelVisible(Visibility.Collapsed);

			//Lấy giao diện hiện tại ra khỏi Stack
			windowsStack.Pop();

			if (ControlStackPanel.Visibility == Visibility.Collapsed)
			{
				ControlStackPanel.Visibility = Visibility.Visible;
				AddFoodAnhDishScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
				SaveOrDiscardBorder.Visibility = Visibility.Collapsed;
			}
			else
			{
				//Do nothing
			}

			//Cập nhật lại nút được chọn
			var window = windowsStack.Peek();
			clickedControlButton = (Button)window[window.Count - 1];
			if (clickedControlButton == HomeButton || clickedControlButton == FavoriteButton)
			{
				if (window[0].GetType().Name != "Int32")
				{
					FilterCondition = (Condition)window[window.Count - 2];
					clickedTypeButton.Foreground = Brushes.Black;
					switch (FilterCondition.Type)
					{
						case "":
							AllButton.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(ColorScheme);
							clickedTypeButton = AllButton;
							break;
						case "Food":
							FoodButton.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(ColorScheme);
							clickedTypeButton = FoodButton;
							break;
						case "Drinks":
							DrinksButton.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(ColorScheme);
							clickedTypeButton = DrinksButton;
							break;
						default:
							break;
					}
				}
				else
				{
					FoodImage_Click(null, null);
				}
			}
			else
			{
				//Do nothing
			}

			//Cập nhật lại giao diện
			UpdateUIFromData();

			//Mở giao diện phía trước
			ProcessPanelVisible(Visibility.Visible);

			//Nếu đã quay về màn hình Home thì ẩn nút Back đi
			if (windowsStack.Count == 1)
			{
				BackButton.Visibility = Visibility.Collapsed;
			}
			else
			{
				//Do nothing
			}
		}

		private void FoodImage_Click(object sender, RoutedEventArgs e)
		{
			if (checkFavoriteIsClicked == false)
			{
				//Đóng giao diện Panel hiện tại
				ProcessPanelVisible(Visibility.Collapsed);

				//Lấy chỉ số của hình ảnh món ăn được nhấn
				if (sender != null)
				{
					CurrentElementIndex = GetElementIndexInArray((Button)sender);
				}
				else
				{
					if (e == null)
					{
						CurrentElementIndex = (int)windowsStack.Peek()[0];
					}
					else
					{
						if (isEditMode == false)
						{
							CurrentElementIndex = ListFoodInfo.Count - 1;
						}
						else
						{
							//Do nothing
						}
					}
				}

				//Binding dữ liệu để hiển thị chi tiết món ăn
				FoodDetailGrid.DataContext = ListFoodInfo[CurrentElementIndex];

				//Thay đổi màu chữ cho tiêu đề thông tin chi tiết món ăn
				FoodInfo_NameTextBlock.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(ColorScheme);
				FoodInfo_IngredientsTextBlock.Foreground = FoodInfo_NameTextBlock.Foreground;
				FoodInfo_DirectionsTextBlock.Foreground = FoodInfo_NameTextBlock.Foreground;
				FoodInfo_VideoTextBlock.Foreground = FoodInfo_NameTextBlock.Foreground;

				UpdatePaginationForDetailFoodUI();

				if (windowsStack.Count == 1)
				{
					var listStack = windowsStack.Pop();
					var condition = new Condition { Favorite = FilterCondition.Favorite, Type = FilterCondition.Type };
					listStack.Insert(listStack.Count - 1, condition);
					windowsStack.Push(listStack);
				}
				else
				{
					//Do nothing
				}

				if (sender != null || e != null)
				{
					//Mở giao diện chi tiết món ăn
					windowsStack.Push(new List<object> { CurrentElementIndex, FoodDetailScrollViewer, clickedControlButton });
					ProcessPanelVisible(Visibility.Visible);

					//Hiển thị nút quay lại
					if (BackButton.Visibility == Visibility.Collapsed)
					{
						BackButton.Visibility = Visibility.Visible;
					}
					else
					{
						//Do nothing
					}
				}
				else
				{
					//Do nothing
				}
			}
			else
			{
				int index = GetElementIndexInArray((Button)sender);

				//Nếu chưa yêu thích thì chuyển sang ảnh yêu thích và thêm vào danh sách yêu thích
				if (ListFoodInfo[index].IsFavorite == true)
				{
					ListFoodInfo[index].IsFavorite = false;
				}
				else //Nếu yêu thích rồi chuyển sang ảnh chưa yêu thích và xóa khỏi danh sách yêu thích
				{
					ListFoodInfo[index].IsFavorite = true;
				}

				checkFavoriteIsClicked = false;

				//Cập nhật lại giao diện
				UpdateFoodStatus();
			}
		}

		private void AddStepButton_Click(object sender, RoutedEventArgs e)
		{
			//newFood.Steps = new BindingList<Step>();
			if (newFood.Steps.Count == 0)
			{
				var newStep = new Step() { Order = 1, ImagesPathPerStep = new BindingList<ImagePerStep>() };
				newFood.Steps.Add(newStep);
			}
			else
			{
				var newStep = new Step()
				{
					Order = newFood.Steps[newFood.Steps.Count - 1].Order + 1,
					ImagesPathPerStep = new BindingList<ImagePerStep>()
				};
				newFood.Steps.Add(newStep);
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
			if (FoodTitleTextBox.Text != "")
			{
				//FoodInfomation food;
				//if (isEditMode == false)
				//{
				//	food = newFood;
				//}
				//else
				//{
				//	food = ListFoodInfo[CurrentElementIndex];
				//}
				newFood.DateAdd = DateTime.Now.ToString();
				var comboBoxItem = (ComboBoxItem)LevelComboBox.SelectedItem;
				newFood.Level = (string)comboBoxItem.Content;
				comboBoxItem = (ComboBoxItem)TypeComboBox.SelectedItem;
				newFood.Type = (string)comboBoxItem.Content;
				string newFolder = GetAppDomain();
				string foodPicName = $"{newFood.ID}.jpg";
				string newPath = $"{newFolder}images\\{foodPicName}";

				if (newFood.PrimaryImagePath != null)
				{
					//Nếu file đã tồn tại thì xóa để thêm mới
					if (File.Exists(newPath) == true)
					{
						//int index = 0;
						//do
						//{
						//	foodPicName = $"temp_{index}_{newFood.ID}.jpg";
						//	newPath = $"{newFolder}\\images\\{foodPicName}";
						//	index++;
						//}
						//while (File.Exists(newPath) == true);
						//File.Delete(newPath);
						if ($"{newFolder}{newFood.PrimaryImagePath}" != newPath)
						{
							File.Delete(newPath);

							File.Copy(newFood.PrimaryImagePath, newPath);
						}
                        else
                        {
							//do nothing
                        }
					}
					else
					{
						File.Copy(newFood.PrimaryImagePath, newPath);
					}
					
					newFood.PrimaryImagePath = $"images\\{foodPicName}";
				}
				else
				{
					//Do nothing
				}

				SaveStepsImages();


				//newFood.Steps = ListStep;
				if (isEditMode == false)
				{
					ListFoodInfo.Add(newFood);
				}
				else
				{
					ListFoodInfo[CurrentElementIndex] = newFood;
				}

				FoodDetailGrid.DataContext = newFood;

				UpdateUIFromData();
				SaveOrDiscardBorder.Visibility = Visibility.Collapsed;
				ProcessPanelVisible(Visibility.Collapsed);
				windowsStack.Pop();
				//FoodImage_Click(null, new RoutedEventArgs());

				UpdatePaginationForDetailFoodUI();
				ProcessPanelVisible(Visibility.Visible);

				isEditMode = false;
				ControlStackPanel.Visibility = Visibility.Visible;
				AddFoodAnhDishScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
			}
			else
			{
				EnterFoodNameTextBlock.Visibility = Visibility.Visible;
				AddFoodAnhDishScrollViewer.ScrollToHome();
			}
		}

		private void CancelFood_Click(object sender, RoutedEventArgs e)
		{
			if (SaveOrDiscardBorder.Visibility == Visibility.Collapsed)
			{
				SaveOrDiscardBorder.Visibility = Visibility.Visible;
			}
			else
			{
				SaveOrDiscardBorder.Visibility = Visibility.Collapsed;
			}
			AddFoodAnhDishScrollViewer.ScrollToEnd();
		}

		private void DiscardChanges_Click(object sender, RoutedEventArgs e)
		{
			CheckAndReplaceTempImageFile();
			isEditMode = false;
			ControlStackPanel.Visibility = Visibility.Visible;
			AddFoodAnhDishScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
			SaveOrDiscardBorder.Visibility = Visibility.Collapsed;
			BackButton_Click(null, null);
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
			var buttonSent = sender as Button;
			var ingredientToEdit = buttonSent.DataContext as Ingredient;
			var editIngredientDialog = new EditIngredientDialog(ColorScheme, ingredientToEdit);
			editIngredientDialog.Owner = this;
			editIngredientDialog.ShowDialog();
			if (editIngredientDialog.DialogResult == true)
			{
				for (int i = 0; i < ListDish.Count; i++)
				{
					ingredientToEdit.Type = editIngredientDialog.IngredientType;
					ingredientToEdit.IngredientName = editIngredientDialog.IngredientName;
				}

				UpdateIngredientGrouping();
			}
			else
			{
				//Xoá
				var currentGroceries = DishInListItemsControl.ItemsSource as BindingList<Ingredient>;
				for (int i = 0; i < currentGroceries.Count; i++)
				{
					if (ingredientToEdit == currentGroceries[i])
					{
						currentGroceries.Remove(ingredientToEdit);
					}
				}

			}
		}

		private void DishListName_Click(object sender, RoutedEventArgs e)
		{
			if (clickedDishButton != null)
			{
				clickedDishButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("LightGray");
				var text = GetChildOfType<TextBlock>(clickedDishButton);
				text.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("Black");
			}
			else
			{
				//Do nothing
			}

			var button = (Button)sender;
			clickedDishButton = button;

			button.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(ColorScheme);
			var textBlock = GetChildOfType<TextBlock>(button);
			textBlock.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("White");
			for (int i = 0; i < ListDish.Count; i++)
			{
				if (ListDish[i].DishName == textBlock.Text)
				{
					DishInListItemsControl.ItemsSource = ListDish[i].GroceriesList;
					DishListNameTextBlock.DataContext = ListDish[i];
					break;
				}
			}
			UpdateIngredientGrouping();
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
							ListDish[i].GroceriesList.Add(new Ingredient { IngredientName = text, IsDone = false, Type = "Other" });
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
			else
			{
				//Do nothing
			}

			if (NextStepButton.IsEnabled == false)
			{
				NextStepButton.IsEnabled = true;
				NextStepTextBlock.Foreground = Brushes.White;
			}
			else
			{
				//Do nothing
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
			else
			{
				//Do nothing
			}

			if (PreviousStepButton.IsEnabled == false)
			{
				PreviousStepButton.IsEnabled = true;
				PreviousStepTextBlock.Foreground = Brushes.White;
			}
			else
			{
				//Do nothing
			}
			//Tiến tới bước tiếp theo
			CurrentStep++;

			//Binging dữ liệu
			StepsGrid.DataContext = ListFoodInfo[CurrentElementIndex].Steps[CurrentStep - 1];
			ImagesPerStepItemsControl.ItemsSource = ListFoodInfo[CurrentElementIndex].Steps[CurrentStep - 1].ImagesPathPerStep;
		}

		private void ColorButton_Click(object sender, RoutedEventArgs e)
		{
			var datatContex = (sender as Button).DataContext;
			var color = (datatContex as ColorSetting).Color;
			ColorScheme = color;
			SettingTextBlock.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(ColorScheme);
			SettingTitleTextBlock.Foreground = SettingTextBlock.Background;
		}

		private void EditDetailFoodInfoButton_Click(object sender, RoutedEventArgs e)
		{
			isEditMode = true;
			ControlStackPanel.Visibility = Visibility.Collapsed;
			AddFoodAnhDishScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
			var tempButton = clickedControlButton;
			changeClickedControlButton_Click(AddDishButton, null);
			clickedControlButton = tempButton;
			//ProcessPanelVisible(Visibility.Collapsed);
			//windowsStack.Pop();
			//ProcessPanelVisible(Visibility.Visible);
		}

		private void DeleteFoodButton_Click(object sender, RoutedEventArgs e)
		{
			string newFolder = GetAppDomain();
			string foodPicName = $"{ListFoodInfo[CurrentElementIndex].ID}.jpg";
			string newPath = $"{newFolder}images\\{foodPicName}";
			if (File.Exists(newPath) == true)
			{
				File.Delete(newPath);
			}
			foreach(Step step in ListFoodInfo[CurrentElementIndex].Steps)
            {
				for(int i = 0; i < step.ImagesPathPerStep.Count; i++)
                {
					File.Delete(step.ImagesPathPerStep[i].ImagePath);
                }
            }
			ListFoodInfo.RemoveAt(CurrentElementIndex);
			ProcessPanelVisible(Visibility.Collapsed);
			windowsStack.Pop();
			ProcessPanelVisible(Visibility.Visible);
			UpdateUIFromData();
			isEditMode = false;
		}

		private void DeleteTextInSearchButton_Click(object sender, RoutedEventArgs e)
		{
			searchTextBox.Text = "";
		}

		private void SearchTextButton_Click(object sender, RoutedEventArgs e)
		{

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
			var curFood = new FoodInfomation();
			//Nếu nhấn hình ảnh món ăn ở màn hình Home
			if (button.Content.GetType().Name == "WrapPanel")
			{
				var wrapPanel = (WrapPanel)button.Content;
				curFood = (FoodInfomation)wrapPanel.DataContext;
			}
			else //Nếu nhấn món ăn ở trong nút Search
			{
				curFood = (FoodInfomation)button.DataContext;
			}

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

		private string ConvertToUnSign(string input)
		{
			input = input.Trim();
			for (int i = 0x20; i < 0x30; i++)
			{
				input = input.Replace(((char)i).ToString(), " ");
			}
			Regex regex = new Regex(@"\p{IsCombiningDiacriticalMarks}+");
			string str = input.Normalize(NormalizationForm.FormD);
			string str2 = regex.Replace(str, string.Empty).Replace('đ', 'd').Replace('Đ', 'D');
			while (str2.IndexOf("?") >= 0)
			{
				str2 = str2.Remove(str2.IndexOf("?"), 1);
			}
			return str2;
		}

		private void searchTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (e.Text != "\u001b")  //khác escapes
			{
				searchComboBox.IsDropDownOpen = true;
			}
			if (!string.IsNullOrEmpty(searchTextBox.Text))
			{
				string fullText = ConvertToUnSign(searchTextBox.Text.Insert(searchTextBox.CaretIndex, (e.Text)));
				searchComboBox.ItemsSource = ListFoodInfo.Where(s => ConvertToUnSign(s.Name).IndexOf(fullText, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
			}
			else if (!string.IsNullOrEmpty(e.Text))
			{
				searchComboBox.ItemsSource = ListFoodInfo.Where(s => ConvertToUnSign(s.Name).IndexOf(ConvertToUnSign(e.Text), StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
			}
			else
			{
				searchComboBox.ItemsSource = ListFoodInfo;
			}
		}

		private void PreviewKeyUp_EnhanceTextBoxSearch(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Back || e.Key == Key.Delete)
			{


				searchComboBox.IsDropDownOpen = true;

				if (!string.IsNullOrEmpty(searchTextBox.Text))
				{
					searchComboBox.ItemsSource = ListFoodInfo.Where(s => ConvertToUnSign(s.Name).IndexOf(ConvertToUnSign(searchTextBox.Text), StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
				}
				else
				{
					searchComboBox.ItemsSource = ListFoodInfo;
				}
			}
		}

		private void Pasting_EnhanceTextSearch(object sender, DataObjectPastingEventArgs e)
		{
			searchComboBox.IsDropDownOpen = true;

			string pastedText = (string)e.DataObject.GetData(typeof(string));
			//string fullText = searchComboBox.Text.Insert(GetChildOfType<TextBox>(searchComboBox).CaretIndex, pastedText);
			string fullText = searchTextBox.Text.Insert(searchTextBox.CaretIndex, (pastedText));

			if (!string.IsNullOrEmpty(fullText))
			{
				searchComboBox.ItemsSource = ListFoodInfo.Where(s => ConvertToUnSign(s.Name).IndexOf(ConvertToUnSign(fullText), StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
			}
			else
			{
				searchComboBox.ItemsSource = ListFoodInfo;
			}
		}

	

		private void DishListTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var textBox = (TextBox)sender;
			if (textBox.Text != "" && AddDishList.IsEnabled == false)
			{
				AddDishList.IsEnabled = true;
			}
			else if (textBox.Text == "" && AddDishList.IsEnabled == true)
			{
				AddDishList.IsEnabled = false;
			}

		}

		private void DishInListTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var textBox = (TextBox)sender;
			if (textBox.Text != "" && AddDishInListItem.IsEnabled == false)
			{
				AddDishInListItem.IsEnabled = true;
			}
			else if (textBox.Text == "" && AddDishInListItem.IsEnabled == true)
			{
				AddDishInListItem.IsEnabled = false;
			}
		}

		private void searchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Down)
			{
				searchComboBox.Focus();
				searchComboBox.SelectedIndex = 0;
				searchComboBox.IsDropDownOpen = true;
			}
			if (e.Key == Key.Escape)
			{
				searchComboBox.IsDropDownOpen = false;
				
			}
		}

		private void searchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int index = searchComboBox.SelectedIndex;
			if (index >= 0)
			{
				var selectedFood = searchComboBox.SelectedItem as FoodInfomation;
				string textSelected = selectedFood.Name;
				searchTextBox.Text = textSelected;
			}

			
		}

		private void searchComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key==Key.Enter)
            {

				Button button = new Button();
				button.DataContext = searchComboBox.SelectedItem as FoodInfomation;
				button.Content = "button";
				SearchFoodButton_Click(button, null);
			}
			//if (e.Key==Key.Escape)
   //         {
			//	searchTextBox.Text = "";
			//	searchTextBox.Focus();
   //         }
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
			FoodInfomation food;
			if (isEditMode == false)
			{
				food = newFood;
			}
			else
			{
				food = ListFoodInfo[CurrentElementIndex];
			}

			var a = ListStep;

			foreach (var step in newFood.Steps)
			{

				string stepPicName;
				string newPath;

				/*Copy lại ảnh mới*/
				for (int i = 0; i < step.ImagesPathPerStep.Count; i++)
				{
					stepPicName = $"{food.ID}_Step{step.Order}_Image{i + 1}.jpg";
					newPath = $"{newFolder}images\\{stepPicName}";
					if ($"{newFolder}{step.ImagesPathPerStep[i].ImagePath}" != newPath)
					{
						if (File.Exists(newPath))
						{
							File.Delete(newPath);
						}
						else
						{
							//do nothing
						}
						File.Copy(step.ImagesPathPerStep[i].ImagePath, newPath);
						step.ImagesPathPerStep[i].ImagePath = $"images\\{stepPicName}";
					}
					
				}
				/*Xoá ảnh cũ tránh dư thừa ảnh*/
				int index = step.ImagesPathPerStep.Count;
				stepPicName = $"{food.ID}_Step{step.Order}_Image{index + 1}.jpg";
				newPath = $"{newFolder}images\\{stepPicName}";
				while (File.Exists(newPath))
				{
					File.Delete(newPath);
					index++;
					stepPicName = $"{food.ID}_Step{step.Order}_Image{index + 1}.jpg";
					newPath = $"{newFolder}images\\{stepPicName}";
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

		private void UpdatePaginationForDetailFoodUI()
		{
			//Binding dữ liệu các hình ảnh của bước hiện tại đang được hiển thị trên màn hình
			if (ListFoodInfo[CurrentElementIndex].Steps.Count > 0)
			{
				StepsGrid.DataContext = ListFoodInfo[CurrentElementIndex].Steps[0];
				ImagesPerStepItemsControl.ItemsSource = ListFoodInfo[CurrentElementIndex].Steps[0].ImagesPathPerStep;

				//Đếm số bước bắt đầu từ 1
				CurrentStep = 1;

				//Hiển thị các bước nếu số bước lớn hơn 0
				DirectionsGrid.Visibility = Visibility.Visible;
			}
			else
			{
				//Không hiển thị các bước nếu số bước bằng 0
				DirectionsGrid.Visibility = Visibility.Collapsed;

				//Đếm số bước bắt đầu từ 0
				CurrentStep = 0;
			}

			//Hiển thị thanh phân trang cho số bước
			TotalStep = ListFoodInfo[CurrentElementIndex].Steps.Count;

			//Chức năng lùi về bước trước bị vô hiệu hóa khi đang ở bước đầu tiên
			PreviousStepButton.IsEnabled = false;
			PreviousStepTextBlock.Foreground = Brushes.Black;

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
				NextStepButton.IsEnabled = true;
				NextStepTextBlock.Foreground = Brushes.White;
			}

			//Hiển thị video mô tả món ăn
			Display(ListFoodInfo[CurrentElementIndex].VideoLink);
		}

		private void SearchFoodButton_Click(object sender, RoutedEventArgs e)
		{
			FoodImage_Click(sender, null);
		}

		private void FoodTitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (FoodTitleTextBox.Text != "" && EnterFoodNameTextBlock.Visibility == Visibility.Visible)
			{
				EnterFoodNameTextBlock.Visibility = Visibility.Collapsed;
			}
			else
			{
				//Do nothing
			}
		}

		private void UpdateIngredientGrouping()
		{
			DishIngredientView = (CollectionView)CollectionViewSource.GetDefaultView(DishInListItemsControl.ItemsSource);
			DishIngredientView.GroupDescriptions.Clear();
			PropertyGroupDescription groupDescription = new PropertyGroupDescription("Type");
			DishIngredientView.GroupDescriptions.Add(groupDescription);
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
		private void ProcessPanelVisible(Visibility state)
		{
			var window = windowsStack.Peek();
			foreach (var panel in window)
			{
				switch (panel.GetType().Name)
				{
					case "DockPanel":
						var dockPanel = (DockPanel)panel;
						dockPanel.Visibility = state;
						break;
					case "StackPanel":
						var stackPanel = (StackPanel)panel;
						stackPanel.Visibility = state;
						break;
					case "ItemsControl":
						var itemsControl = (ItemsControl)panel;
						itemsControl.Visibility = state;
						break;
					case "Grid":
						var grid = (Grid)panel;
						grid.Visibility = state;
						break;
					case "ScrollViewer":
						var scrollViewer = (ScrollViewer)panel;
						scrollViewer.Visibility = state;
						break;
					case "Button":
						//Hiển thị màu của thanh mới vừa được chọn
						var wrapPanel = (WrapPanel)(panel as Button).Content;
						var collection = wrapPanel.Children;
						var block = (TextBlock)collection[0];
						var text = (TextBlock)collection[2];
						if (state == Visibility.Visible)
						{
							block.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(ColorScheme);
							text.Foreground = block.Background;
						}
						else
						{
							block.Background = Brushes.Transparent;
							text.Foreground = Brushes.Black;
						}
						break;
					default:
						//Do nothing
						break;
				}
			}
		}

		private void CheckAndReplaceTempImageFile()
		{
			if (ListFoodInfo[CurrentElementIndex].PrimaryImagePath.Contains("images\\temp"))
			{
				string newFolder = GetAppDomain();
				string foodPicName = $"{ListFoodInfo[CurrentElementIndex].ID}.jpg";
				string path = $"{newFolder}\\images\\{foodPicName}";
				File.Delete(path);
			}
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
