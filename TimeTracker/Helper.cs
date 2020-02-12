using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TimeTracker
{
    static class Helper
    {

        /// ######################HELPER################################################
        public static StackPanel SearchVisualTree(DependencyObject targetElement, string controlName)
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

        public static Label SearchVisualTreeForLabel(DependencyObject targetElement, string controlName)
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

        public static List<StackPanel> SearchVisualTreeAndReturnList(DependencyObject targetElement, string controlName)
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
    }
}
