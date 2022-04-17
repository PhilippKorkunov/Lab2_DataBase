using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace DataBase
{
    public partial class MainWindow : Window
    {
        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }
            return (IntPtr)0;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                mmi.ptMinTrackSize.x = 950;
                mmi.ptMinTrackSize.y = 675;
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {

            public int x;

            public int y;

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public static readonly RECT Empty = new RECT();
            public int Width { get { return Math.Abs(right - left); } }
            public int Height { get { return bottom - top; } }
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
            public RECT(RECT rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }
            public bool IsEmpty { get { return left >= right || top >= bottom; } }
            public override string ToString()
            {
                if (this == Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }

            public override int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();

            public static bool operator ==(RECT rect1, RECT rect2) { return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom); }

            public static bool operator !=(RECT rect1, RECT rect2) { return !(rect1 == rect2); }

        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);


        public MainWindow()
        {

            InitializeComponent();
            if (Sheet.Exists()) { Sheet.Parse(); }
            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };
            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            MaximizeButton.Click += (s, e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            CloseButton.Click += (s, e) => Close();
            DownloadButton.Click += (s, e) => Download();
            SaveButton.Click += (s, e) => Save();
            PathButton.Click += (s, e) => PathSelect();
            UpdateButton.Click += (s,e) => Update();
        }


        int currentPageIndex = 0;
        int itemPerPage = 20;
        int totalPage = 0;
        int itemCount = Sheet.rowCount;
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Sheet.ReadPath();
            Sheet.Parse();
            Refresh();
        }

        private void Download()
        {
            Sheet.Update();
            Refresh();
        }

        private void PathSelect() 
        {
            Sheet.SelectPath();
            Sheet.Parse();
            Refresh();
        }

        private void Update()
        {
            var prevThreatDict = Sheet.threatDict;
            Sheet.Update();
            var updateList = Sheet.ChangeCheck(prevThreatDict, Sheet.threatDict);
            foreach(var list in updateList)
            {
                if (list[0] != null && list[1] != null)
                {
                    var info_prev  = new FullInfo(0, 0, " (было)");
                    info_prev.threat = list[0];
                    info_prev.Show();

                    var info_new = new FullInfo(0, 0, " (стало)");
                    info_new.threat = list[0];
                    info_new.Show();
                }
                if (list[0] != null && list[1] == null)
                {
                    var info_prev = new FullInfo(0, 0, " (удалено)");
                    info_prev.threat = list[0];
                    info_prev.Show();
                }
                if (list[0] == null && list[1] != null)
                {
                    var info_prev = new FullInfo(0, 0, " (новая)");
                    info_prev.threat = list[1];
                    info_prev.Show();
                }
            }


            Refresh();
        }
       
        private void Save()
        {
            bool flag = Sheet.TrySave();
            if(!flag) { MessageBox.Show("Файл не найден! Попробуйте сначала либо, указать путь к файлу, если он уже скачен, либо загрузить его."); }
        }

        private void Refresh()
        {
            itemCount = Sheet.rowCount;
            totalPage = (itemCount) / itemPerPage;
            if (itemCount % itemPerPage != 0) totalPage += 1;
            if (itemCount > 0)
            {
                int maxLength = 0;
                UIB.Items.Clear();
                for (int i = itemPerPage * currentPageIndex; i < itemPerPage * currentPageIndex + itemPerPage; i++)
                {
                    if (i == Sheet.rowCount) { break; }
                    UIB.Items.Add(Sheet.threatDict.Values.ToList()[i]);
                    maxLength = Math.Max(maxLength, Sheet.threatDict.Keys.ToList()[i].Length);
                }
                PageNo.Text = $"Страница № {currentPageIndex + 1}";
                UIB.RowHeight = 18 * RowHeightCounter(maxLength);
            }
        }

        private int RowHeightCounter(int maxLength) => maxLength / 61 + 1;

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement((DataGrid)sender,
                                        e.OriginalSource as DependencyObject) as DataGridRow;

            if (row == null) return;
            else 
            {
                var info = new FullInfo(row.GetIndex(), currentPageIndex, "");
                info.threat = null;
                info.Show();
            }
        }
    

        private void First_Click(object sender, RoutedEventArgs e)
        {
            if (currentPageIndex != 0)
            {
                currentPageIndex = 0;
                Refresh();
            }
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                Refresh();
            }
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (currentPageIndex < totalPage - 1)
            {
                currentPageIndex++;
                Refresh();
            }
        }

        private void Last_Click(object sender, RoutedEventArgs e)
        {
            if (currentPageIndex != totalPage - 1)
            {
                currentPageIndex = totalPage - 1;
                Refresh();
            }
        }


    }
}