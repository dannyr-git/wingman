using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using wingman.ViewModels;

namespace wingman.Views
{
    public sealed partial class Footer : Page
    {


        public Footer()
        {
            InitializeComponent();
            ViewModel = Ioc.Default.GetRequiredService<FooterViewModel>();
            DataContext = ViewModel;
        }

        public FooterViewModel ViewModel { get; }


    }

    public static class TextBoxBehavior
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(TextBoxBehavior), new PropertyMetadata(false, OnAutoScrollChanged));

        public static bool GetAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollProperty, value);
        }

        private static void OnAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox && e.NewValue is bool autoScroll)
            {
                if (autoScroll)
                {
                    textBox.TextChanged += OnTextChanged;
                    ScrollToBottom(textBox);
                }
                else
                {
                    textBox.TextChanged -= OnTextChanged;
                }
            }
        }

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                ScrollToBottom(textBox);
            }
        }

        private static void ScrollToBottom(TextBox textBox)
        {
            var grid = (Grid)VisualTreeHelper.GetChild(textBox, 0);
            if (grid != null)
            {
                for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
                {
                    object obj = VisualTreeHelper.GetChild(grid, i);
                    if (!(obj is ScrollViewer)) continue;
                    ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f);
                    break;
                }
            }
        }
    }

    public static class ScrollViewerBehavior
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(ScrollViewerBehavior), new PropertyMetadata(false, OnAutoScrollChanged));

        public static bool GetAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollProperty, value);
        }

        private static void OnAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer && e.NewValue is bool autoScroll)
            {
                if (autoScroll)
                {
                    scrollViewer.ViewChanged += OnViewChanged;
                    ScrollToBottom(scrollViewer);
                }
                else
                {
                    scrollViewer.ViewChanged -= OnViewChanged;
                }
            }
        }

        private static void OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer && !e.IsIntermediate)
            {
                ScrollToBottom(scrollViewer);
            }
        }

        private static void ScrollToBottom(ScrollViewer scrollViewer)
        {
            scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null);
        }
    }
}

