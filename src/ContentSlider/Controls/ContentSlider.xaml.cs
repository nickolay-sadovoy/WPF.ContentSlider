using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
            new UIPropertyMetadata(default(int), OnPageIndexChanged));

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
            set { SetValue(PageIndexProperty, value); }
        }

        public SliderEx()
        {
            InitializeComponent();
        }

        private static void OnContentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SliderEx control && control.Contents != null && control.Contents.Any())
            {
                var newControl = control.CreateControl(visibility: Visibility.Visible);
                newControl.Content = control.Contents.First();
                control.currentContent = newControl;
                control.CurrentContentGrid.Children.Add(newControl);
            }
        }

        private ContentControl CreateControl(HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center, VerticalAlignment verticalAlignment = VerticalAlignment.Center, Visibility visibility = Visibility.Collapsed)
        {
            var newContentControl = new ContentControl() { HorizontalAlignment = horizontalAlignment, VerticalAlignment = verticalAlignment, Visibility = visibility};
            return newContentControl;
        }

        private void CreateSwipe(int currentPageIndex, int newPageIndex)
        {
            var newControl = CreateControl();
            newControl.Content = Contents[newPageIndex];

            NewContentGrid.Children.Add(newControl);

            var storyboardName = "LeftSwipe12";

            if(currentPageIndex > newPageIndex)
            {
                storyboardName = isUseFistContentGrid ? "RightSwipe12" : "RightSwipe21";
            }
            else if(!isUseFistContentGrid)
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
                CurrentContentGrid.Children.Remove(removeElement);
                isUseFistContentGrid = !isUseFistContentGrid;
            };
            s.Completed += handler;

            newControl.Visibility = Visibility.Visible;

            s.Begin();
        }

        private Grid CurrentContentGrid => isUseFistContentGrid ? this.FirsContentGrid : this.SecondContentGrid;

        private Grid NewContentGrid => isUseFistContentGrid ? this.SecondContentGrid : this.FirsContentGrid;


    }
}
