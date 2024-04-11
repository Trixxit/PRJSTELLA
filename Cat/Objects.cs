﻿using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Cat
{
    /// <summary>
    /// Contains classes for creating UI objects like shutdown screens and speech bubbles.
    /// </summary>
    internal static class Objects
    {
        /// <summary>
        /// OLD shutdown screen test that didn't really work too well. For later, I suppose.
        /// </summary>
        /// <remarks>
        /// The windows shutdown circle is an accessible font, use it!
        /// </remarks>
        [Obsolete("Incomplete, discontinued / on hold until further notice", true)]
        internal class ShutDownScreen : Canvas
        {
            private static ShutDownScreen inst;

            /// <summary>
            /// Toggles the shutdown screen on or off.
            /// </summary>
            /// <param name="canv">The canvas to add or remove the shutdown screen from.</param>
            /// <returns>The instance of the shutdown screen.</returns>
            internal static ShutDownScreen ToggleScreen(Canvas canv)
            { if (inst != null) { canv.Children.Remove(inst); inst = null; return inst; } else { inst = new ShutDownScreen(); canv.Children.Add(inst); return inst; } }

            private ShutDownScreen()
            {
                inst = this;
                Children.Add(new System.Windows.Shapes.Rectangle() { Width = SystemParameters.PrimaryScreenWidth, Height = SystemParameters.PrimaryScreenHeight });
                SetTop<double>(this, 0);
                SetLeft<double>(this, 0);
            }
        }

        /// <summary>
        /// Creates an overlay for the current screen at the user's set opacity but set grey colour.
        /// </summary>
        /// <remarks>
        /// Need to merge this with the interface's back ground for continuity
        /// </remarks>
        internal static class OverlayRect
        {
            /// <summary>
            /// The rectangle itself, only created once.
            /// </summary>
            private static readonly Rectangle Rectangle = new Rectangle { Width = Catowo.GetScreen().Bounds.Width, Height = Catowo.GetScreen().Bounds.Height, Fill = new SolidColorBrush(Colors.Gray), Opacity = UserData.Opacity };

            /// <summary>
            /// Adds <see cref="Rectangle"/> to <paramref name="c"/>
            /// </summary>
            /// <param name="c">The canvas to add to</param>
            internal static void AddToCanvas(Canvas c)
            {
                c.Children.Add(Rectangle);
                UpdateRect();
            }

            /// <summary>
            /// Updates the rectangle to the user's data and for screen flexibility
            /// </summary>
            private static void UpdateRect()
            {
                Rectangle.Opacity = UserData.Opacity;
                Rectangle.Width = Catowo.GetScreen().Bounds.Width;
                Rectangle.Height = Catowo.GetScreen().Bounds.Height;
            }

            /// <summary>
            /// Does the opposite of <see cref="AddToCanvas(Canvas)"/>
            /// </summary>
            /// <param name="c">The canvas in which to remove from</param>
            internal static void RemoveFromCanvas(Canvas c)
                => c.Children.Remove(Rectangle);
        }

        /// <summary>
        /// Static class for dealing with direct Clara interactions, such as speech bubbles and images / animations (if we can get it)
        /// </summary>
        /// <remarks>
        /// Need to have back progression (using left arrow)
        /// </remarks>
        internal static class ClaraHerself
        {
            /// <summary>
            /// Used with the speech bubble arrays to keep track of which array item we're on.
            /// </summary>
            private static byte num = 0;

            /// <summary>
            /// Holds the introduction text
            /// </summary>
            private static readonly string[] Introduction = [
                "Hey! It's me, Clara! \nIt seems this is the first time you've opened me (or I've been updated owo).\nIf you want to skip this, please type 'skip'. \nIf you want to view the changelog, type 'changelog'\nIf you want to run through the introduction, just press the right arrow key!",
                "So you wanna do the introduction again... sweet!\nI'm Clara, the Centralised, Logistical, Administrative and Requisition Assistant. \nMy sole purpose is to automate, optimize and otherwise improve your computer experience.\n You can press the left arrow key to move through parts",
                "There are two (at the moment) main modes to this program: Background and Interface.\nInterface is where there's an overlay with a textbox and an output box, where you can enter commands.\n   Key shortcuts won't work here, but this is where most of the functionality is.\nBackground is where there... is no main overlay (you're currently in background mode!).\n   This is what the app will be in 99% of the time.",
                "To open the interface:\n  Hold both shifts (both th left and right one),\n  Then press and hold Q,\n  then press I!\n  (LShift + RShift + Q + I). \n To close the interface run the 'close' command.\nTo view the help page, run 'help'",
                "Hmmm.. is there anything else..?\nOh right! Local data is stored at C:\\ProgramData\\Kitty\\Cat\\\nHave fun, I hope you enjoy this app! o/"
            ];

            /// <summary>
            /// Whichever array is currently being spoken
            /// </summary>
            private static string[] CurrentStory = [];

            /// <summary>
            /// The bubble object being shown
            /// </summary>
            private static SpeechBubble? bubble;

            /// <summary>
            /// The canvas object we're attached to
            /// </summary>
            private static Canvas? canvas;

            /// <summary>
            /// However lonely you feel, you're never alone. There are literally millions of bugs, mites and bacteria living in your house. Goodnight.
            /// </summary>
            /// <param name="parameters"></param>
            /// <returns></returns>
            private delegate object? UniversalDelegate();

            private static UniversalDelegate delegation = null;//(args) => { _ = "Lorum Ipsum"; return null; };
            
            /// <summary>
            /// Needs to reflect what arrays we have
            /// </summary>
            internal enum Mode : byte
            {
                Introduction
            }

            /// <summary>
            /// Cancer cures smoking.
            /// </summary>
            /// <param name="mode">Which array to run through</param>
            /// <param name="canvas">The canvas</param>
            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            internal static void RunClara(Mode mode, Canvas canvas)
            {
                ClaraHerself.canvas = canvas;
                OverlayRect.AddToCanvas(canvas);
                Catowo.inst.MakeNormalWindow();
                switch (mode)
                {
                    case Mode.Introduction:
                        CurrentStory = Introduction;
                        break;
                }
                // The first message
                bubble = new();
                Point location = new(Catowo.inst.Width - 30, Catowo.inst.Height - 30);
                Logging.LogP("Location", location);
                bubble.LowerRightCornerFreeze = location;
                bubble.Text = CurrentStory[num];
                canvas.Children.Add(bubble);
                Catowo.inst.PreviewKeyDown += ProgressionKeydown;

            }

            /// <summary>
            /// Event handler for the pressing of keys while a speech is activated. This is the base method to be used in tangent with specific methods for each set of speech, and is to be loading in and out as needed.
            /// </summary>
            /// <param name="sender">The caller of the event</param>
            /// <param name="e">The key event args</param>
            private static void ProgressionKeydown(object sender, System.Windows.Input.KeyEventArgs e)
            {
                if (e.Key == Key.Right)
                    if (canvas != null) {
                        if (++num > CurrentStory.Length - 1)
                        {
                            num = 0;
                            Catowo.inst.MakeFunnyWindow();
                            Catowo.inst.PreviewKeyDown -= ProgressionKeydown;
                            OverlayRect.RemoveFromCanvas(canvas);
                            if (bubble != null)
                            {
                                canvas.Children.Remove(bubble);
                                bubble = null;
                            }
                            return;
                        }
                        if (bubble != null)
                            bubble.Text = CurrentStory[num];
                        return;
                    }
                if (e.Key == Key.Left)
                    if (canvas != null) 
                    {
                        num--;
                        if (num >= 0)
                            if (bubble != null)
                                bubble.Text = CurrentStory[num];
                        return;
                    }
                if (e.Key == Key.Up)
                    if (canvas != null)
                    {
                        num = 0;
                        Catowo.inst.MakeFunnyWindow();
                        Catowo.inst.PreviewKeyDown -= ProgressionKeydown;
                        OverlayRect.RemoveFromCanvas(canvas);
                        if (bubble != null)
                        {
                            canvas.Children.Remove(bubble);
                            bubble = null;
                        }
                        return;
                    }
                if (e.Key == Key.Down)
                    if (delegation != null)
                        delegation();

            }

            /// <summary>
            /// Represents a speech bubble UI element.
            /// </summary>
            private class SpeechBubble : Canvas
            {
                /// <summary>
                /// The text displayed
                /// </summary>
                private readonly TextBlock textBlock;
                /// <summary>
                /// The bubble part
                /// </summary>
                private readonly Rectangle rectangle;
                /// <summary>
                /// The arrow part that really completes the bubble
                /// </summary>
                private readonly Polygon tail;
                /// <summary>
                /// Controls padding
                /// </summary>
                private const float Control = 5.0F;
                /// <summary>
                /// Fixed position for the lower right corner
                /// </summary>
                internal Point LowerRightCornerFreeze = new(1000, 500);

                /// <summary>
                /// Abstraction Property
                /// </summary>
                public string Text
                {
                    get => textBlock.Text;
                    set {
                        textBlock.Text = value;
                        UpdateLayout();
                    }
                }

                /// <summary>
                /// Abstraction Property
                /// </summary>
                public double FontSize
                {
                    get => textBlock.FontSize;
                    set => textBlock.FontSize = value;
                }

                /// <summary>
                /// Abstraction Property
                /// </summary>
                public double BubbleOpacity
                {
                    get => Opacity;
                    set => Opacity = value;
                }

                /// <summary>
                /// Abstraction Property
                /// </summary>
                public Brush BubbleColor
                {
                    get => rectangle.Fill;
                    set
                    {
                        rectangle.Fill = value;
                        tail.Fill = value;
                    }
                }

                /// <summary>
                /// Abstraction Property
                /// </summary>
                public Thickness TextPadding
                {
                    get => textBlock.Margin;
                    set => textBlock.Margin = value;
                }

                /// <summary>
                /// Constructor
                /// </summary>
                public SpeechBubble()
                {
                    rectangle = new()
                    {
                        RadiusX = 10,
                        RadiusY = 10,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Fill = new SolidColorBrush(Colors.White),
                    };

                    tail = new Polygon
                    {
                        Points = new PointCollection(new[] { new Point(0, 0), new Point(15, 0), new Point(7.5, 20) }),
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Fill = new SolidColorBrush(Colors.White)
                    };

                    textBlock = new TextBlock
                    {
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(Control)
                    };

                    TextPadding = new Thickness(Control);
                    Children.Add(rectangle);
                    Children.Add(tail);
                    Children.Add(textBlock);
                    FontSize = 20.0f;//UserData.FontSize;

                    SizeChanged += (s, e) => UpdateLayout();
                }

                public void UpdateLayout()
                {
                    textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

                    double textWidth = textBlock.DesiredSize.Width + TextPadding.Left + TextPadding.Right;
                    double textHeight = textBlock.DesiredSize.Height + TextPadding.Top + TextPadding.Bottom;

                    rectangle.Width = textWidth + (Control * 2);
                    rectangle.Height = textHeight + (Control * 2) + 20;
                    SetLeft<double>(textBlock, TextPadding.Left + Control);
                    SetTop<double>(textBlock, TextPadding.Top + Control);
                    SetLeft<double>(tail, rectangle.Width - 10); 
                    SetTop<double>(tail, rectangle.Height);
                    double left = LowerRightCornerFreeze.X - rectangle.Width;
                    double top = LowerRightCornerFreeze.Y - rectangle.Height;
                    SetLeft<double>(this, left);
                    SetTop<double>(this, top);
                    Width = rectangle.Width;
                    Height = rectangle.Height;
                }
            }

        }

        internal static class CursorEffects
        {
            private static bool isOn = false;
            private static DispatcherTimer timer;
            private static Window allencompassing;

            internal static void Toggle()
            {
                if (!isOn) Run();
                else if (isOn) Stop();
                isOn = !isOn;
            }

            private static void Run()
            {
                if (isOn) return;
                int left = Screen.AllScreens.Min(screen => screen.Bounds.Left);
                int top = Screen.AllScreens.Min(screen => screen.Bounds.Top);
                int right = Screen.AllScreens.Max(screen => screen.Bounds.Right);
                int bottom = Screen.AllScreens.Max(screen => screen.Bounds.Bottom);
                int width = right - left;
                int height = bottom - top;

                allencompassing = new Window
                {
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = left,
                    Top = top,
                    Width = width,
                    Height = height,
                    Background = Brushes.Transparent
                };
                allencompassing.Show();
                allencompassing.Loaded += (sender, e) =>
                {
                    var hwnd = new WindowInteropHelper(allencompassing).Handle;
                    var originalStyle = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
                    SetWindowLongWrapper(hwnd, GWL_EXSTYLE, originalStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
                    var editedstyle = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
                    Logging.Log($"Set Win Style of Handle {hwnd} from {originalStyle:X} ({originalStyle:B}) [{originalStyle}] to {editedstyle:X} ({editedstyle:B}) [{editedstyle}]");
                };
                timer = new() { Interval = TimeSpan.FromSeconds(0.5) };
                timer.Tick += (s, e) =>
                {
                    GetCursorPosWrapper(out POINT mouseposition);
                };
                isOn = true;
            }

            private static void Stop()
            {
                if (!isOn) return;
            }
        }

        internal readonly record struct Command(string Call, string Raw, object[][]? Parameters = null);
    }
}
