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
        public Popup popup = new Popup();
        public int currentID;
        public List<Task> _allTasks = new List<Task>();
        private MainWindow _parentWin;

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

            saveButton = new Button();
            saveButton.Background = UXDefaults.ColorBlue;
            saveButton.Foreground = UXDefaults.ColorWhite;
            saveButton.Width = 100;
            saveButton.Height = 30;
            saveButton.Margin = new Thickness(0, 10, 0, 20);
            saveButton.Content = "SAVE";
            saveButton.Click += (sender, EventArgs) => { SaveButton_MouseLeftButtonDown(sender, EventArgs, titleTextBox.Text, subtitleTextBox.Text); };

            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Vertical;
            stack.Background = UXDefaults.ColorWhite;

            stack.Children.Add(titleTextBox);
            stack.Children.Add(subtitleTextBox);
            stack.Children.Add(saveButton);

            Grid grid = new Grid();
            grid.Children.Add(stack);
            grid.Children.Add(border);

            popup.Child = grid;
            popup.Placement = PlacementMode.Left;
            popup.StaysOpen = false;
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
                _allTasks.Add(task);
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
