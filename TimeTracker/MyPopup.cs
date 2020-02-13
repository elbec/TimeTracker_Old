using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TimeTracker
{
    public partial class MyPopup : Popup
    {
        public Button saveButton;
        private TextBox titleTextBox;
        private TextBox subtitleTextBox;
        private TextBox startTimeTextBox;
        private TextBox stopTimeTextBox;
        public Popup popup = new Popup();
        public int currentID;
        public List<Task> _allTasks;
        private MainWindow _parentWin;
        private StackPanel stack = new StackPanel();
        private bool isEditing = false;

        public MyPopup(MainWindow parentWin)
        {
            configurePopup();
            _parentWin = parentWin;
        }

        public void configurePopup()
        {
            Border border = new Border();
            border.BorderBrush = UXDefaults.ColorBlue;
            border.BorderThickness = new Thickness(2);

            titleTextBox = new TextBox();
            titleTextBox.Text = "Title";
            titleTextBox.Height = 30;
            titleTextBox.Width = 250;
            titleTextBox.Margin = new Thickness(20, 20, 20, 10);
            titleTextBox.Background = UXDefaults.ColorWhite;
            titleTextBox.Foreground = UXDefaults.ColorBlue;
            titleTextBox.Opacity = 0.8;

            subtitleTextBox = new TextBox();
            subtitleTextBox.Text = "Subtitle";
            subtitleTextBox.Height = 30;
            subtitleTextBox.Width = 250;
            subtitleTextBox.Margin = new Thickness(20, 0, 20, 10);
            subtitleTextBox.Background = UXDefaults.ColorWhite;
            subtitleTextBox.Foreground = UXDefaults.ColorBlue;

            startTimeTextBox = new TextBox();
            startTimeTextBox.Name = "start_time";
            startTimeTextBox.Text = "00:00:00";
            startTimeTextBox.Height = 30;
            startTimeTextBox.Width = 250;
            startTimeTextBox.Margin = new Thickness(20, 0, 20, 10);
            startTimeTextBox.Background = UXDefaults.ColorWhite;
            startTimeTextBox.Foreground = UXDefaults.ColorBlue;

            stopTimeTextBox = new TextBox();
            stopTimeTextBox.Name = "stop_time";
            stopTimeTextBox.Text = "00:00:00";
            stopTimeTextBox.Height = 30;
            stopTimeTextBox.Width = 250;
            stopTimeTextBox.Margin = new Thickness(20, 0, 20, 10);
            stopTimeTextBox.Background = UXDefaults.ColorWhite;
            stopTimeTextBox.Foreground = UXDefaults.ColorBlue;

            saveButton = new Button();
            saveButton.Background = UXDefaults.ColorBlue;
            saveButton.Foreground = UXDefaults.ColorWhite;
            saveButton.Width = 100;
            saveButton.Height = 30;
            saveButton.Margin = new Thickness(0, 10, 0, 20);
            saveButton.Content = "SAVE";
            saveButton.Click += (sender, EventArgs) => { SaveButton_MouseLeftButtonDown(sender, EventArgs, titleTextBox.Text, subtitleTextBox.Text); };
           
            stack.Orientation = Orientation.Vertical;
            stack.Background = UXDefaults.ColorWhite;

            stack.Children.Add(titleTextBox);
            stack.Children.Add(subtitleTextBox);
            stack.Children.Add(startTimeTextBox);
            stack.Children.Add(stopTimeTextBox);
            stack.Children.Add(saveButton);

            Grid grid = new Grid();
            grid.Children.Add(stack);
            grid.Children.Add(border);

            popup.Child = grid;
            popup.Placement = PlacementMode.Left;
            popup.StaysOpen = false;
        }
        public void updateView(int id)
        {
            if (id == 0)
            {
                isEditing = false;
                titleTextBox.Text = "Title";
                subtitleTextBox.Text = "Subtitle";
            }
            else
            {
                isEditing = true;
                currentID = id;
                Task getTask = _allTasks.Find(x => x.id == currentID);
                titleTextBox.Text = getTask.title;
                subtitleTextBox.Text = getTask.subtitle;
                startTimeTextBox.Text = getTask.timerData.StartTime.ToString("HH:mm:ss");
                stopTimeTextBox.Text = getTask.timerData.EndTime.ToString("HH:mm:ss");
            }
        }
        public void hideTimeTextBox()
        {
            foreach(var textBox in stack.Children)
            {
                TextBox box = textBox as TextBox;
                if (box != null) {
                    if (box.Name.Contains("_time"))
                    {
                        box.Visibility = Visibility.Collapsed;
                    }
                }
               
            }
        }
        public void showTimeTextBox()
        {
            foreach (var textBox in stack.Children)
            {
                TextBox box = textBox as TextBox;
                if (box != null)
                {
                    if (box.Name.Contains("_time"))
                    {
                        box.Visibility = Visibility.Visible;
                    }
                }
            }
        }
        private void SaveButton_MouseLeftButtonDown(object sender, EventArgs e, string titleText, string subtitleText)
        {
            Task task = new Task();
            task.id = currentID;
            task.title = titleText;
            task.subtitle = subtitleText;
            task.createDate = DateTime.Now.ToString("ddMMyyyy");

            resetData();
            popup.IsOpen = false;

            if (_allTasks == null)
            {
                _allTasks = new List<Task> { task };
            }
            else
            {
                if (isEditing)
                {
                    int oldIndex = _allTasks.FindIndex(x => x.id == currentID);
                    task.timerData.StartTime = Convert.ToDateTime(startTimeTextBox.Text);
                    task.timerData.EndTime = Convert.ToDateTime(stopTimeTextBox.Text);
                    _allTasks[oldIndex] = task;
                }
                else
                {
                    _allTasks.Add(task);
                }
            }
            Json.writeToJson(_allTasks);
            _parentWin.myTask = task;
            
        }
        private void resetData()
        {
            titleTextBox.Text = "Title";
            subtitleTextBox.Text = "Subtitle";
        }
    }
}
