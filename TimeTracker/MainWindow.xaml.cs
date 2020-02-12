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

       
    

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Task task = new Task();
        List<Task> _allTasks = new List<Task>();

        StackPanel mainStack;
        Popup codePopup = new Popup();

        int id = 0;
        
        int runningTimerId;
        Image actualPlayStopImage;

        public MainWindow()
        {
            InitializeComponent();
            mainStack = new StackPanel();
            actualPlayStopImage = null;
            mainStack.Orientation = Orientation.Vertical;
            StopTimer.Visibility = Visibility.Hidden;

            DataGrid.Children.Add(mainStack);

            _allTasks = Json.readFromJson();
            createAllObjects();
        }

        private void deleteAllObjects()
        {
            mainStack.Children.Clear();
            id = 0;
        }

        private void createAllObjects()
        {
            if (_allTasks != null && _allTasks.Count > 0)
            {
                foreach (Task item in _allTasks)
                {
                    createNewEntry(item);
                }
                id = _allTasks.Last().id;
            }
            updateView();
        }

        ///  ###################### UPDATE ################################################

        private void updateView()
        {
            //Update TODAY, YESTERDAY,... Title
            List<StackPanel> allDayPanels = Helper.SearchVisualTreeAndReturnList(mainStack, "DayPanel_");
            if (allDayPanels != null)
            {
                foreach (StackPanel stack in allDayPanels)
                {
                    StackPanel titleStack = Helper.SearchVisualTree(stack, "stackTitle");

                    Label lab = Helper.SearchVisualTreeForLabel(titleStack, "titleLabel");
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
                    StackPanel dayStack = Helper.SearchVisualTree(stack, "Day_" + date[1]);

                    List<StackPanel> detailStacks = Helper.SearchVisualTreeAndReturnList(dayStack, "Issue_");
                    foreach (StackPanel detail in detailStacks)
                    {
                        Label lab1 = Helper.SearchVisualTreeForLabel(detail, "Duration");
                        if (lab1 != null)
                        {
                            var myStack = lab1.Parent as StackPanel;

                            var findData = _allTasks.Find(x => x.id == getIssueId(myStack));
                            lab1.Content = findData.timerData.getTotalDuration().ToString();
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
            label.Foreground = UXDefaults.ColorGreen;
            label.Content = dayName;
            label.FontWeight = FontWeights.Bold;
            label.Name = "titleLabel" + dayName;

            StackPanel detailStack = new StackPanel();
                        detailStack.Orientation = Orientation.Vertical;
            //string currentDay = DateTime.Now.ToString("ddMMyyyy");
            string[] date = dayName.Split('_');
            detailStack.Name = "Day_" + date[1];

            Image minMaximize = new Image();
            minMaximize.Source = ResourcePathToImageSource("up");
            minMaximize.Width = 12;
            minMaximize.Height = 12;

            minMaximize.MouseLeftButtonDown += (s, e) =>
            {
               
                StackPanel stack = Helper.SearchVisualTree(dayPanel, "Day_" + date[1]);

                    if (stack.Visibility == Visibility.Visible)
                    {
                    minMaximize.Source = ResourcePathToImageSource("down");
                    stack.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                    minMaximize.Source = ResourcePathToImageSource("up");
                    stack.Visibility = Visibility.Visible;
                    }  
            };

            titleStack.Children.Add(label);
            titleStack.Children.Add(minMaximize);

            dayPanel.Children.Add(titleStack);
            dayPanel.Children.Add(detailStack);
            return dayPanel;
        }

        private StackPanel addIssue(Task newData)
        {
            StackPanel titleSubtitleTime = new StackPanel();
            titleSubtitleTime.Name = "Issue_" + newData.id.ToString() + "_" + newData.createDate;
            titleSubtitleTime.Orientation = Orientation.Horizontal;
            titleSubtitleTime.VerticalAlignment = VerticalAlignment.Center;

            Image startStopButton = new Image();
            startStopButton.Source = ResourcePathToImageSource("play");
            startStopButton.Margin = new Thickness(10, 10, 10, 10);
            startStopButton.Width = 20;
            startStopButton.Height = 20;
            startStopButton.MouseLeftButtonDown += StartStopButton_MouseLeftButtonDown;

            Image editButton = new Image();
            editButton.Source = ResourcePathToImageSource("edit");
            editButton.Margin = new Thickness(5, 5, 5, 5);
            editButton.Width = 20;
            editButton.Height = 20;
            editButton.MouseLeftButtonDown += EditButton_MouseLeftButtonDown;

            Image deleteButton = new Image();
            deleteButton.Source = ResourcePathToImageSource("trash");
            deleteButton.Width = 20;
            deleteButton.Height = 20;
            deleteButton.MouseLeftButtonDown += DeleteButton_MouseLeftButtonDown;

            StackPanel titleSubtitle = new StackPanel();
            titleSubtitle.Orientation = Orientation.Vertical;

            Label titleLabel = new Label();
            titleLabel.FontSize = 15;
            titleLabel.Foreground = UXDefaults.ColorGray;
            titleLabel.Content = newData.title;
            titleLabel.Width = 400;

            Label subtitleLabel = new Label();
            subtitleLabel.FontSize = 15;
            subtitleLabel.FontWeight = FontWeights.Bold;
            subtitleLabel.Foreground = UXDefaults.ColorGray;

            subtitleLabel.Content = newData.subtitle;
            subtitleLabel.Width = 400;

            titleSubtitle.Children.Add(titleLabel);
            titleSubtitle.Children.Add(subtitleLabel);

            Label totalDuration = new Label();
            totalDuration.Name = "Duration";
            totalDuration.FontSize = 15;
            totalDuration.Foreground = UXDefaults.ColorBlue;
            totalDuration.Content = newData.timerData.getTotalDuration();
            totalDuration.Width = 90;
            totalDuration.Height = 30;

            titleSubtitleTime.Children.Add(startStopButton);
            titleSubtitleTime.Children.Add(editButton);
            titleSubtitleTime.Children.Add(deleteButton);
            titleSubtitleTime.Children.Add(titleSubtitle);
            titleSubtitleTime.Children.Add(totalDuration);

            return titleSubtitleTime;
        }


        /// ######################ACTIONS################################################
        private void StartStopButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;
            actualPlayStopImage = image;
            var stack = image.Parent as StackPanel;
            string[] getId = stack.Name.Split('_');

            Task findData = _allTasks.Find(x => x.id.ToString() == getId[1]);
            int findIndex = _allTasks.FindIndex(x => x.id.ToString() == getId[1]);
            runningTimerId = int.Parse(getId[1]);
            if (!findData.timerData.isTimerRunning)
            {
                image.Source = ResourcePathToImageSource("stop");
                StopTimer.Visibility = Visibility.Visible;

                Recorder newTimerData = new Recorder();
                newTimerData.StartTime = DateTime.Now;
                newTimerData.isTimerRunning = true;

                Task newData = new Task()
                {
                    id = findData.id,
                    title = findData.title,
                    subtitle = findData.subtitle,
                    createDate = findData.createDate,
                    timerData = newTimerData
                };

                _allTasks[findIndex] = newData;
            }
            else
            {
                image.Source = ResourcePathToImageSource("play");
                StopTimer.Visibility = Visibility.Hidden;

                Recorder oldTimerData = findData.timerData;
                oldTimerData.EndTime = DateTime.Now;
                oldTimerData.isTimerRunning = false;

                Task newData = new Task()
                {
                    id = findData.id,
                    title = findData.title,
                    subtitle = findData.subtitle,
                    createDate = findData.createDate,
                    timerData = oldTimerData
                };

                _allTasks[findIndex] = newData;
                Json.writeToJson(_allTasks);
                updateView();
            }
            
        }

        private void EditButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;
            actualPlayStopImage = image;
            var stack = image.Parent as StackPanel;
            string[] getId = stack.Name.Split('_');

            Task findData = _allTasks.Find(x => x.id.ToString() == getId[1]);
            int findIndex = _allTasks.FindIndex(x => x.id.ToString() == getId[1]);

            showPopUp();
        }

        private void DeleteButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image mySender = sender as Image;
            StackPanel parent = mySender.Parent as StackPanel;

            _allTasks.RemoveAll(r => r.id == getIssueId(parent));
            Json.writeToJson(_allTasks);
            deleteAllObjects();
            createAllObjects();
        }

        private int getIssueId(StackPanel stack)
        {
            var myId = stack.Name.Split('_');
            return int.Parse(myId[1]);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();

            codePopup.Focus();
        }

        private void addNewProject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            id += 1;
            showPopUp();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void SaveButton_MouseLeftButtonDown(object sender, EventArgs e, string titleText, string subtitleText)
        {
            task.id = id;
            task.title = titleText;
            task.subtitle = subtitleText;
            task.createDate = DateTime.Now.ToString("ddMMyyyy");
            codePopup.IsOpen = false;

            if (_allTasks == null)
            {
                _allTasks = new List<Task>{ task };
            }
            else
            {
                _allTasks.Add(task);
            }
            Json.writeToJson(_allTasks);
            createNewEntry(task);
        }
        private void createNewEntry(Task newData)
        {
            if (newData.title != "Title" && newData.subtitle != "Subtitle")
            {

                StackPanel mainStackPanel = Helper.SearchVisualTree(mainStack, "DayPanel_" + newData.createDate);

                if (mainStackPanel == null)
                {
                    StackPanel dayPanel = addDayStack("DayPanel_" + newData.createDate);
                    mainStack.Children.Add(dayPanel);
                    mainStackPanel = dayPanel;
                }

                StackPanel stack = Helper.SearchVisualTree(mainStackPanel, "Day_" + newData.createDate);
                if (stack != null)
                {
                    StackPanel issue = addIssue(newData);
                    stack.Children.Add(issue);
                }

                updateView();
            }
        }

        private void Image_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {

        }

        /// ######################POPUP################################################

        private void showPopUp()
        {
            Border border = new Border();
            border.BorderBrush = UXDefaults.ColorBlue;
            border.BorderThickness = new Thickness(2);

            TextBox title = new TextBox();
            title.Text = "Title";
            title.Height = 30;
            title.Width = 250;
            title.Margin = new Thickness(20, 20, 20, 10);
            title.Background = UXDefaults.ColorWhite;
            title.Foreground = UXDefaults.ColorBlue;
            title.Opacity = 0.8;

            TextBox subtitle = new TextBox();
            subtitle.Text = "Subtitle";
            subtitle.Height = 30;
            subtitle.Width = 250;
            subtitle.Margin = new Thickness(20, 0, 20, 10);
            subtitle.Background = UXDefaults.ColorWhite;
            subtitle.Foreground = UXDefaults.ColorBlue;

            Button button = new Button();
            button.Background = UXDefaults.ColorBlue;
            button.Foreground = UXDefaults.ColorWhite;
            button.Width = 100;
            button.Height = 30;
            button.Margin = new Thickness(0, 10, 0, 20);
            button.Content = "SAVE";
            button.Click += (sender, EventArgs) => { SaveButton_MouseLeftButtonDown(sender, EventArgs, title.Text, subtitle.Text); };

            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Vertical;
            stack.Background = UXDefaults.ColorWhite;

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
        private ImageSource ResourcePathToImageSource(string resourcesName)
        {
            return (DrawingImage)Application.Current.TryFindResource(resourcesName);
        }

        private void OpenPopupButton_MouseEnter(object sender, MouseEventArgs e)
        {
            codePopup.StaysOpen = true;
        }

        private void OpenPopupButton_MouseLeave(object sender, MouseEventArgs e)
        {
            codePopup.StaysOpen = false;
        }

        private void Close_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void StopTimer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Task findData = _allTasks.Find(x => x.id.ToString() == runningTimerId.ToString());
            int findIndex = _allTasks.FindIndex(x => x.id.ToString() == runningTimerId.ToString());
            if (findData.timerData.isTimerRunning) { 
               actualPlayStopImage.Source = ResourcePathToImageSource("play");
                StopTimer.Visibility = Visibility.Hidden;

                Recorder oldTimerData = findData.timerData;
                oldTimerData.EndTime = DateTime.Now;
                oldTimerData.isTimerRunning = false;

                Task newData = new Task()
                {
                    id = findData.id,
                    title = findData.title,
                    subtitle = findData.subtitle,
                    createDate = findData.createDate,
                    timerData = oldTimerData
                };

                _allTasks[findIndex] = newData;
                Json.writeToJson(_allTasks);
                updateView();
            }

        }
    }
}
