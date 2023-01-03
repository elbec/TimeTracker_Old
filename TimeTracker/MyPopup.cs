using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

namespace TimeTracker
{
    public partial class MyPopup : Popup
    {
        public Button saveButton;
        private TextBox titleTextBox;
        private TextBox subtitleTextBox;
        //private TextBox startTimeTextBox;
        //private TextBox stopTimeTextBox;
        public Popup popup = new Popup();
        public int currentID;
        //public List<Task> _allTasks;
        private MainWindow _parentWin;
        private StackPanel stack = new StackPanel();
        private bool isEditing = false;

        public const string titleValue = "Title...";
        public const string subtitleValue = "Subtitle...";

        private AppDbContext context = new AppDbContext();

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
            titleTextBox.Text = titleValue;
            titleTextBox.GotFocus += (sender, e) => RemoveText(sender, e, titleValue);
            titleTextBox.LostFocus += (sender, e) => AddText(sender, e, titleValue);
            titleTextBox.Height = 30;
            titleTextBox.Width = 250;
            titleTextBox.Margin = new Thickness(20, 20, 20, 10);
            titleTextBox.Background = UXDefaults.ColorWhite;
            titleTextBox.Foreground = UXDefaults.ColorBlue;
            titleTextBox.Opacity = 0.8;

            subtitleTextBox = new TextBox();
            subtitleTextBox.Text = subtitleValue;
            subtitleTextBox.GotFocus += (sender, e) => RemoveText(sender, e, subtitleValue);
            subtitleTextBox.LostFocus += (sender, e) => AddText(sender, e, subtitleValue);
            subtitleTextBox.Height = 30;
            subtitleTextBox.Width = 250;
            subtitleTextBox.Margin = new Thickness(20, 0, 20, 10);
            subtitleTextBox.Background = UXDefaults.ColorWhite;
            subtitleTextBox.Foreground = UXDefaults.ColorBlue;

            //startTimeTextBox = new TextBox();
            //startTimeTextBox.Name = "start_time";
            //startTimeTextBox.Text = "00:00:00";
            //startTimeTextBox.Height = 30;
            //startTimeTextBox.Width = 250;
            //startTimeTextBox.Margin = new Thickness(20, 0, 20, 10);
            //startTimeTextBox.Background = UXDefaults.ColorWhite;
            //startTimeTextBox.Foreground = UXDefaults.ColorBlue;

            //stopTimeTextBox = new TextBox();
            //stopTimeTextBox.Name = "stop_time";
            //stopTimeTextBox.Text = "00:00:00";
            //stopTimeTextBox.Height = 30;
            //stopTimeTextBox.Width = 250;
            //stopTimeTextBox.Margin = new Thickness(20, 0, 20, 10);
            //stopTimeTextBox.Background = UXDefaults.ColorWhite;
            //stopTimeTextBox.Foreground = UXDefaults.ColorBlue;

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
            //stack.Children.Add(startTimeTextBox);
            //stack.Children.Add(stopTimeTextBox);
            stack.Children.Add(saveButton);

            Grid grid = new Grid();
            grid.Children.Add(stack);
            grid.Children.Add(border);

            popup.Child = grid;
            popup.Placement = PlacementMode.Left;
            popup.StaysOpen = false;
        }
        public void RemoveText(object sender, EventArgs e, string value)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text == value)
            {
                tb.Text = "";
            }
        }

        public void AddText(object sender, EventArgs e, string value)
        {
            TextBox tb = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(tb.Text))
                tb.Text = value;
        }
        public void updateView(int id)
        {
            if (id == 0)
            {
                isEditing = false;
                titleTextBox.Text = titleValue;
                subtitleTextBox.Text = subtitleValue;
            }
            else
            {
                isEditing = true;
                currentID = id;
                Task getTask = context.Tasks.Where(x => x.id == currentID).FirstOrDefault();
                titleTextBox.Text = getTask.title;
                subtitleTextBox.Text = getTask.subtitle;
                //startTimeTextBox.Text = getTask.Recorders.StartTime.ToString("HH:mm:ss");
                //stopTimeTextBox.Text = getTask.Recorders.EndTime.ToString("HH:mm:ss");
            }
        }
        public void hideTimeTextBox()
        {
            foreach(var textBox in stack.Children)
            {
                TextBox box = textBox as TextBox;
                if (box != null && box.Name.Contains("_time")) {
                        box.Visibility = Visibility.Collapsed;
                }
               
            }
        }
        public void showTimeTextBox()
        {
            foreach (var textBox in stack.Children)
            {
                TextBox box = textBox as TextBox;
                if (box != null && box.Name.Contains("_time"))
                        box.Visibility = Visibility.Visible;
            }
        }
        private void SaveButton_MouseLeftButtonDown(object sender, EventArgs e, string titleText, string subtitleText)
        {
            Task currentTask;

            if (isEditing)
            {
                currentTask = context.Tasks.Where(x => x.id == currentID).FirstOrDefault();
            }
            else
            {
                currentTask = new Task();
                currentTask.createDate = DateTime.Now.ToString("ddMMyyyy");
                currentTask.title = titleText;
                currentTask.subtitle = subtitleText;
            }

            currentTask.title = titleText;
            currentTask.subtitle = subtitleText;

            if (currentTask.title != MyPopup.titleValue && currentTask.subtitle != MyPopup.subtitleValue)
            {
                if(!isEditing)
                    context.Tasks.Add(currentTask);

                context.SaveChanges();
                //_parentWin.myTask = currentTask;
                _parentWin.deleteAllObjects();
                _parentWin.createAllObjects();
                popup.IsOpen = false;
            } 
        }
        private void ResetData()
        {
            titleTextBox.Text = titleValue;
            subtitleTextBox.Text = subtitleValue;
        }
    }
}
