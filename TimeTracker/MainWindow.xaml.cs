using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace TimeTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppDbContext context = new AppDbContext();
        //List<Task> _allTasks = new List<Task>();
        private Task task { get; set; }
        private bool isEditing = false;
        Stopwatch TaskStopwatch = new Stopwatch();

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

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            DataGrid.Children.Add(mainStack);

            createAllObjects();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            LiveTimer.Content = TaskStopwatch.Elapsed;
        }

        public void deleteAllObjects()
        {
            mainStack.Children.Clear();
            id = 0;
        }

        public void createAllObjects()
        {
            context = new AppDbContext();
            if (context.Tasks.Count() > 0)
            {
                foreach (Task item in context.Tasks)
                {
                    createNewEntry(item);
                }
                id = context.Tasks.OrderByDescending(x => x.id).FirstOrDefault().id;
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
                            int findId = getIssueId(myStack);
                            Task findData = context.Tasks.Where(x => x.id == findId).Include(rec => rec.Recorders).FirstOrDefault();
                            TimeSpan timeSpan = TimeSpan.Zero;
                            if (findData.Recorders != null)
                            {
                                List<Recorder> findRecorders = findData.Recorders.ToList();

                                foreach (Recorder rec in findRecorders)
                                {
                                    timeSpan += GetTotalDuration(rec);
                                }
                            }
                            lab1.Content = timeSpan.ToString();
                        }

                    }
                }
            }
        }

        public TimeSpan GetTotalDuration(Recorder recorder)
        {
            if(recorder == null)
                return TimeSpan.Zero;
            return recorder.EndTime.Subtract(recorder.StartTime).StripMilliseconds();
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
            TimeSpan timeSpan = TimeSpan.Zero;
            if (newData.Recorders != null)
            {
                foreach (Recorder rec in newData.Recorders)
                {
                    timeSpan += GetTotalDuration(rec);
                }
            }
            totalDuration.Content = timeSpan;
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
            string getId = stack.Name.Split('_')[1];


            Task findData = context.Tasks.Where(x => x.id.ToString() == getId).Include(x => x.Recorders).FirstOrDefault();
            Recorder currentRecorder = null;
            if(findData.Recorders == null || findData.Recorders.Where(rec => rec.IsTimerRunning == true).Count() < 1)
            {
                Recorder newRecorder = new Recorder();
                currentRecorder = newRecorder;
                findData.Recorders.Add(newRecorder);
            } else
            {
                currentRecorder = findData.Recorders.Where(rec => rec.IsTimerRunning == true).First();
            }

            runningTimerId = int.Parse(getId);
            if (!currentRecorder.IsTimerRunning)
            {
                image.Source = ResourcePathToImageSource("stop");
                StopTimer.Visibility = Visibility.Visible;
                LiveTimer.Visibility = Visibility.Visible;
                LiveTimer.Content = TimeSpan.Zero;
                TaskStopwatch.Restart();

                currentRecorder.StartTime = DateTime.Now;
                currentRecorder.IsTimerRunning = true;
                context.SaveChanges();
            }
            else
            {
                image.Source = ResourcePathToImageSource("play");
                StopTimer.Visibility = Visibility.Hidden;
                LiveTimer.Visibility = Visibility.Collapsed;
                TaskStopwatch.Stop();

                currentRecorder.EndTime = DateTime.Now;
                currentRecorder.IsTimerRunning = false;

                if(GetTotalDuration(currentRecorder) <= new TimeSpan(0,0,5))
                    context.Recorders.Remove(currentRecorder);
                context.SaveChanges();

                //Json.writeToJson(_allTasks);
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

            //codePopup._allTasks = context.Tasks.ToList();
            codePopup.popup.PlacementTarget = image;
            codePopup.showTimeTextBox();
            codePopup.updateView(int.Parse(getId[1]));
            codePopup.popup.IsOpen = true;
        }

        private void DeleteButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image mySender = sender as Image;
            StackPanel parent = mySender.Parent as StackPanel;
            int idToDelete = getIssueId(parent);
            Task taskToDelete = context.Tasks.Where(x => x.id == idToDelete).Include(x => x.Recorders).FirstOrDefault();

            MessageBoxResult res = MessageBox.Show("Are you sure you want to Delete", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                context.Recorders.RemoveRange(taskToDelete.Recorders);
                context.Tasks.Remove(taskToDelete);
                context.SaveChanges();

                //updateView
                deleteAllObjects();
                createAllObjects();
            } 
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
            //codePopup._allTasks = _allTasks;
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
            if (newData.title != MyPopup.titleValue && newData.subtitle != MyPopup.subtitleValue)
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
            Task findData = context.Tasks.Where(x => x.id.ToString() == runningTimerId.ToString()).FirstOrDefault();
            //int findIndex = _allTasks.FindIndex(x => x.id.ToString() == runningTimerId.ToString());
            Recorder currentRecorder = findData.Recorders.Where(rec => rec.IsTimerRunning).First();
            if (currentRecorder.IsTimerRunning) {
                actualPlayStopImage.Source = ResourcePathToImageSource("play");
                StopTimer.Visibility = Visibility.Hidden;
                LiveTimer.Visibility = Visibility.Hidden;
                TaskStopwatch.Stop();
                currentRecorder.EndTime = DateTime.Now;
                currentRecorder.IsTimerRunning = false;
                context.SaveChanges();

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
                    Helper.CreateCSVFromGenericList(context.Tasks.ToList(), sfd.FileName);
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
