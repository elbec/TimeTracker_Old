using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimeTracker
{

    struct headData
    {
        public int id;
        public String title;
        public String subtitle;
        public String createDate;
        public timerData timerData;
        
    }

    struct timerData
    {

        public DateTime StartTime;
        public DateTime EndTime;
        public bool isTimerRunning;

        public TimeSpan getTotalDuration(int index)
        {
            if (StartTime != null)
            {
                var start = StartTime;
                var stop = EndTime;

                return stop.Subtract(start);
            }
            return TimeSpan.FromSeconds(10);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<headData> _allHeadData = new List<headData>();
        headData myHeaderData = new headData();
//        timerData myTimerData = new timerData();
        int id = 0;
        StackPanel mainStack;
        Popup codePopup = new Popup();
        private SolidColorBrush blueColor = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#003241"));


        public MainWindow()
        {
            InitializeComponent();

            mainStack = new StackPanel();
            mainStack.Orientation = Orientation.Vertical;

            DataGrid.Children.Add(mainStack);

            readFromJson();
            //set next id
            var lastId = 0;

            foreach (headData item in _allHeadData)
            {
                createNewEntry(item);
                if (item.id > lastId)
                    lastId = item.id;
            }
            id = lastId + 1;

            updateView();

        }

        ///  ###################### UPDATE ################################################

        private void updateView()
        {
            //Update TODAY, YESTERDAY,... Title
            List<StackPanel> allDayPanels = SearchVisualTreeAndReturnList(mainStack, "DayPanel_");
            if (allDayPanels != null)
            {
                foreach (StackPanel stack in allDayPanels)
                {
                    StackPanel titleStack = SearchVisualTree(stack, "stackTitle");

                    Label lab = SearchVisualTreeForLabel(titleStack, "titleLabel");
                    //get date
                    string titleStackDate = lab.Name.ToString();
                    String[] date = titleStackDate.Split('_');

                    String dateToDay = DateTime.Now.ToString("ddMMyyyy");
                    if (date[1] == dateToDay)
                    {
                        lab.Content = "TODAY";
                    }
                    else if (date[1] == DateTime.Now.AddDays(-1).ToString("ddMMyyyy"))
                    {
                        lab.Content = "YESTERDAY";
                    }
                    else
                    {
                        string[] formats = { "ddMMyyyy" };
                        var dateTime = DateTime.ParseExact(date[1], formats, new CultureInfo("en-US"), DateTimeStyles.None);
                        lab.Content = dateTime.DayOfWeek.ToString().ToUpper() + " " + dateTime.ToString("MMM. dd yyyy");
                    }

                    //Update Detail
                    StackPanel dayStack = SearchVisualTree(stack, "Day_" + date[1]);

                    List<StackPanel> detailStacks = SearchVisualTreeAndReturnList(dayStack, "Issue_");
                    foreach (StackPanel detail in detailStacks)
                    {
                        Label lab1 = SearchVisualTreeForLabel(detail, "Duration");
                        if (lab1 != null)
                        {
                            var myStack = lab1.Parent as StackPanel;
                            var myId = myStack.Name.Split('_');

                            var findData = _allHeadData.Find(x => x.id.ToString() == myId[1]);
                            lab1.Content = findData.timerData.getTotalDuration(0).ToString();
                         //   lab1.Content = myTimerData.getTotalDuration(int.Parse(myId[1])).ToString();
                        }

                    }
                }
            }
        }

        ///  ######################ADD FUNCTIONS################################################

        private StackPanel addDayStack(string dayName)
        {
            StackPanel dayPanel = new StackPanel();
            dayPanel.Orientation = Orientation.Vertical;
            dayPanel.Name = dayName;


            // ''''''''''TITLE''''''''''''''''''''''''''''''''''''
            StackPanel titleStack = new StackPanel();
            titleStack.Orientation = Orientation.Horizontal;
            titleStack.Name = "stackTitle";

            Label label = new Label();
            label.FontSize = 15;
            label.Foreground = Brushes.Green;
            label.Content = dayName;
            label.FontWeight = FontWeights.Bold;
            label.Name = "titleLabel" + dayName;

            StackPanel detailStack = new StackPanel();
                        detailStack.Orientation = Orientation.Vertical;
            //string currentDay = DateTime.Now.ToString("ddMMyyyy");
            string[] date = dayName.Split('_');
            detailStack.Name = "Day_" + date[1];

            Image minMaximize = new Image();
            BitmapImage image = new BitmapImage(new Uri("Assets/chevron-up-solid.png", UriKind.Relative));
            minMaximize.Source = image;
            minMaximize.Width = 20;
            minMaximize.Height = 20;

            minMaximize.MouseLeftButtonDown += (s, e) =>
            {
               // var id = 1;
               
                StackPanel stack = SearchVisualTree(dayPanel, "Day_" + date[1]);

                    if (stack.Visibility == Visibility.Visible)
                    {
                        minMaximize.Source = new BitmapImage(new Uri("Assets/chevron-down-solid.png", UriKind.Relative));
                    stack.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        minMaximize.Source = new BitmapImage(new Uri("Assets/chevron-up-solid.png", UriKind.Relative));
                    stack.Visibility = Visibility.Visible;
                    }
                            
            };

            titleStack.Children.Add(label);
            titleStack.Children.Add(minMaximize);

            dayPanel.Children.Add(titleStack);
            dayPanel.Children.Add(detailStack);
            return dayPanel;
        }

        private void addDetailStack(StackPanel parentStack, headData newData)
        {
            StackPanel issue = addIssue(newData);
           // id += 1;
            parentStack.Children.Add(issue);
        }

        private StackPanel addIssue(headData newData)
        {
            StackPanel titleSubtitleTime = new StackPanel();
            titleSubtitleTime.Name = "Issue_" + id.ToString() + "_" + newData.createDate;
            titleSubtitleTime.Orientation = Orientation.Horizontal;
            titleSubtitleTime.VerticalAlignment = VerticalAlignment.Center;

            Image startStopButton = new Image();
            BitmapImage image = new BitmapImage(new Uri("Assets/play-circle-regular.png", UriKind.Relative));
            startStopButton.Source = image;
            startStopButton.Width = 40;
            startStopButton.Height = 40;
            startStopButton.MouseLeftButtonDown += StartStopButton_MouseLeftButtonDown;

            StackPanel titleSubtitle = new StackPanel();
            titleSubtitle.Orientation = Orientation.Vertical;

            Label titleLabel = new Label();
            titleLabel.FontSize = 15;
            titleLabel.Foreground = Brushes.LightSlateGray;
            titleLabel.Content = newData.title;
            titleLabel.Width = 400;

            Label subtitleLabel = new Label();
            subtitleLabel.FontSize = 15;
            subtitleLabel.FontWeight = FontWeights.Bold;
            subtitleLabel.Foreground = blueColor;
            subtitleLabel.Content = newData.subtitle;
            subtitleLabel.Width = 400;

            titleSubtitle.Children.Add(titleLabel);
            titleSubtitle.Children.Add(subtitleLabel);

            Label totalDuration = new Label();
            totalDuration.Name = "Duration";
            totalDuration.FontSize = 15;
            totalDuration.Foreground = blueColor;
            totalDuration.Content = "3:00";
            totalDuration.Width = 90;
            totalDuration.Height = 30;

            titleSubtitleTime.Children.Add(startStopButton);
            titleSubtitleTime.Children.Add(titleSubtitle);
            titleSubtitleTime.Children.Add(totalDuration);

            return titleSubtitleTime;
        }

        /// ######################HELPER################################################
        private StackPanel SearchVisualTree(DependencyObject targetElement, string controlName)
        {
            var count = VisualTreeHelper.GetChildrenCount(targetElement);
            if (count == 0)
                return null;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(targetElement, i);
                if (child is StackPanel)
                {
                    StackPanel targetItem = (StackPanel)child;

                    if (targetItem.Name.Contains(controlName))
                    {
                        return targetItem;
                    }
                }
                else
                {
                    SearchVisualTree(child, controlName);
                }
            }
            return null;
        }

        private Label SearchVisualTreeForLabel(DependencyObject targetElement, string controlName)
        {
            var count = VisualTreeHelper.GetChildrenCount(targetElement);
            if (count == 0)
                return null;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(targetElement, i);
                if (child is Label)
                {
                    Label targetItem = (Label)child;

                    if (targetItem.Name.Contains(controlName))
                    {
                        return targetItem;
                    }
                }
            }
            return null;
        }

        private List<StackPanel> SearchVisualTreeAndReturnList(DependencyObject targetElement, string controlName)
        {
            List<StackPanel> allStackPanels = new List<StackPanel>();
            var count = VisualTreeHelper.GetChildrenCount(targetElement);
            if (count == 0)
                return null;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(targetElement, i);
                if (child is StackPanel)
                {
                    StackPanel targetItem = (StackPanel)child;

                    if (targetItem.Name.Contains(controlName))
                    {
                        allStackPanels.Add(targetItem);
                    }
                }
                else
                {
                    SearchVisualTree(child, controlName);
                }
            }
            return allStackPanels;
        }

        /// ######################ACTIONS################################################

        /// <summary>
        /// Action for start and stop button - left side of title subtitle stackPanel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void StartStopButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;

            var stack = image.Parent as StackPanel;
            string[] getId = stack.Name.Split('_');

            headData findData = _allHeadData.Find(x => x.id.ToString() == getId[1]);
            if (!findData.timerData.isTimerRunning)
            {
                image.Source = new BitmapImage(new Uri("Assets/stop-circle-regular.png", UriKind.Relative));
                var myId = stack.Name.Split('_');
                int index = int.Parse(getId[1]);

                timerData newTimerData = new timerData();
                newTimerData.StartTime = DateTime.Now;
                newTimerData.isTimerRunning = true;

                headData newData = new headData()
                {
                    id = findData.id,
                    title = findData.title,
                    subtitle = findData.subtitle,
                    createDate = findData.createDate,
                    timerData = newTimerData
                };

                _allHeadData[index] = newData;
            }
            else
            {
                image.Source = new BitmapImage(new Uri("Assets/play-circle-regular.png", UriKind.Relative));
                var myId = stack.Name.Split('_');
                int index = int.Parse(getId[1]);

                timerData oldTimerData = findData.timerData;
                oldTimerData.EndTime = DateTime.Now;
                oldTimerData.isTimerRunning = false;

                headData newData = new headData()
                {
                    id = findData.id,
                    title = findData.title,
                    subtitle = findData.subtitle,
                    createDate = findData.createDate,
                    timerData = oldTimerData
                };

                _allHeadData[index] = newData;
                writeToJson();
            }
            updateView();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();

            codePopup.Focus();
        }

        private void addNewProject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            showPopUp();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void SaveButton_MouseLeftButtonDown(object sender, EventArgs e, string titleText, string subtitleText)
        {
            myHeaderData.id = id;
            myHeaderData.title = titleText;
            myHeaderData.subtitle = subtitleText;
            myHeaderData.createDate = DateTime.Now.ToString("ddMMyyyy");
            codePopup.IsOpen = false;

            saveToJson();

            createNewEntry(myHeaderData);
        }

        private void createNewEntry(headData newData)
        {
            if (newData.title != "Title" && newData.subtitle != "Subtitle")
            {

                StackPanel mainStackPanel = SearchVisualTree(mainStack, "DayPanel_" + newData.createDate);

                if (mainStackPanel == null)
                {
                    StackPanel dayPanel = addDayStack("DayPanel_" + newData.createDate);
                    mainStack.Children.Add(dayPanel);
                    mainStackPanel = dayPanel;
                }

                StackPanel stack = SearchVisualTree(mainStackPanel, "Day_" + newData.createDate);
                if (stack != null)
                {
                    addDetailStack(stack, newData);
                }

                updateView();
            }
        }

        private void Image_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            //if (!myTimerData.isTimerRunning)
            //{
            //    myTimerData.Start(0);
            //    StartStopTimer.Source = new BitmapImage(new Uri("Assets/stop-circle-regular.png", UriKind.Relative));
            //} else
            //{
            //    myTimerData.Stop(0);
            //    StartStopTimer.Source = new BitmapImage(new Uri("Assets/play-circle-regular.png", UriKind.Relative));
            //}

        }

        /// ######################POPUP################################################

        private void showPopUp()
        {
            Border border = new Border();
            border.BorderBrush = blueColor;
            border.BorderThickness = new Thickness(2);

            TextBox title = new TextBox();
            title.Text = "Title";
            title.Height = 30;
            title.Width = 250;
            title.Margin = new Thickness(20, 20, 20, 10);
            title.Background = Brushes.White;
            title.Foreground = blueColor;
            title.Opacity = 0.8;

            TextBox subtitle = new TextBox();
            subtitle.Text = "Subtitle";
            subtitle.Height = 30;
            subtitle.Width = 250;
            subtitle.Margin = new Thickness(20, 0, 20, 10);
            subtitle.Background = Brushes.White;
            subtitle.Foreground = blueColor;

            Button button = new Button();
            button.Background = blueColor;
            button.Foreground = Brushes.White;
            button.Width = 100;
            button.Height = 30;
            button.Margin = new Thickness(0, 10, 0, 20);
            button.Content = "SAVE";
            button.Click += (sender, EventArgs) => { SaveButton_MouseLeftButtonDown(sender, EventArgs, title.Text, subtitle.Text); };

            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Vertical;
            stack.Background = Brushes.White;

            stack.Children.Add(title);
            stack.Children.Add(subtitle);
            stack.Children.Add(button);

            Grid grid = new Grid();
            grid.Children.Add(stack);
            grid.Children.Add(border);

            codePopup.Child = grid;

            codePopup.PlacementTarget = Add;
            codePopup.Placement = PlacementMode.Left;

         codePopup.IsOpen = true;

        }

        private void OpenPopupButton_MouseEnter(object sender, MouseEventArgs e)
        {
            codePopup.StaysOpen = true;
        }

        private void OpenPopupButton_MouseLeave(object sender, MouseEventArgs e)
        {
            codePopup.StaysOpen = false;
        }

        ///  ###################### JSON ################################################
        ///  
        public void saveToJson()
        {
            _allHeadData.Add(myHeaderData);
            writeToJson();
        }

        public void writeToJson()
        {
            using (StreamWriter file = File.CreateText(@"C:\Temp\output.txt"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, _allHeadData);
            }
        }

        public void readFromJson()
        {
            string path = @"C:\Temp\output.txt";
            if (File.Exists(path)) {
                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    List<headData> allData = (List<headData>)serializer.Deserialize(file, typeof(List<headData>));
                    if (allData != null)
                        _allHeadData = allData;
                }
            }
        }

        private void Close_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
