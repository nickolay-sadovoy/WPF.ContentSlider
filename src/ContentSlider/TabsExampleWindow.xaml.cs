using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ContentSlider
{
    public partial class TabsExampleWindow : Window
    {
        public TabsExampleWindow()
        {
            InitializeComponent();
            this.Tabs.SelectionChanged += this.OnSelectionChanged;
        }

        int page;

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabs = sender as TabControl;

            var panelWidth = ((Double)Resources["PanelWidth"]);
            var sb = ((Storyboard)Resources["SwitchPage"]);
            sb.Stop();
            ((DoubleAnimation)sb.Children[0]).From = -(panelWidth * (page - 1));
            ((DoubleAnimation)sb.Children[0]).To = -(panelWidth * (tabs.SelectedIndex));
            ((DoubleAnimation)sb.Children[1]).From = -(panelWidth * (page - 2));
            ((DoubleAnimation)sb.Children[1]).To = -(panelWidth * (tabs.SelectedIndex - 1));
            ((DoubleAnimation)sb.Children[2]).From = -(panelWidth * (page - 3));
            ((DoubleAnimation)sb.Children[2]).To = -(panelWidth * (tabs.SelectedIndex - 2));
            if (page != 0) sb.Begin();
            page = tabs.SelectedIndex + 1;
        }
    }
}
