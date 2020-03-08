using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace TimeTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Task> _allTasks = new List<Task>();
        private Task task { get; set; }
        private bool isEditing = false;

        public Task myTask
        {
            get
            {
                return task;
            }
            set
            {
                task = value;
                if (!isEditing)
                {
                    createNewEntry(task);
                    updateView();
                } else
                {
                    int findIndex = _allTasks.FindIndex(x => x.id == task.id);
                    _allTasks[findIndex] = task;
                    deleteAllObjects();
                    createAllObjects();
                }
            }
        }

        StackPanel mainStack;
        MyPopup codePopup;

        int id = 0;

        int runningTimerId;
        Image actualPlayStopImage;

        public MainWindow()
        {
            codePopup = new MyPopup(this);
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
            string[] date = dayName.Split('_');
            detailStack.Name = "Day_" + date[1];

            Image minMaximize = new Image();
            minMaximize.Source = ResourcePathToImageSource("up");
            minMaximize.Width = 12;
            minMaximize.Height = 12;
            minMaximize.Name = "minMaxImage";

            minMaximize.MouseLeftButtonDown += (s, e) =>
            {
                foldOrUnfoldDayStack(detailStack, titleStack);
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
            titleSubtitleTime.Margin = new Thickness(0, 5, 0, 5);

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
            editButton.MouseEnter += OpenPopupButton_MouseEnter;
            editButton.MouseLeave += ClosePopupButton_MouseLeave;

            Image deleteButton = new Image();
            deleteButton.Source = ResourcePathToImageSource("trash");
            deleteButton.Width = 20;
            deleteButton.Height = 20;
            deleteButton.Margin = new Thickness(0, 0, 10, 0);
            deleteButton.MouseLeftButtonDown += DeleteButton_MouseLeftButtonDown;

            StackPanel titleSubtitle = new StackPanel();
            titleSubtitle.Orientation = Orientation.Vertical;

            TextBlock titleLabel = new TextBlock();
            titleLabel.FontSize = 15;
            titleLabel.Foreground = UXDefaults.ColorGray;
            titleLabel.Text = newData.title;
            titleLabel.Width = 400;
            titleLabel.TextWrapping = TextWrapping.Wrap;

            TextBlock subtitleLabel = new TextBlock();
            subtitleLabel.FontSize = 15;
            subtitleLabel.FontWeight = FontWeights.Bold;
            subtitleLabel.Foreground = UXDefaults.ColorGray;
            subtitleLabel.Text = newData.subtitle;
            subtitleLabel.Width = 400;
            subtitleLabel.TextWrapping = TextWrapping.Wrap;

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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && (Keyboard.IsKeyDown(Key.OemPlus) || Keyboard.IsKeyDown(Key.Add)))
            {
                findAllDayPanels(Visibility.Visible);

            } else if (Keyboard.IsKeyDown(Key.LeftCtrl) && (Keyboard.IsKeyDown(Key.OemMinus) || Keyboard.IsKeyDown(Key.Subtract)))
            {
                findAllDayPanels(Visibility.Collapsed);
            }
        }

        private void findAllDayPanels(Visibility state)
        {
            List<StackPanel> allDayPanels = Helper.SearchVisualTreeAndReturnList(mainStack, "DayPanel_");
            if (allDayPanels != null)
            {
                foreach (StackPanel stack in allDayPanels)
                {
                    StackPanel titleStack = Helper.SearchVisualTree(stack, "stackTitle");
                    StackPanel detailStack = Helper.SearchVisualTree(stack, "Day_");
                    foldOrUnfoldDayStack(detailStack, titleStack, state);
                }
            }
        }

        private void StartStopButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;
            actualPlayStopImage = image;
            var stack = image.Parent as StackPanel;
            string[] getId = stack.Name.Split('_');

            Task findData = _allTasks.Find(x => x.id.ToString() == getId[1]);
            int findIndex = _allTasks.FindIndex(x => x.id.ToString() == getId[1]);
            runningTimerId = int.Parse(getId[1]);
            if (!findData.timerData.IsTimerRunning)
            {
                image.Source = ResourcePathToImageSource("stop");
                StopTimer.Visibility = Visibility.Visible;

                Recorder newTimerData = new Recorder();
                newTimerData.StartTime = DateTime.Now;
                newTimerData.IsTimerRunning = true;

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
                oldTimerData.IsTimerRunning = false;

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
            isEditing = true;
            var image = sender as Image;
            actualPlayStopImage = image;
            var stack = image.Parent as StackPanel;
            string[] getId = stack.Name.Split('_');

            codePopup._allTasks = _allTasks;
            codePopup.popup.PlacementTarget = image;
            codePopup.showTimeTextBox();
            codePopup.updateView(int.Parse(getId[1]));
            codePopup.popup.IsOpen = true;
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
            isEditing = false;
            codePopup._allTasks = _allTasks;
            codePopup.currentID = id;
            codePopup.popup.PlacementTarget = Add;
            codePopup.hideTimeTextBox();
            codePopup.updateView(0);
            codePopup.popup.IsOpen = true;
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Image image = sender as Image;
                ContextMenu contextMenu = image.ContextMenu;
                contextMenu.PlacementTarget = image;
                contextMenu.IsOpen = true;
                e.Handled = true;
            }
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

        private ImageSource ResourcePathToImageSource(string resourcesName)
        {
            return (DrawingImage)Application.Current.TryFindResource(resourcesName);
        }

        private void OpenPopupButton_MouseEnter(object sender, MouseEventArgs e)
        {
            codePopup.popup.StaysOpen = true;
        }

        private void ClosePopupButton_MouseLeave(object sender, MouseEventArgs e)
        {
            codePopup.popup.StaysOpen = false;
        }

        private void Close_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void StopTimer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Task findData = _allTasks.Find(x => x.id.ToString() == runningTimerId.ToString());
            int findIndex = _allTasks.FindIndex(x => x.id.ToString() == runningTimerId.ToString());
            if (findData.timerData.IsTimerRunning) {
                actualPlayStopImage.Source = ResourcePathToImageSource("play");
                StopTimer.Visibility = Visibility.Hidden;

                Recorder oldTimerData = findData.timerData;
                oldTimerData.EndTime = DateTime.Now;
                oldTimerData.IsTimerRunning = false;

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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item.Name == "ExportItem")
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "csv files (*.csv)|*.csv";
                sfd.RestoreDirectory = true;

                if (sfd.ShowDialog() == true)
                {
                    Helper.CreateCSVFromGenericList(_allTasks, sfd.FileName);
                }
            }
        }

        /// ######################HELPER################################################

        private void foldOrUnfoldDayStack(StackPanel detailStack, StackPanel titleStack, Visibility? newState = null)
        {
            Image img = Helper.SearchVisualTreeForImage(titleStack, "minMaxImage");
            if (newState == null)
            {
                if (detailStack.Visibility == Visibility.Visible)
                {
                    img.Source = ResourcePathToImageSource("down");
                    detailStack.Visibility = Visibility.Collapsed;
                }
                else
                {
                    img.Source = ResourcePathToImageSource("up");
                    detailStack.Visibility = Visibility.Visible;
                }
            } else if (newState == Visibility.Collapsed)
            {
                img.Source = ResourcePathToImageSource("down");
                detailStack.Visibility = Visibility.Collapsed;

            } else if (newState == Visibility.Visible)
            {
                img.Source = ResourcePathToImageSource("up");
                detailStack.Visibility = Visibility.Visible;
            }
        }
    }
}
