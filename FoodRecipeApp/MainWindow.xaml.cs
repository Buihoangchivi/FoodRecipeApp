using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
		public event PropertyChangedEventHandler PropertyChanged;

		private Button clickedControlButton;
		private bool checkFavoriteIsClicked, isMinimizeMenu;
		private List<FoodInfomation> _list = new List<FoodInfomation>();
		private CollectionView view;
		public enum FoodType { Food, Drinks };

		private List<FoodInfomation> FoodOnScreen;                        //Danh sách food để hiện trên màn hình

		private int FoodperPage = 12;                           //Số món ăn mỗi trang

		private int _totalPage = 0;                             //Tổng số trang

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

		private int _currentPage = 1;                           //Trang hiện tại

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

		class Condition
		{
			public bool Favorite;
			public FoodType? Type;
		}

		private Condition FilterCondition = new Condition { Favorite = false, Type = null };

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

		BindingList<Step> ListSteps = new BindingList<Step>();
		FoodInfomation newFood;                     //Món ăn thêm

		public partial class FoodInfomation : INotifyPropertyChanged
		{
			public int ID { get; set; }              //ID món ăn 
			public string Name { get; set; }            // Tên món ăn
			public string Ingredients { get; set; }        //Danh sách nguyên liệu
			public bool IsFavorite { get; set; }        //Món yêu thích
			public DateTime DateAdd { get; set; }       //ngày thêm

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
			public string VideoLink { get; set; }
			public string Level { get; set; }

			public FoodType Type { get; set; }

			public string Discription { get; set; }

			public event PropertyChangedEventHandler PropertyChanged;

		}

		//class FoodImageDAO
		//{
		//	public static List<FoodInfomation> GetAll()
		//	{
		//		var result = new List<FoodInfomation>()
		//		{
		//			new FoodInfomation() { Name="Chu Tùng Nhân", Image="Images/playerImage01.jpg", Favorite=false, Type=FoodType.Food },
		//			new FoodInfomation() { Name="Nguyen Ánh Du", Image="Images/playerImage02.jpg", Favorite=true , Type=FoodType.Food },
		//			new FoodInfomation() { Name="Lều Bách Khánh", Image="Images/playerImage03.jpg", Favorite=false , Type=FoodType.Drinks },
		//			new FoodInfomation() { Name="Thiều Duy Hành", Image="Images/playerImage04.jpg", Favorite=true , Type=FoodType.Drinks },
		//			new FoodInfomation() { Name="Nhiệm Băng Đoan", Image="Images/playerImage05.jpg", Favorite=false , Type=FoodType.Food },
		//			new FoodInfomation() { Name="Mang Đình Từ", Image="Images/playerImage06.jpg", Favorite=false , Type=FoodType.Food },
		//			new FoodInfomation() { Name="Bùi Tuyền", Image="Images/playerImage07.jpg", Favorite=false , Type=FoodType.Drinks },
		//			new FoodInfomation() { Name="Triệu Triều Hải", Image="Images/playerImage08.jpg", Favorite=false , Type=FoodType.Drinks },
		//			new FoodInfomation() { Name="Tạ Đoan Huệ", Image="Images/playerImage09.jpg", Favorite=false , Type=FoodType.Food },
		//			new FoodInfomation() { Name="Đào Sương Thư", Image="Images/playerImage10.jpg", Favorite=false , Type=FoodType.Food }
		//		};

		//		return result;
		//	}
		//}

		/*Lưu lại danh sách món ăn*/
		private void SaveListFood()
        {
			XmlSerializer xs = new XmlSerializer(typeof(List<FoodInfomation>));
			TextWriter writer = new StreamWriter(@"data\Food.xml");
			xs.Serialize(writer, _list);
			writer.Close();
		}
		
		//Cài đặt nút đóng cửa sổ
		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			SaveListFood();
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

			//Display("https://www.youtube.com/watch?v=qGRU3sRbaYw");
			//this.DataContext = newFood;

			checkFavoriteIsClicked = false;
			isMinimizeMenu = false;

			//Default buttons
			//clickedTypeButton = AllButton;
			//clickedTypeButton.Background = Brushes.LightSkyBlue;
			clickedControlButton = HomeButton;
			//clickedControlButton.Background = Brushes.LightSkyBlue;
		}

		////Cập nhật lại thay đổi từ dữ liệu lên màn hình
		//private void UpdateUIFromData()
		//{
		//	view.Filter = Filter;
		//	CollectionViewSource.GetDefaultView(foodButtonItemsControl.ItemsSource).Refresh();
		//}

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

			//CollectionViewSource.GetDefaultView(foodButtonItemsControl.ItemsSource).Refresh();
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
				FilterCondition.Type = null;
			}
			else if (button == FoodButton)
			{
				FilterCondition.Type = FoodType.Food;
			}
			else if (button == DrinksButton)
			{
				FilterCondition.Type = FoodType.Drinks;
			}
			else
			{
				//Do nothing
			}

			//Cập nhật lại giao diện
			UpdateUIFromData();
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
				if (clickedControlButton == HomeButton || clickedControlButton == FavoriteButton)
				{
					TypeBar.Visibility = Visibility.Collapsed;
					foodButtonItemsControl.Visibility = Visibility.Collapsed;
				}
				else if (clickedControlButton == AddDishButton)
				{
					AddFood.Visibility = Visibility.Collapsed;
				}
				else if (clickedControlButton == IngredientButton)
				{

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
				}
				else if (button == FavoriteButton)
				{
					FilterCondition.Favorite = true;
					TypeBar.Visibility = Visibility.Visible;
					foodButtonItemsControl.Visibility = Visibility.Visible;
				}
				else if (button == AddDishButton)
				{
					SortFoodList();
					var index = GetMinID();
					newFood = new FoodInfomation() { ID = index, Type = FoodType.Food };
					AddFood.Visibility = Visibility.Visible;
					AddFood.DataContext = newFood;
					AddDirectionItemsControl.ItemsSource = ListSteps;
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

				//Hiển thị màu của thanh mới vừa được chọn
				wrapPanel = (WrapPanel)clickedControlButton.Content;
				collection = wrapPanel.Children;
				block = (TextBlock)collection[0];
				text = (TextBlock)collection[2];
				block.Background = Brushes.Red;
				text.Foreground = Brushes.Red;

				//Cập nhật lại giao diện
				UpdateUIFromData();
			}
			else
			{
				//Do nothing
			}
		}

		private void SortFoodList()
		{
			FoodInfomation temp;
			for (int i = 0; i < _list.Count - 1; i++)
			{
				for (int j = i + 1; j < _list.Count; j++)
				{
					if (_list[i].ID > _list[j].ID)
					{
						temp = _list[i];
						_list[i] = _list[j];
						_list[j] = temp;
					}
				}
			}
		}

		private int GetMinID()
		{
			int result = 1;
			for (int i = 0; i < _list.Count; i++)
			{
				if (result < _list[i].ID)
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

		private bool Filter(object item)
		{
			bool result;
			var foodInfo = (FoodInfomation)item;
			if (FilterCondition.Favorite == true && foodInfo.IsFavorite == false)
			{
				result = false;
			}
			else if (FilterCondition.Type != null && FilterCondition.Type != foodInfo.Type)
			{
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//Đọc dữ liệu từ data

			XmlSerializer xs = new XmlSerializer(typeof(List<FoodInfomation>));
			using (var reader = new StreamReader(@"data\Food.xml"))
			{
				_list = (List<FoodInfomation>)xs.Deserialize(reader);
			}
			FoodOnScreen = _list;
			TotalPage = (_list.Count - 1) / FoodperPage + 1;

			this.DataContext = this;	//Binding Số trang

			/*Lấy danh sách food*/
			var foods = FoodOnScreen.Take(FoodperPage);
			foodButtonItemsControl.ItemsSource = foods;
			//view = (CollectionView)CollectionViewSource.GetDefaultView(foodButtonItemsControl.ItemsSource);
			view = (CollectionView)CollectionViewSource.GetDefaultView(_list);

		}

		private void Menu_Click(object sender, RoutedEventArgs e)
		{
			if (isMinimizeMenu == false)
			{
				ControlStackPanel.Width = 47;
				isMinimizeMenu = true;
			}
			else
			{
				ControlStackPanel.Width = 250;
				isMinimizeMenu = false;
			}
		}

		private int GetElementIndexInArray(Button button)
		{
			var wrapPanel = (WrapPanel)button.Content;
			//var collection = wrapPanel.Children;
			//var rectangle = (Rectangle)collection[0];
			//var imageBrush = (ImageBrush)rectangle.Fill;
			//var imageSource = imageBrush.ImageSource;
			//var imageSourceString = imageSource.ToString();
			//var imageName = imageSourceString.Substring(23);
			var curFood = wrapPanel.DataContext as FoodInfomation;
			var result = 0;
			for (int i = 0; i < _list.Count; i++)
			{
				if (curFood == _list[i])
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
						_list[index].ImagePath,
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
				int index = GetElementIndexInArray((Button)sender);

				//Nếu chưa yêu thích thì chuyển sang ảnh yêu thích và thêm vào danh sách yêu thích
				if (_list[index].IsFavorite == true)
				{
					_list[index].IsFavorite = false;
					//imgName = "Images/unloved.png";
				}
				else //Nếu yêu thích rồi chuyển sang ảnh chưa yêu thích và xóa khỏi danh sách yêu thích
				{
					_list[index].IsFavorite = true;
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

		private Regex YouTubeURLIDRegex = new Regex(@"[\?&]v=(?<v>[^&]+)");
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

		private void AddStepButton_Click(object sender, RoutedEventArgs e)
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

		private void AddPrimaryFoodPhoto_Click(object sender, RoutedEventArgs e)
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

		private void AddOtherFoodPhoto_Click(object sender, RoutedEventArgs e)
		{
			var fileDialog = new OpenFileDialog();
			fileDialog.Filter = "Image Files(*.JPG*)|*.JPG";
			fileDialog.Title = "Select Image";
			if (fileDialog.ShowDialog() == true)
			{
				var container = FindParent<StackPanel>(sender as DependencyObject);
				string filePath = fileDialog.FileName;
				if (container != null)
				{
					var currData = container.DataContext as Step;
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
			doc.Element("ArrayOfFoodInfomation").Add(
					new XElement
					(
							"FoodInfomation",
							new XElement("ID", newFood.ID),
							new XElement("Name", newFood.Name),
							new XElement("Ingredients", newFood.Ingredients),
							new XElement("IsFavorite", newFood.IsFavorite),
							new XElement("DateAdd", newFood.DateAdd),
							new XElement("ImagePath", newFood.ImagePath),
							new XElement("VideoLink", newFood.VideoLink),
							new XElement("Type", newFood.Type)
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

		private void SaveFood_Click(object sender, RoutedEventArgs e)
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

			_list.Add(newFood);
			//foodButtonItemsControl.ItemsSource = _list;
			view = (CollectionView)CollectionViewSource.GetDefaultView(_list);
			UpdateUIFromData();
		}

		private void Favorite_Click(object sender, RoutedEventArgs e)
		{
			checkFavoriteIsClicked = true;
		}

		private void btnNextPage_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentPage < TotalPage)
			{
				var foods = FoodOnScreen.Skip(CurrentPage * FoodperPage).Take(FoodperPage);
				CurrentPage++;
				foodButtonItemsControl.ItemsSource = foods;
			}
		}

		/*Về trang trước*/
		private void btnPrevPage_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentPage > 1)
			{
				CurrentPage--;
				var foods = FoodOnScreen.Skip((CurrentPage - 1) * FoodperPage).Take(FoodperPage);
				foodButtonItemsControl.ItemsSource = foods;
			}
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

		private void Cb_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
			ComboBox cmb = (ComboBox)sender;

			cmb.IsDropDownOpen = true;

			if (!string.IsNullOrEmpty(cmb.Text))
			{
				string fullText = cmb.Text.Insert(GetChildOfType<TextBox>(cmb).CaretIndex, e.Text);
				cmb.ItemsSource = _list.Where(s => s.Name.IndexOf(fullText, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
			}
			else if (!string.IsNullOrEmpty(e.Text))
			{
				cmb.ItemsSource = _list.Where(s => s.Name.IndexOf(e.Text, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
			}
			else
			{
				cmb.ItemsSource = _list;
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
					cmb.ItemsSource = _list.Where(s => s.Name.IndexOf(cmb.Text, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
				}
				else
				{
					cmb.ItemsSource = _list;
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
				cmb.ItemsSource = _list.Where(s => s.Name.IndexOf(fullText, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
			}
			else
			{
				cmb.ItemsSource = _list;
			}
		}

        private void dockingContentControl_KeyDown(object sender, KeyEventArgs e)
        {
			if (e.Key == Key.Down)
			{
				SearchComboBox.SelectedIndex++;
			}
		}

        /*Lấy danh sách móna ăn của view*/
        private void GetFilterList()
		{
			FoodOnScreen = new List<FoodInfomation>();
			foreach (var food in view)
			{
				FoodOnScreen.Add((FoodInfomation)food);
			}
		}
	}
}
