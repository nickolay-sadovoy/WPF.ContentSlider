using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ContentSlider
{
    public partial class SliderEx : UserControl
    {
        private bool isUseFistContentGrid = true;
        private FrameworkElement currentContent;

        public static readonly DependencyProperty ContensProperty = DependencyProperty.Register(
            "StackPanels",
            typeof(StackPanel[]),
            typeof(SliderEx),
            new UIPropertyMetadata(default(StackPanel[]), OnContentsChanged));

        public static readonly DependencyProperty PageIndexProperty = DependencyProperty.Register(
            "PageIndex",
            typeof(int),
            typeof(SliderEx),
            new UIPropertyMetadata(default(int), OnPageIndexChanged));

        private static void OnPageIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SliderEx control)
            {
                control.CreateSwipe((int)e.OldValue, (int)e.NewValue);
            }
        }

        public StackPanel[] StackPanels
        {
            get { return (StackPanel[])this.GetValue(ContensProperty); }
            set { SetValue(ContensProperty, value); }
        }

        public int PageIndex
        {
            get { return (int)this.GetValue(PageIndexProperty); }
            set
            {
                if (value < 0)
                    value = this.StackPanels.Length-1;
                else if (value >= this.StackPanels.Length)
                    value = 0;

                SetValue(PageIndexProperty, value);
            }
        }
       
        public SliderEx()
        {
            InitializeComponent();

            this.KeyDown += OnSliderExKeyDown;
        }

        private void OnSliderExKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
                this.PageIndex--;
            else if (e.Key == Key.Right)
                this.PageIndex++;
        }

        private static void OnContentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SliderEx control && control.StackPanels != null && control.StackPanels.Any())
            {
                var newControl = control.CreateControl(visibility: Visibility.Visible);
                var stackPanel = newControl.Content = control.StackPanels.First();
                control.currentContent = newControl;

                control.TryReadAutomationProperties((StackPanel)stackPanel);

                control.NewContentGrid.Children.Add(newControl);
            }
        }

        private ContentControl CreateControl(HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center, VerticalAlignment verticalAlignment = VerticalAlignment.Center, Visibility visibility = Visibility.Collapsed)
        {
            var newContentControl = new ContentControl() { HorizontalAlignment = horizontalAlignment, VerticalAlignment = verticalAlignment, Visibility = visibility, Focusable = false };
            return newContentControl;
        }

        private void CreateSwipe(int currentPageIndex, int newPageIndex)
        {
            var newControl = CreateControl();
            newControl.Content = StackPanels[newPageIndex];

            NewContentGrid.Children.Add(newControl);

            var storyboardName = SliderSettings.PreviewContentFrom1To2StoryBoard;

            if (currentPageIndex > newPageIndex)
                storyboardName = isUseFistContentGrid ? SliderSettings.NextContentFrom1To2StoryBoard : SliderSettings.NextContentFrom2To1StoryBoard;
            else if (!isUseFistContentGrid)
                storyboardName = SliderSettings.PreviewContentFrom2To1StoryBoard;

            var s = ((Storyboard)this.Resources[storyboardName]);

            EventHandler handler = null;
            handler = (sndr, evtArgs) =>
            {
                s.Completed -= handler;
                var removeElement = currentContent;
                currentContent = newControl;
                CurrentContentGrid.Children.Remove(removeElement);
                isUseFistContentGrid = !isUseFistContentGrid;
                TryReadAutomationProperties(StackPanels[newPageIndex]);

            };
            s.Completed += handler;

            newControl.Visibility = Visibility.Visible;

            s.Begin();
        }

        private void TryReadAutomationProperties(StackPanel stackPanel)
        {
            var textblocks = stackPanel?.Children.OfType<TextBlock>();

            if (textblocks != null)
            {
                AutomationPeer peer = null;

                foreach (TextBlock textBlock in textblocks)
                {
                    peer = TextBlockAutomationPeer.FromElement(textBlock);
                    peer?.RaiseAutomationEvent(AutomationEvents.MenuOpened);
                }
            }
        }


        private Grid CurrentContentGrid => isUseFistContentGrid ? this.FirstContentGrid : this.SecondContentGrid;

        private Grid NewContentGrid => isUseFistContentGrid ? this.SecondContentGrid : this.FirstContentGrid;

        private void CopyAutopmationProperties(FrameworkElement sourceFrameworkElement, FrameworkElement targetFrameworkElement)
        {
            AutomationProperties.SetName(targetFrameworkElement, AutomationProperties.GetName(sourceFrameworkElement));
            AutomationProperties.SetHelpText(targetFrameworkElement, AutomationProperties.GetHelpText(sourceFrameworkElement));
        }
    }


    public static class SliderSettings
    {
        public static string PreviewContentFrom1To2StoryBoard => "LeftSwipe12";
        public static string PreviewContentFrom2To1StoryBoard => "LeftSwipe21";
        public static string NextContentFrom1To2StoryBoard => "RightSwipe12";
        public static string NextContentFrom2To1StoryBoard => "RightSwipe21";
    }
}
