using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Drawing = System.Drawing;
using WinForms = System.Windows.Forms;
using WApp = System.Windows.Application;

namespace ZoteroLegend
{
    // ---------------- 配置模型 ----------------

    public class PointD
    {
        public double x;
        public double y;
    }

    public class ColorDef
    {
        public string Id;
        public int Order;
        public string Hex;
        public string ShortText;
        public string Question;
        public string CardSection;
        public bool Disabled;
    }

    public class Config
    {
        public double OffsetTop = 8;
        public double DotDiameter = 18;
        public double DotGap = 10;
        public double HoverScale = 1.2;
        public bool OnlyWhenZoteroForeground = true;
        public int PollIntervalMs = 200;
        public int TopmostRefreshMs = 3000;
        public List<ColorDef> Colors = new List<ColorDef>();
        public Dictionary<string, PointD> Positions = new Dictionary<string, PointD>();

        public static Config Defaults()
        {
            Config c = new Config();
            c.Colors.Add(Make("yellow", 1, "#ffd400", "主张", "这一段是不是作者自己的核心结论？", "§1 一句话结论 / §6 主张列", false));
            c.Colors.Add(Make("red", 2, "#ff6666", "可疑", "哪里不成立？没证明？有前提？还是夸大了？", "§7 局限 / §8 批判", false));
            c.Colors.Add(Make("green", 3, "#5fb236", "方法", "方法能不能被我复现、迁移或改造？", "§3 方法与证据链 / §5 可复用方法", false));
            c.Colors.Add(Make("blue", 4, "#2ea8e5", "背景", "这是定义、前提设定，还是别人的相关工作？", "§2 关键定义 / §4 研究空白", false));
            c.Colors.Add(Make("purple", 5, "#a28ae5", "可复用", "这段表达或工具，我能直接搬进自己的论文吗？", "§5 可复用方法 / §9 素材", false));
            c.Colors.Add(Make("magenta", 6, "#e56eee", "表述素材", "不参与高亮，只走「下划线」通道", "—", true));
            c.Colors.Add(Make("orange", 7, "#f19837", "数据", "这个数字来自哪张表哪次实验？", "§3 方法与证据链 / §6 数据列", false));
            c.Colors.Add(Make("gray", 8, "#aaaaaa", "待查", "这里我还没看懂，需要回头补或查证吗？", "§10 待查待办", false));
            return c;
        }

        private static ColorDef Make(string id, int order, string hex, string s, string q, string sec, bool disabled)
        {
            ColorDef d = new ColorDef();
            d.Id = id; d.Order = order; d.Hex = hex; d.ShortText = s;
            d.Question = q; d.CardSection = sec; d.Disabled = disabled;
            return d;
        }

        public static Config Load(string path)
        {
            string text = File.ReadAllText(path, Encoding.UTF8);
            // PowerShell's Set-Content -Encoding UTF8 writes a BOM, which the serializer rejects.
            text = text.TrimStart('\uFEFF');
            JavaScriptSerializer ser = new JavaScriptSerializer();
            Dictionary<string, object> root = Json.Dict(ser.DeserializeObject(text));

            Config c = new Config();

            Dictionary<string, object> bar = Json.Dict(Json.Get(root, "bar"));
            c.OffsetTop = Json.Num(bar, "offsetTop", 8);
            c.DotDiameter = Json.Num(bar, "dotDiameter", 18);
            c.DotGap = Json.Num(bar, "dotGap", 10);
            c.HoverScale = Json.Num(bar, "hoverScale", 1.2);
            Dictionary<string, object> pos = Json.Dict(Json.Get(bar, "positions"));
            foreach (KeyValuePair<string, object> kv in pos)
            {
                Dictionary<string, object> p = Json.Dict(kv.Value);
                PointD pt = new PointD();
                pt.x = Json.Num(p, "x", 0);
                pt.y = Json.Num(p, "y", 0);
                c.Positions[kv.Key] = pt;
            }

            Dictionary<string, object> beh = Json.Dict(Json.Get(root, "behavior"));
            c.OnlyWhenZoteroForeground = Json.Bool(beh, "onlyWhenZoteroForeground", true);
            c.PollIntervalMs = (int)Json.Num(beh, "pollIntervalMs", 200);
            c.TopmostRefreshMs = (int)Json.Num(beh, "topmostRefreshMs", 3000);

            List<object> arr = Json.Arr(Json.Get(root, "colors"));
            foreach (object o in arr)
            {
                Dictionary<string, object> d = Json.Dict(o);
                ColorDef cd = new ColorDef();
                cd.Id = Json.Str(d, "id", "?");
                cd.Order = (int)Json.Num(d, "order", c.Colors.Count + 1);
                cd.Hex = Json.Str(d, "hex", "#999999");
                cd.ShortText = Json.Str(d, "short", "?");
                cd.Question = Json.Str(d, "question", "");
                cd.CardSection = Json.Str(d, "cardSection", "");
                cd.Disabled = Json.Bool(d, "disabled", false);
                c.Colors.Add(cd);
            }
            if (c.Colors.Count == 0)
            {
                throw new InvalidDataException("colors 数组为空");
            }
            c.Colors.Sort(delegate (ColorDef a, ColorDef b) { return a.Order.CompareTo(b.Order); });
            return c;
        }

        public string ToJson()
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            StringBuilder sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"bar\": {\n");
            sb.Append("    \"anchor\": \"top-center\",\n");
            sb.Append("    \"offsetTop\": ").Append(Num(OffsetTop)).Append(",\n");
            sb.Append("    \"dotDiameter\": ").Append(Num(DotDiameter)).Append(",\n");
            sb.Append("    \"dotGap\": ").Append(Num(DotGap)).Append(",\n");
            sb.Append("    \"hoverScale\": ").Append(Num(HoverScale)).Append(",\n");
            sb.Append("    \"positions\": ");
            if (Positions.Count == 0)
            {
                sb.Append("{}");
            }
            else
            {
                sb.Append("{\n");
                bool first = true;
                foreach (KeyValuePair<string, PointD> kv in Positions)
                {
                    if (!first) sb.Append(",\n");
                    first = false;
                    sb.Append("      ").Append(ser.Serialize(kv.Key)).Append(": { \"x\": ")
                      .Append(Num(kv.Value.x)).Append(", \"y\": ").Append(Num(kv.Value.y)).Append(" }");
                }
                sb.Append("\n    }");
            }
            sb.Append("\n  },\n");
            sb.Append("  \"behavior\": {\n");
            sb.Append("    \"onlyWhenZoteroForeground\": ").Append(OnlyWhenZoteroForeground ? "true" : "false").Append(",\n");
            sb.Append("    \"pollIntervalMs\": ").Append(PollIntervalMs.ToString(CultureInfo.InvariantCulture)).Append(",\n");
            sb.Append("    \"topmostRefreshMs\": ").Append(TopmostRefreshMs.ToString(CultureInfo.InvariantCulture)).Append("\n");
            sb.Append("  },\n");
            sb.Append("  \"colors\": [\n");
            for (int i = 0; i < Colors.Count; i++)
            {
                ColorDef c = Colors[i];
                sb.Append("    {\n");
                sb.Append("      \"id\": ").Append(ser.Serialize(c.Id)).Append(",\n");
                sb.Append("      \"order\": ").Append(c.Order.ToString(CultureInfo.InvariantCulture)).Append(",\n");
                sb.Append("      \"hex\": ").Append(ser.Serialize(c.Hex)).Append(",\n");
                sb.Append("      \"short\": ").Append(ser.Serialize(c.ShortText)).Append(",\n");
                sb.Append("      \"question\": ").Append(ser.Serialize(c.Question)).Append(",\n");
                sb.Append("      \"cardSection\": ").Append(ser.Serialize(c.CardSection));
                if (c.Disabled)
                {
                    sb.Append(",\n      \"disabled\": true");
                }
                sb.Append("\n    }");
                if (i < Colors.Count - 1) sb.Append(",");
                sb.Append("\n");
            }
            sb.Append("  ]\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        private static string Num(double v)
        {
            return v.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }

    public static class Json
    {
        public static Dictionary<string, object> Dict(object o)
        {
            Dictionary<string, object> d = o as Dictionary<string, object>;
            return d == null ? new Dictionary<string, object>() : d;
        }

        public static object Get(Dictionary<string, object> d, string k)
        {
            object v;
            if (d != null && d.TryGetValue(k, out v)) return v;
            return null;
        }

        public static string Str(Dictionary<string, object> d, string k, string def)
        {
            object v = Get(d, k);
            if (v == null) return def;
            return Convert.ToString(v, CultureInfo.InvariantCulture);
        }

        public static double Num(Dictionary<string, object> d, string k, double def)
        {
            object v = Get(d, k);
            if (v == null) return def;
            try { return Convert.ToDouble(v, CultureInfo.InvariantCulture); }
            catch { return def; }
        }

        public static bool Bool(Dictionary<string, object> d, string k, bool def)
        {
            object v = Get(d, k);
            if (v == null) return def;
            if (v is bool) return (bool)v;
            string s = Convert.ToString(v, CultureInfo.InvariantCulture);
            return string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);
        }

        public static List<object> Arr(object o)
        {
            List<object> l = o as List<object>;
            if (l != null) return l;
            object[] a = o as object[];
            if (a != null) return new List<object>(a);
            return new List<object>();
        }
    }

    // ---------------- Win32 ----------------

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct MONITORINFOEX
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public int dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;
    }

    public static class Native
    {
        public const uint MONITOR_DEFAULTTONEAREST = 2;
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOACTIVATE = 0x0010;

        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetMonitorInfoW")]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX mi);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT r);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT p);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int idx);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int idx, IntPtr val);

        [DllImport("shcore.dll")]
        public static extern int GetDpiForMonitor(IntPtr hmon, int dpiType, out uint dpiX, out uint dpiY);

        [DllImport("shcore.dll")]
        public static extern int GetProcessDpiAwareness(IntPtr hProcess, out int value);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int index);
    }

    public class PosInfo
    {
        public RECT rc;
        public string device;
        public double scale;
    }

    // ---------------- 主体 ----------------

    public class LegendApp
    {
        private Config cfg;
        private string cfgPath;

        private Window win;
        private IntPtr hwnd;
        private Grid rootGrid;
        private StackPanel stack;
        private StackPanel dotsPanel;
        private Border bubble;
        private TextBlock bubbleText;
        private Border card;
        private Ellipse cardDot;
        private TextBlock cardTitle;
        private TextBlock cardQuestion;
        private TextBlock cardSection;
        private List<Ellipse> dots = new List<Ellipse>();
        private ColorDef cardColor;

        private DispatcherTimer poll;
        private bool visible;
        private bool forceHidden;
        private bool firstShow = true;

        private List<uint> zoteroPids = new List<uint>();
        private DateTime lastPidRefresh = DateTime.MinValue;
        private IntPtr zoteroHwnd = IntPtr.Zero;
        private DateTime lastTopmost = DateTime.MinValue;
        private string currentDevice = "";

        private bool dragging;
        private bool moved;
        private Ellipse dragSource;
        private POINT startCursor;
        private RECT startRect;

        private WinForms.NotifyIcon tray;
        private uint myPid;

        private int targetX;
        private int targetY;
        private int expectedW;
        private int expectedH;
        private bool revealed;
        private DateTime shownAt;
        private int settledW;
        private string loggedDpiDevice = "";
        private double systemScale = 1;
        private const double NominalWidth = 340;
        private DispatcherTimer settle;
        private bool alwaysVisible;

        public void Start()
        {
            myPid = (uint)Process.GetCurrentProcess().Id;
            // Undocumented test switch: keep the bar up regardless of the foreground app.
            string[] argv = Environment.GetCommandLineArgs();
            for (int i = 0; i < argv.Length; i++)
            {
                if (string.Compare(argv[i], "--always", true) == 0) alwaysVisible = true;
            }
            cfgPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "colors.json");
            try { File.Delete(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ZoteroLegend.log")); } catch { }
            LogMsg("start pid=" + myPid + " cfg=" + cfgPath);
            LoadConfigSafe();
            LogMsg("config loaded: colors=" + cfg.Colors.Count + " d=" + cfg.DotDiameter + " gap=" + cfg.DotGap);
            int awareness;
            int ahr = Native.GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, out awareness);
            int sysDpi = Native.GetDeviceCaps(Native.GetDC(IntPtr.Zero), 88);
            if (sysDpi > 0) systemScale = sysDpi / 96.0;
            LogMsg("dpi awareness hr=" + ahr + " value=" + awareness + " sysDpi=" + sysDpi + " systemScale=" + systemScale);

            BuildWindow();
            BuildTray();

            poll = new DispatcherTimer(DispatcherPriority.Background);
            poll.Interval = TimeSpan.FromMilliseconds(Math.Max(100, cfg.PollIntervalMs));
            poll.Tick += new EventHandler(PollTick);
            poll.Start();
        }

        private void LoadConfigSafe()
        {
            try
            {
                cfg = Config.Load(cfgPath);
            }
            catch (Exception ex)
            {
                Log(ex);
                cfg = Config.Defaults();
                if (tray != null)
                {
                    tray.ShowBalloonTip(6000, "Zotero 图例条", "colors.json 读取失败，已用内置配色：" + ex.Message, WinForms.ToolTipIcon.Warning);
                }
            }
        }

        // ---------- 界面 ----------

        private void BuildWindow()
        {
            win = new Window();
            win.Title = "ZoteroLegend";
            win.WindowStyle = WindowStyle.None;
            win.AllowsTransparency = true;
            win.Background = null;
            win.Topmost = true;
            win.ShowInTaskbar = false;
            win.ShowActivated = false;
            win.ResizeMode = ResizeMode.NoResize;
            win.SizeToContent = SizeToContent.WidthAndHeight;
            win.Left = -20000;
            win.Top = -20000;
            win.SourceInitialized += new EventHandler(OnSourceInitialized);

            Grid grid = new Grid();
            grid.Width = NominalWidth;
            rootGrid = grid;
            win.Content = grid;

            stack = new StackPanel();
            stack.HorizontalAlignment = HorizontalAlignment.Center;
            stack.VerticalAlignment = VerticalAlignment.Top;
            grid.Children.Add(stack);

            dotsPanel = new StackPanel();
            dotsPanel.Orientation = Orientation.Horizontal;
            dotsPanel.HorizontalAlignment = HorizontalAlignment.Center;
            dotsPanel.Margin = new Thickness(0, 0, 0, 0);
            dotsPanel.Background = null;
            stack.Children.Add(dotsPanel);

            bubbleText = new TextBlock();
            bubbleText.Foreground = Brushes.White;
            bubbleText.FontFamily = UiFont();
            bubbleText.FontSize = 12;
            bubble = new Border();
            bubble.Background = BrushFrom("#E6242424");
            bubble.CornerRadius = new CornerRadius(6);
            bubble.Padding = new Thickness(9, 3, 9, 4);
            bubble.Margin = new Thickness(0, 7, 0, 0);
            bubble.HorizontalAlignment = HorizontalAlignment.Center;
            bubble.Visibility = Visibility.Collapsed;
            bubble.Child = bubbleText;
            stack.Children.Add(bubble);

            cardDot = new Ellipse();
            cardDot.Width = 10;
            cardDot.Height = 10;
            cardDot.Margin = new Thickness(0, 0, 7, 0);
            cardDot.VerticalAlignment = VerticalAlignment.Center;

            cardTitle = new TextBlock();
            cardTitle.Foreground = Brushes.White;
            cardTitle.FontFamily = UiFont();
            cardTitle.FontSize = 15;
            cardTitle.FontWeight = FontWeights.Bold;

            StackPanel head = new StackPanel();
            head.Orientation = Orientation.Horizontal;
            head.Children.Add(cardDot);
            head.Children.Add(cardTitle);

            cardQuestion = new TextBlock();
            cardQuestion.Foreground = BrushFrom("#DEDEDE");
            cardQuestion.FontFamily = UiFont();
            cardQuestion.FontSize = 12.5;
            cardQuestion.TextWrapping = TextWrapping.Wrap;
            cardQuestion.Margin = new Thickness(0, 5, 0, 0);

            cardSection = new TextBlock();
            cardSection.Foreground = BrushFrom("#9A9A9A");
            cardSection.FontFamily = UiFont();
            cardSection.FontSize = 11.5;
            cardSection.TextWrapping = TextWrapping.Wrap;
            cardSection.Margin = new Thickness(0, 6, 0, 0);

            StackPanel body = new StackPanel();
            body.Children.Add(head);
            body.Children.Add(cardQuestion);
            body.Children.Add(cardSection);

            card = new Border();
            card.Background = BrushFrom("#F01E1E1E");
            card.CornerRadius = new CornerRadius(8);
            card.Padding = new Thickness(13, 10, 13, 11);
            card.Margin = new Thickness(0, 8, 0, 0);
            card.HorizontalAlignment = HorizontalAlignment.Center;
            card.MaxWidth = 320;
            card.Visibility = Visibility.Collapsed;
            card.Child = body;
            stack.Children.Add(card);

            win.LayoutUpdated += new EventHandler(OnLayoutUpdated);

            BuildDots();
        }

        private void BuildDots()
        {
            dotsPanel.Children.Clear();
            dots.Clear();
            double d = cfg.DotDiameter;
            double gap = cfg.DotGap;
            foreach (ColorDef c in cfg.Colors)
            {
                Ellipse e = new Ellipse();
                e.Width = d;
                e.Height = d;
                e.Margin = new Thickness(gap / 2, 0, gap / 2, 0);
                e.RenderTransformOrigin = new Point(0.5, 0.5);
                e.RenderTransform = new ScaleTransform(1, 1);
                e.Cursor = Cursors.Hand;
                e.Tag = c;
                Color col = ParseColor(c.Hex);
                if (c.Disabled)
                {
                    SolidColorBrush fill = new SolidColorBrush(Color.FromArgb(46, col.R, col.G, col.B));
                    fill.Freeze();
                    e.Fill = fill;
                    SolidColorBrush st = new SolidColorBrush(col);
                    st.Freeze();
                    e.Stroke = st;
                    e.StrokeThickness = 1.4;
                    e.StrokeDashArray = new DoubleCollection(new double[] { 2.2, 2.2 });
                    e.Opacity = 0.9;
                }
                else
                {
                    SolidColorBrush fill = new SolidColorBrush(col);
                    fill.Freeze();
                    e.Fill = fill;
                }
                e.MouseEnter += new MouseEventHandler(DotEnter);
                e.MouseLeave += new MouseEventHandler(DotLeave);
                e.MouseLeftButtonDown += new MouseButtonEventHandler(DotDown);
                e.MouseMove += new MouseEventHandler(DotMove);
                e.MouseLeftButtonUp += new MouseButtonEventHandler(DotUp);
                dotsPanel.Children.Add(e);
                dots.Add(e);
            }
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            hwnd = new WindowInteropHelper(win).Handle;
            IntPtr ex = Native.GetWindowLongPtr(hwnd, Native.GWL_EXSTYLE);
            long v = ex.ToInt64() | Native.WS_EX_TOOLWINDOW | Native.WS_EX_NOACTIVATE;
            Native.SetWindowLongPtr(hwnd, Native.GWL_EXSTYLE, new IntPtr(v));
        }

        // ---------- 悬停 / 卡片 ----------

        private void DotEnter(object sender, MouseEventArgs e)
        {
            Ellipse el = (Ellipse)sender;
            ColorDef c = (ColorDef)el.Tag;
            ScaleTransform st = el.RenderTransform as ScaleTransform;
            if (st != null) { st.ScaleX = cfg.HoverScale; st.ScaleY = cfg.HoverScale; }
            bubbleText.Text = c.ShortText;
            bubble.Visibility = Visibility.Visible;
        }

        private void DotLeave(object sender, MouseEventArgs e)
        {
            Ellipse el = (Ellipse)sender;
            ScaleTransform st = el.RenderTransform as ScaleTransform;
            if (st != null) { st.ScaleX = 1; st.ScaleY = 1; }
            if (!dragging)
            {
                bubble.Visibility = Visibility.Collapsed;
            }
        }

        private void ToggleCard(Ellipse el)
        {
            ColorDef c = (ColorDef)el.Tag;
            if (card.Visibility == Visibility.Visible && cardColor == c)
            {
                card.Visibility = Visibility.Collapsed;
                cardColor = null;
                return;
            }
            cardColor = c;
            cardDot.Fill = el.Fill;
            cardTitle.Text = c.ShortText;
            cardQuestion.Text = c.Question;
            cardSection.Text = c.Disabled ? "停用色：只在下划线通道使用" : "落卡：" + c.CardSection;
            card.Visibility = Visibility.Visible;
        }

        private void HideCard()
        {
            card.Visibility = Visibility.Collapsed;
            cardColor = null;
        }

        private void HideBubble()
        {
            bubble.Visibility = Visibility.Collapsed;
        }

        // ---------- 拖拽 ----------

        private void DotDown(object sender, MouseButtonEventArgs e)
        {
            if (hwnd == IntPtr.Zero) return;
            Ellipse el = (Ellipse)sender;
            dragging = true;
            moved = false;
            dragSource = el;
            Native.GetCursorPos(out startCursor);
            RECT r;
            Native.GetWindowRect(hwnd, out r);
            startRect = r;
            el.CaptureMouse();
            e.Handled = true;
        }

        private void DotMove(object sender, MouseEventArgs e)
        {
            if (!dragging) return;
            POINT p;
            Native.GetCursorPos(out p);
            int dx = p.X - startCursor.X;
            int dy = p.Y - startCursor.Y;
            if (!moved && (Math.Abs(dx) > 4 || Math.Abs(dy) > 4)) moved = true;
            if (moved)
            {
                Native.SetWindowPos(hwnd, Native.HWND_TOPMOST,
                    startRect.Left + dx, startRect.Top + dy, 0, 0,
                    Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
            }
        }

        private void DotUp(object sender, MouseButtonEventArgs e)
        {
            if (!dragging) return;
            dragging = false;
            Ellipse el = (Ellipse)sender;
            el.ReleaseMouseCapture();
            if (moved)
            {
                SavePosition();
            }
            else
            {
                ToggleCard(el);
            }
            e.Handled = true;
        }

        private void SavePosition()
        {
            PosInfo pi = MonitorOf(zoteroHwnd);
            RECT r;
            Native.GetWindowRect(hwnd, out r);
            PointD pt = new PointD();
            pt.x = Math.Round((r.Left - pi.rc.Left) / pi.scale);
            pt.y = Math.Round((r.Top - pi.rc.Top) / pi.scale);
            cfg.Positions[pi.device] = pt;
            SaveConfig();
        }

        private void SaveConfig()
        {
            try
            {
                File.WriteAllText(cfgPath, cfg.ToJson(), new UTF8Encoding(true));
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }

        // ---------- 轮询 ----------

        private void PollTick(object sender, EventArgs e)
        {
            try
            {
                if ((DateTime.Now - lastPidRefresh).TotalSeconds >= 2)
                {
                    RefreshZotero();
                }

                IntPtr fg = Native.GetForegroundWindow();
                uint pid;
                Native.GetWindowThreadProcessId(fg, out pid);
                bool zot = zoteroPids.Contains(pid);
                bool ours = (pid == myPid) || (fg == hwnd);

                if (zot && fg != IntPtr.Zero)
                {
                    zoteroHwnd = fg;
                }

                bool want = (cfg.OnlyWhenZoteroForeground && !alwaysVisible) ? (zot || (ours && visible)) : true;
                if (forceHidden) want = false;
                if (want != visible)
                {
                    LogMsg("visible=" + want + " fgpid=" + pid + " zot=" + zot + " ours=" + ours);
                    SetVisible(want);
                }
                if (!visible) return;

                PosInfo pi = MonitorOf(zoteroHwnd);
                if (pi.device != currentDevice)
                {
                    currentDevice = pi.device;
                    if (!dragging) PlaceWindow(pi, false);
                }

                if ((DateTime.Now - lastTopmost).TotalMilliseconds > cfg.TopmostRefreshMs)
                {
                    Native.SetWindowPos(hwnd, Native.HWND_TOPMOST, 0, 0, 0, 0,
                        Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
                    lastTopmost = DateTime.Now;
                }

                if (!dragging) CloseWhenCursorOutside();

                // WPF re-lays-out the window a beat after it lands on a monitor with a
                // different DPI; re-apply the target position until it sticks.
                if (!dragging && visible)
                {
                    RECT cur;
                    if (Native.GetWindowRect(hwnd, out cur))
                    {
                        int cw = cur.Right - cur.Left;
                        if (Math.Abs(cur.Left - targetX) > 1 || Math.Abs(cur.Top - targetY) > 1 ||
                            Math.Abs(cw - expectedW) > 1)
                        {
                            PlaceWindow(MonitorOf(zoteroHwnd), true);
                        }

                        if (!revealed)
                        {
                            double age = (DateTime.Now - shownAt).TotalMilliseconds;
                            if (Math.Abs(cw - settledW) <= 2 || age > 700)
                            {
                                revealed = true;
                                win.Opacity = 1;
                                LogMsg("revealed w=" + cw + " settled=" + settledW + " age=" + (int)age);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }

        private void CloseWhenCursorOutside()
        {
            if (card.Visibility != Visibility.Visible && bubble.Visibility != Visibility.Visible) return;
            POINT p;
            if (!Native.GetCursorPos(out p)) return;
            RECT r;
            if (!Native.GetWindowRect(hwnd, out r)) return;
            bool inside = p.X >= r.Left - 2 && p.X <= r.Right + 2 && p.Y >= r.Top - 2 && p.Y <= r.Bottom + 60;
            if (!inside)
            {
                HideBubble();
                HideCard();
            }
        }

        private void RefreshZotero()
        {
            lastPidRefresh = DateTime.Now;
            zoteroPids.Clear();
            IntPtr main = IntPtr.Zero;
            try
            {
                foreach (Process p in Process.GetProcessesByName("zotero"))
                {
                    zoteroPids.Add((uint)p.Id);
                    if (p.MainWindowHandle != IntPtr.Zero) main = p.MainWindowHandle;
                }
            }
            catch (Exception ex)
            {
                Log(ex);
            }
            if (main != IntPtr.Zero) zoteroHwnd = main;
        }

        private void SetVisible(bool v)
        {
            if (v == visible) return;
            visible = v;
            if (v)
            {
                HideBubble();
                HideCard();
                // WPF builds the window at the system DPI and only later switches to the
                // target monitor's DPI, which briefly doubles the dots. Stay invisible
                // until the size has settled.
                if (!revealed)
                {
                    win.Opacity = 0;
                    shownAt = DateTime.Now;
                }
                win.Show();
                win.UpdateLayout();
                if (firstShow)
                {
                    firstShow = false;
                    win.Left = -20000;
                    win.Top = -20000;
                }
                PosInfo pi = MonitorOf(zoteroHwnd);
                currentDevice = pi.device;
                PlaceWindow(pi, true);
                lastTopmost = DateTime.Now;
                if (settle == null)
                {
                    settle = new DispatcherTimer(DispatcherPriority.Background);
                    settle.Interval = TimeSpan.FromMilliseconds(90);
                    settle.Tick += new EventHandler(delegate (object s2, EventArgs e2)
                    {
                        settle.Stop();
                        if (visible && !dragging) PlaceWindow(MonitorOf(zoteroHwnd), true);
                    });
                }
                settle.Start();
            }
            else
            {
                HideBubble();
                HideCard();
                win.Hide();
            }
        }

        // Check what scale WPF is actually rendering at. WPF starts at the system DPI and
        // switches after it receives the per-monitor DPI change; log any mismatch so the
        // compensation below can be reintroduced if a real machine needs it.
        private void ApplyDpiScale(PosInfo pi)
        {
            rootGrid.LayoutTransform = Transform.Identity;
            win.UpdateLayout();
            RECT r;
            if (!Native.GetWindowRect(hwnd, out r)) return;
            double w = r.Right - r.Left;
            double used = w / NominalWidth;
            if (Math.Abs(used - pi.scale) > 0.05)
            {
                LogMsg("dpi mismatch dev=" + pi.device + " monitor=" + pi.scale + " wpf=" + used + " w=" + w);
            }
        }

        private void PlaceWindow(PosInfo pi, bool useSaved)
        {
            if (hwnd == IntPtr.Zero) return;
            ApplyDpiScale(pi);
            win.UpdateLayout();
            RECT wr;
            Native.GetWindowRect(hwnd, out wr);
            int ww = wr.Right - wr.Left;
            int wh = wr.Bottom - wr.Top;
            int mw = pi.rc.Right - pi.rc.Left;
            int mh = pi.rc.Bottom - pi.rc.Top;
            int x = 0;
            int y = 0;
            bool placed = false;
            PointD saved;
            if (useSaved && cfg.Positions.TryGetValue(pi.device, out saved))
            {
                x = pi.rc.Left + (int)Math.Round(saved.x * pi.scale);
                y = pi.rc.Top + (int)Math.Round(saved.y * pi.scale);
                if (x >= pi.rc.Left && x + ww <= pi.rc.Right && y >= pi.rc.Top && y + wh <= pi.rc.Bottom)
                {
                    placed = true;
                }
            }
            if (!placed)
            {
                x = pi.rc.Left + (mw - ww) / 2;
                y = pi.rc.Top + (int)Math.Round(cfg.OffsetTop * pi.scale);
            }
            targetX = x;
            targetY = y;
            expectedW = ww;
            settledW = (int)Math.Round(NominalWidth * pi.scale);
            expectedH = wh;
            Native.SetWindowPos(hwnd, Native.HWND_TOPMOST, x, y, 0, 0,
                Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
            RECT after;
            Native.GetWindowRect(hwnd, out after);
            LogMsg("place dev=" + pi.device + " mon=" + pi.rc.Left + "," + pi.rc.Top + "," + pi.rc.Right + "," + pi.rc.Bottom +
                   " scale=" + pi.scale + " size=" + ww + "x" + wh + " want=" + x + "," + y +
                   " got=" + after.Left + "," + after.Top + "," + after.Right + "," + after.Bottom);
        }

        private PosInfo MonitorOf(IntPtr target)
        {
            PosInfo pi = new PosInfo();
            IntPtr mon = Native.MonitorFromWindow(target == IntPtr.Zero ? hwnd : target, Native.MONITOR_DEFAULTTONEAREST);
            MONITORINFOEX mi = new MONITORINFOEX();
            mi.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
            if (!Native.GetMonitorInfo(mon, ref mi))
            {
                mi.rcMonitor.Left = 0;
                mi.rcMonitor.Top = 0;
                mi.rcMonitor.Right = 1920;
                mi.rcMonitor.Bottom = 1080;
                mi.szDevice = "\\\\.\\DISPLAY1";
            }
            pi.rc = mi.rcMonitor;
            pi.device = mi.szDevice == null ? "" : mi.szDevice;
            uint dx;
            uint dy;
            int hr = Native.GetDpiForMonitor(mon, 0, out dx, out dy);
            pi.scale = (hr == 0 && dx > 0) ? dx / 96.0 : 1.0;
            if (hr != 0 && pi.device != loggedDpiDevice)
            {
                loggedDpiDevice = pi.device;
                LogMsg("GetDpiForMonitor FAILED hr=" + hr + " dev=" + pi.device + " fallback scale=" + pi.scale);
            }
            return pi;
        }

        // ---------- 托盘 ----------

        private void BuildTray()
        {
            try
            {
                tray = new WinForms.NotifyIcon();
                tray.Icon = CreateTrayIcon();
                tray.Text = "Zotero 高亮图例条";
                tray.Visible = true;

                WinForms.ContextMenuStrip menu = new WinForms.ContextMenuStrip();
                menu.Items.Add("显示图例", null, new EventHandler(delegate (object s, EventArgs e) { forceHidden = false; }));
                menu.Items.Add("隐藏图例", null, new EventHandler(delegate (object s, EventArgs e) { forceHidden = true; SetVisible(false); }));
                menu.Items.Add(new WinForms.ToolStripSeparator());
                menu.Items.Add("重新载入配置", null, new EventHandler(delegate (object s, EventArgs e) { ReloadConfig(); }));
                menu.Items.Add("打开配置文件夹", null, new EventHandler(delegate (object s, EventArgs e) { OpenConfigFolder(); }));
                menu.Items.Add(new WinForms.ToolStripSeparator());
                menu.Items.Add("退出", null, new EventHandler(delegate (object s, EventArgs e) { Shutdown(); }));
                tray.ContextMenuStrip = menu;

                tray.DoubleClick += new EventHandler(delegate (object s, EventArgs e)
                {
                    forceHidden = !forceHidden;
                    if (forceHidden) SetVisible(false);
                });
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }

        private void OpenConfigFolder()
        {
            try
            {
                Process.Start("explorer.exe", "/select,\"" + cfgPath + "\"");
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }

        private void ReloadConfig()
        {
            LoadConfigSafe();
            HideBubble();
            HideCard();
            BuildDots();
            if (visible)
            {
                win.UpdateLayout();
                PlaceWindow(MonitorOf(zoteroHwnd), true);
            }
        }

        private Drawing.Icon CreateTrayIcon()
        {
            Drawing.Bitmap bmp = new Drawing.Bitmap(16, 16);
            using (Drawing.Graphics g = Drawing.Graphics.FromImage(bmp))
            {
                g.Clear(Drawing.Color.Transparent);
                Drawing.Color[] cols = new Drawing.Color[]
                {
                    Drawing.Color.FromArgb(255, 255, 212, 0),
                    Drawing.Color.FromArgb(255, 255, 102, 102),
                    Drawing.Color.FromArgb(255, 95, 178, 54),
                    Drawing.Color.FromArgb(255, 46, 168, 229),
                    Drawing.Color.FromArgb(255, 162, 138, 229),
                    Drawing.Color.FromArgb(255, 229, 110, 238),
                    Drawing.Color.FromArgb(255, 241, 152, 55),
                    Drawing.Color.FromArgb(255, 170, 170, 170)
                };
                using (Drawing.Brush b = new Drawing.SolidBrush(Drawing.Color.FromArgb(255, 250, 250, 250)))
                {
                    g.FillEllipse(b, 0, 0, 16, 16);
                }
                for (int i = 0; i < 8; i++)
                {
                    int cx = 2 + (i % 4) * 3;
                    int cy = 3 + (i / 4) * 7;
                    using (Drawing.Brush b = new Drawing.SolidBrush(cols[i]))
                    {
                        g.FillEllipse(b, cx, cy, 5, 5);
                    }
                }
            }
            IntPtr h = bmp.GetHicon();
            return Drawing.Icon.FromHandle(h);
        }

        private void Shutdown()
        {
            try
            {
                poll.Stop();
            }
            catch { }
            try
            {
                if (tray != null)
                {
                    tray.Visible = false;
                    tray.Dispose();
                    tray = null;
                }
            }
            catch { }
            try
            {
                win.Close();
            }
            catch { }
            WApp.Current.Shutdown();
        }

        // ---------- 工具 ----------

        private static FontFamily UiFont()
        {
            return new FontFamily("Microsoft YaHei UI, Microsoft YaHei, Segoe UI");
        }

        private static SolidColorBrush BrushFrom(string hex)
        {
            SolidColorBrush b = new SolidColorBrush(ParseColor(hex));
            b.Freeze();
            return b;
        }

        private static Color ParseColor(string hex)
        {
            if (hex == null) return Colors.Gray;
            string h = hex.Trim();
            if (h.StartsWith("#")) h = h.Substring(1);
            if (h.Length == 3)
            {
                h = "" + h[0] + h[0] + h[1] + h[1] + h[2] + h[2];
            }
            if (h.Length == 4)
            {
                h = "" + h[0] + h[0] + h[1] + h[1] + h[2] + h[2] + h[3] + h[3];
            }
            if (h.Length != 6 && h.Length != 8) return Colors.Gray;
            byte a = 255;
            int offset = 0;
            if (h.Length == 8)
            {
                a = Convert.ToByte(h.Substring(0, 2), 16);
                offset = 2;
            }
            int r = Convert.ToInt32(h.Substring(offset, 2), 16);
            int g = Convert.ToInt32(h.Substring(offset + 2, 2), 16);
            int b = Convert.ToInt32(h.Substring(offset + 4, 2), 16);
            return Color.FromArgb(a, (byte)r, (byte)g, (byte)b);
        }

        private static void Log(Exception ex)
        {
            LogMsg("EX " + ex.ToString());
        }

        private static void LogMsg(string m)
        {
            try
            {
                string p = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ZoteroLegend.log");
                File.AppendAllText(p, DateTime.Now.ToString("HH:mm:ss.fff") + "  " + m + Environment.NewLine, Encoding.UTF8);
            }
            catch { }
        }
    }

    public static class Program
    {
        private static Mutex mutex;

        [STAThread]
        public static void Main()
        {
            bool created;
            mutex = new Mutex(true, "ZoteroLegend.SingleInstance", out created);
            if (!created) return;

            WApp app = new WApp();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            LegendApp legend = new LegendApp();
            legend.Start();
            app.Run();
        }
    }
}
