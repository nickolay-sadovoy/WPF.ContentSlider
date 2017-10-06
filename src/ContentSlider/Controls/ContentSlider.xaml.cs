using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ContentSlider
{
    /// <summary>
    /// Interaction logic for ContentSlider.xaml
    /// </summary>
    public partial class SliderEx : UserControl
    {
        private bool isUseFistContentGrid = true;
        private FrameworkElement currentContent;

        public static readonly DependencyProperty ContensProperty = DependencyProperty.Register(
            "Contents",
            typeof(FrameworkElement[]),
            typeof(SliderEx),
            new UIPropertyMetadata(default(FrameworkElement[]), OnContentsChanged));

        public static readonly DependencyProperty PageIndexProperty = DependencyProperty.Register(
            "PageIndex",
            typeof(int),
            typeof(SliderEx),
            new UIPropertyMetadata(1, OnPageIndexChanged));

        private static void OnPageIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SliderEx control)
            {
                control.CreateSwipe((int)e.OldValue, (int)e.NewValue);
            }
        }

        public FrameworkElement[] Contents
        {
            get { return (FrameworkElement[])this.GetValue(ContensProperty); }
            set { SetValue(ContensProperty, value); }
        }

        public int PageIndex
        {
            get { return (int)this.GetValue(PageIndexProperty); }
            set
            {
                if (value < 0)
                    value = this.Contents.Length-1;
                else if (value >= this.Contents.Length)
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
            if (d is SliderEx control && control.Contents != null && control.Contents.Any())
            {
                var newControl = control.CreateControl(visibility: Visibility.Visible);
                newControl.Content = control.Contents.First();
                control.currentContent = newControl;
                control.CopyAutopmationProperties(newControl, control.CurrentContentGrid);


                control.CurrentContentGrid.Content = newControl;
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
            newControl.Content = Contents[newPageIndex];

            CopyAutopmationProperties(Contents[newPageIndex], NewContentGrid);
            CurrentContentGrid.Focusable = false;
            NewContentGrid.Focusable = true;
            NewContentGrid.Focus();

            NewContentGrid.Content = newControl;
            NewContentGrid.Visibility = Visibility.Visible;

            var storyboardName = "LeftSwipe12";

            if (currentPageIndex > newPageIndex)
            {
                storyboardName = isUseFistContentGrid ? "RightSwipe12" : "RightSwipe21";
            }
            else if (!isUseFistContentGrid)
            {
                storyboardName = "LeftSwipe21";
            }

            var s = ((Storyboard)this.Resources[storyboardName]);

            EventHandler handler = null;
            handler = (sndr, evtArgs) =>
            {
                s.Completed -= handler;
                var removeElement = currentContent;
                currentContent = newControl;
                CurrentContentGrid.ClearValue(ContentProperty);
                CurrentContentGrid.Visibility = Visibility.Collapsed;
                isUseFistContentGrid = !isUseFistContentGrid;
            };
            s.Completed += handler;

            newControl.Visibility = Visibility.Visible;

            s.Begin();
        }

        private TabItem CurrentContentGrid => isUseFistContentGrid ? this.FirstContentGrid : this.SecondContentGrid;

        private TabItem NewContentGrid => isUseFistContentGrid ? this.SecondContentGrid : this.FirstContentGrid;

        private void CopyAutopmationProperties(FrameworkElement sourceFrameworkElement, FrameworkElement targetFrameworkElement)
        {
            AutomationProperties.SetName(targetFrameworkElement, AutomationProperties.GetName(sourceFrameworkElement));
            AutomationProperties.SetHelpText(targetFrameworkElement, AutomationProperties.GetHelpText(sourceFrameworkElement));
        }

    }
}
