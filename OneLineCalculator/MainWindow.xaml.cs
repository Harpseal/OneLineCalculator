using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OneLineCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class CalcItem : INotifyPropertyChanged
    {
        public CalcItem()
        {
            mCalcText = "";
            mCalcResult = 0.0;
            mCalcTextResult = mCalcResult.ToString();
        }

        public CalcItem(CalcItem item)
        {
            mCalcText = item.mCalcText;
            mCalcResult = item.mCalcResult;
            mCalcTextResult = item.mCalcTextResult;
        }
        public CalcItem(string calcText, double result)
        {
            mCalcText = calcText;
            mCalcResult = result;
            mCalcTextResult = mCalcResult.ToString();
        }

        public string mCalcText;
        public string mCalcTextResult;
        public double mCalcResult;
        public string CalcText
        {
            get { return mCalcText; }
            set
            {
                if (mCalcText != value)
                {
                    mCalcText = value;
                    NotifyPropertyChanged("CalcText");
                }
            }
        }

        public double CalcResult
        {
            get { return CalcResult; }
            set
            {
                if (mCalcResult != value)
                {
                    mCalcResult = value;
                    mCalcTextResult = value.ToString();
                    NotifyPropertyChanged("CalcResult");
                }
            }
        }

        public string FullText
        {
            get { return mCalcText + " = " + mCalcResult; }
        }


        #region INotifyPropertyChanged Members

        /// Need to implement this interface in order to get data binding
        /// to work properly.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public partial class MainWindow : Window
    {

        private DynamicExpresso.Interpreter mInterpreter = new DynamicExpresso.Interpreter();

        public List<CalcItem> mCalcList { get; set; }
        public CalcItem mCalcCur { get; set; }
        //public CalcItem mCalcCur = new CalcItem("test",0.123);

        #region ComboBox mouse over event
        DependencyPropertyDescriptor dpd;
        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBoxItem cmb = sender as ComboBoxItem;
            dpd = DependencyPropertyDescriptor
                .FromProperty(IsMouseOverProperty, typeof(ComboBoxItem));
            if (dpd != null)
                dpd.AddValueChanged(cmb, OnIsMouseOver);

        }

        private void ComboBox_Unloaded(object sender, RoutedEventArgs e)
        {
            ComboBoxItem cmb = sender as ComboBoxItem;
            if (dpd != null)
                dpd.RemoveValueChanged(cmb, OnIsMouseOver);

        }
        private void OnIsMouseOver(object sender, EventArgs e)
        {
            ComboBoxItem cmb = sender as ComboBoxItem;
            if (cmb.IsMouseOver)
            {
                CalcItem item = cmb.DataContext as CalcItem;
                if (item != null)
                {
                    //Console.WriteLine(item.FullText);
                    mTextResult.Text = item.mCalcTextResult;

                    Size size = MeasureString(mTextResult.Text);
                    mTextResult.Width = size.Width;
                }
                //do something...
            }
        }
        #endregion


        #region GlobalKeyboardHook
        //https://stackoverflow.com/questions/604410/global-keyboard-capture-in-c-sharp-application
        private GlobalKeyboardHook mGlobalKeyboardHook;

        private bool mIsKeyAltDown = false;
        private bool mIsKeyCtrlDown = false;
        private bool mIsKeyShiftDown = false;
        private bool mIsKeyWinDown = false;
        private int mHitState = 0;
        private void OnGlobalKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            Console.WriteLine(e.KeyboardData.VirtualCode  + "(" + KeyInterop.KeyFromVirtualKey(e.KeyboardData.VirtualCode).ToString() + 
                ")  state:" + e.KeyboardState + "  flag:" + e.KeyboardData.Flags.ToString("X4"));

            Key key = KeyInterop.KeyFromVirtualKey(e.KeyboardData.VirtualCode);

            bool isKeyDown = (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown || e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyDown);
            bool isKeyUp = (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp || e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyUp);

            bool isFlag0, isFlag1;
            Key key0 = CvtStringToKey(Properties.Settings.Default.GlobalHotkey0, out isFlag0);
            Key key1 = CvtStringToKey(Properties.Settings.Default.GlobalHotkey1, out isFlag1);

            bool isHit = true;

            if (isFlag0)
            {
                if (key0 == Key.LWin || key0 == Key.RWin)
                    isHit &= mIsKeyWinDown;
                if (key0 == Key.LeftCtrl || key0 == Key.RightCtrl)
                    isHit &= mIsKeyCtrlDown;
                if (key0 == Key.LeftAlt || key0 == Key.RightAlt)
                    isHit &= mIsKeyAltDown;
                if (key0 == Key.LeftShift || key0 == Key.RightShift)
                    isHit &= mIsKeyShiftDown;

                isHit &= (key1 == key) && isKeyDown;

                if (isHit && mHitState == 0)
                    mHitState = 2;
            }
            else
            {
                isHit &= (key0 == key) && isKeyUp;
                if (isHit)
                    mHitState = 2;
            }

            if (key == Key.LWin || key == Key.RWin)
            {
                if (mIsKeyWinDown && mHitState == 1 && isKeyUp)
                    mHitState = 2;
                mIsKeyWinDown = isKeyDown;
            }
            if (key == Key.LeftCtrl || key == Key.RightCtrl)
            {
                if (mIsKeyCtrlDown && mHitState == 1 && isKeyUp)
                    mHitState = 2;
                mIsKeyCtrlDown = isKeyDown;
            }
            if (key == Key.LeftAlt || key == Key.RightAlt)
            {
                if (mIsKeyAltDown && mHitState == 1 && isKeyUp)
                    mHitState = 2;
                mIsKeyAltDown = isKeyDown;
            }
            if (key == Key.LeftShift || key == Key.RightShift)
                mIsKeyShiftDown = isKeyDown;


            Console.WriteLine("Hit " + mHitState + "(" + mIsKeyAltDown + ") keyup " + isKeyUp + "  "  + key0 + "+" + key1 + "  Cur " + key + "  alt:" + mIsKeyAltDown + " ctrl:" + mIsKeyCtrlDown + " shf:" + mIsKeyShiftDown + " win:" + mIsKeyWinDown);

            if (mHitState == 2)
            {

                new Thread(()=> {
                    if (Properties.Settings.Default.GlobalDelay > 0)
                        Thread.Sleep(Properties.Settings.Default.GlobalDelay);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ShowCalc();
                    }));
                   
                }).Start();
                
                e.Handled = true;
                mHitState = 0;
            }

            //if (e.KeyboardData.VirtualCode != GlobalKeyboardHook.VkSnapshot)
            //    return;

            // seems, not needed in the life.
            //if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyDown &&
            //    e.KeyboardData.Flags == GlobalKeyboardHook.LlkhfAltdown)
            //{
            //    MessageBox.Show("Alt + Print Screen");
            //    e.Handled = true;
            //}
            //else

            //if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
            //{
            //    MessageBox.Show("Print Screen");
            //    e.Handled = true;
            //}
        }

        public void Dispose()
        {
            mGlobalKeyboardHook?.Dispose();
        }

        #endregion


        #region MainWindow callback
        System.Windows.Forms.NotifyIcon mNotifyIcon = null;
        public MainWindow()
        {
            mGlobalKeyboardHook = new GlobalKeyboardHook();
            mGlobalKeyboardHook.KeyboardPressed += OnGlobalKeyPressed;

            InitializeComponent();

            mCalcCur = null;
            mCalcList = new List<CalcItem>();
            mComboBoxCalc.ItemsSource = mCalcList;


            double res;
            //res = RegexFormulas.Eval("(99)+(-23-((3+101b)/(0xF0)+-0x123+(0+1+2-3/4*5%6pow7pow8--++----9)))");

            //res = RegexFormulas.Eval("-1+1<<4");
            //res = RegexFormulas.Eval("-(1+2)");
            //res = RegexFormulas.Eval("100^165");
            //res = RegexFormulas.Eval("int(1111pow11111)");

            //RegexFormulas.Eval("0+1+2-3/4*5%6pow7pow8--++----9", out res);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess();
            var procList = System.Diagnostics.Process.GetProcesses().Where(p =>
                             p.ProcessName == proc.ProcessName && p.Id != proc.Id);
            //proc.Proc
            if (procList.Count() >= 1)
            {
                //foreach (var p in procList)
                //{
                //    SetWindowPos(p.MainWindowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                //    //ShowWindow(p.MainWindowHandle, 1);
                //    //SetFocus(new System.Runtime.InteropServices.HandleRef(null, p.MainWindowHandle));
                //    //SetForegroundWindow(p.MainWindowHandle);
                //}
                MessageBox.Show(Application.Current.MainWindow.GetType().Assembly.GetName().Name+"\nAlready an instance is running...", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                App.Current.Shutdown();
                //this.Close();
                return;
            }

            TextBlock text = mTextResult;
            if (text != null)
            {
                //Copy the settings of note text box.
                if (mNoteTypeFace == null)
                {
                    mNoteTypeFace = new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch);
                    mNoteFontSize = text.FontSize;
                }
                else
                {
                    text.FontFamily = mNoteTypeFace.FontFamily;
                    text.FontStyle = mNoteTypeFace.Style;
                    text.FontWeight = mNoteTypeFace.Weight;
                    text.FontStretch = mNoteTypeFace.Stretch;
                    text.FontSize = mNoteFontSize;
                }
            }

            mNotifyIcon = new System.Windows.Forms.NotifyIcon();

            System.IO.Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/OneLineCalculator;component/Resource/Icons/baseline_add_box_black_36dp.ico")).Stream;
            mNotifyIcon.Icon = new System.Drawing.Icon(iconStream);

            mNotifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu();

            System.Windows.Forms.MenuItem menuItem;

            menuItem = new System.Windows.Forms.MenuItem();
            menuItem.Index = -1;
            menuItem.Text = "Show";
            menuItem.Click += new System.EventHandler((s, args) => { this.ShowCalc(); });
            mNotifyIcon.ContextMenu.MenuItems.Add(menuItem);

            menuItem = new System.Windows.Forms.MenuItem();
            menuItem.Index = -1;
            menuItem.Text = "Show(center)";
            menuItem.Click += new System.EventHandler((s, args) => {
                Properties.Settings.Default.WindowPositionX = Double.NaN;
                Properties.Settings.Default.WindowPositionY = Double.NaN;
                this.ShowCalc();
            });
            mNotifyIcon.ContextMenu.MenuItems.Add(menuItem);

            menuItem = new System.Windows.Forms.MenuItem();
            menuItem.Index = -1;
            menuItem.Text = "Close";
            menuItem.Click += new System.EventHandler((s, args) => { this.Close(); });
            mNotifyIcon.ContextMenu.MenuItems.Add(menuItem);

            mNotifyIcon.DoubleClick +=
                 delegate (object s, EventArgs args)
                 {
                     this.ShowCalc();
                 };

            mNotifyIcon.Visible = true;
            this.ShowInTaskbar = false;

            ShowCalc();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (mNotifyIcon != null)
            {
                mNotifyIcon.Dispose();
                mNotifyIcon = null;
            }
        }
        #endregion

        public class MoveToForeground
        {
            [System.Runtime.InteropServices.DllImportAttribute("User32.dll")]
            private static extern int FindWindow(String ClassName, String WindowName);

            const int SWP_NOMOVE = 0x0002;
            const int SWP_NOSIZE = 0x0001;
            const int SWP_SHOWWINDOW = 0x0040;
            const int SWP_NOACTIVATE = 0x0010;
            [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetWindowPos")]
            public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

            public static void DoOnProcess(string processName)
            {
                var allProcs = System.Diagnostics.Process.GetProcessesByName(processName);
                if (allProcs.Length > 0)
                {
                    System.Diagnostics.Process proc = allProcs[0];
                    int hWnd = FindWindow(null, proc.MainWindowTitle.ToString());
                    // Change behavior by settings the wFlags params. See http://msdn.microsoft.com/en-us/library/ms633545(VS.85).aspx
                    SetWindowPos(new IntPtr(hWnd), 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW | SWP_NOACTIVATE);
                }
            }

            public static void DoOnProcess(IntPtr hWnd)
            {
                // Change behavior by settings the wFlags params. See http://msdn.microsoft.com/en-us/library/ms633545(VS.85).aspx
                SetWindowPos(hWnd, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW | SWP_NOACTIVATE);
            }
        }

        private void ShowCalc()
        {
            System.Drawing.Rectangle rect = System.Windows.Forms.SystemInformation.VirtualScreen;

            double posx, posy;
            try
            {
                posx = Properties.Settings.Default.WindowPositionX;
            }
            catch (System.NullReferenceException)
            {
                posx = Double.NaN;
            }

            try
            {
                posy = Properties.Settings.Default.WindowPositionY;
            }
            catch (System.NullReferenceException)
            {
                posy = Double.NaN;
            }

            if (Double.IsNaN(posx) && Double.IsNaN(posy))
            {
                this.Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.X + System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2;
                this.Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y + System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 2 - this.Height / 2;
            }
            else if (!Double.IsNaN(posx) && !Double.IsNaN(posy))
            {
                this.Left = posx;
                this.Top = posy;
            }
            else
            {
                System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
                this.Left = point.X - this.Width / 2;
                this.Top = point.Y - this.Height / 2;
            }

            if (this.Left < rect.X)
                this.Left = rect.X;
            else if (this.Left + this.Width > rect.X + rect.Width)
                this.Left = rect.X + rect.Width - this.Width;


            if (this.Top < rect.Y)
                this.Top = rect.Y;
            else if (this.Top + this.Height > rect.Y + rect.Height)
                this.Top = rect.Y + rect.Height - this.Height;

#if true
            if (MenuItemShowMode.IsChecked == true)
            {
                //https://stackoverflow.com/questions/257587/bring-a-window-to-the-front-in-wpf
                if (!this.IsVisible)
                {
                    this.Show();
                }

                if (this.WindowState == WindowState.Minimized)
                {
                    this.WindowState = WindowState.Normal;
                }

                this.Activate();
                this.Topmost = true;  // important
                this.Topmost = false; // important
                this.Focus();         // important
            }
            else
            {
                this.Activate();
            }
#else
            this.Activate();
#endif
            mComboBoxCalc.Focus();
        }


        private Typeface mNoteTypeFace = null;
        private double mNoteFontSize = 20;
        private Size MeasureString(string candidate)
        {
            if (mNoteTypeFace == null)
                return new Size(0, 0);

            var formattedText = new FormattedText(
                candidate,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                mNoteTypeFace,
                mNoteFontSize,
                Brushes.Black,
                new NumberSubstitution(),
                System.Windows.Media.TextFormattingMode.Ideal);

            return new Size(formattedText.Width, formattedText.Height);
        }


        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

#region Drag window
        private System.Windows.Point m_MousePosition;
        public DateTime m_MouseDownTime;
        public bool m_bIsMouseDown = false;

        private void OnButtonMove_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Control uiCur = (sender as Control);
            if (uiCur == null) return;
            m_MousePosition = e.GetPosition(uiCur);
            m_MouseDownTime = DateTime.Now;
            m_bIsMouseDown = true;
        }

        private void OnButtonMove_MouseMove(object sender, MouseEventArgs e)
        {
            MainWindow window = this;

            Control uiCur = (sender as Control);
            if (uiCur == null) return;
            double minDragDis = 10;// Math.Min(Math.Min(uiCur.ActualWidth, uiCur.ActualHeight) * 0.25, 15);
            var currentPoint = e.GetPosition(uiCur);

            if (e.LeftButton == MouseButtonState.Pressed
                &&
                //uiCur.IsMouseCaptured &&
                (Math.Abs(currentPoint.X - m_MousePosition.X) > minDragDis ||
                Math.Abs(currentPoint.Y - m_MousePosition.Y) > minDragDis))
            {
                // Prevent Click from firing
                uiCur.ReleaseMouseCapture();
                m_bIsMouseDown = false;

                try
                {
                    window.DragMove();
                }
                catch (System.InvalidOperationException)
                {

                }
            }

        }
#endregion


#region ComboBox Callback

        private enum CMD_TYPE
        {
            None               = 0x0000,
            Default            = 0x0001,
            Detected           = 0x0002,
            Done               = 0x0004,

            WindowPos          = 0x0100,
            Hotkey             = 0x0200,
            Delay              = 0x0400,

            WindowPos_Default  = WindowPos | Default,
            WindowPos_Detected = WindowPos | Detected,
            WindowPos_Done     = WindowPos | Done,

            Hotkey_Default     = Hotkey | Default,
            Hotkey_Detected    = Hotkey | Detected,
            Hotkey_Done        = Hotkey | Done,

            Delay_Default      = Delay | Default,
            Delay_Detected     = Delay | Detected,
            Delay_Done         = Delay | Done
        };
        private const string mKeyWordWindowsPos = "Pos";
        private const string mKeyWordHotkey = "Key";
        private const string mKeyWordDelay= "Delay";

        private Key CvtStringToKey(string text, out bool isFlagKey)
        {
            isFlagKey = false;
            text = text.ToLower();
            if (text.Contains("shf") || text.Contains("shift"))
            {
                if (text.StartsWith("r"))
                    text = Key.RightShift.ToString().ToLower();
                else
                    text = Key.LeftShift.ToString().ToLower();
            }
            else if (text.Contains("alt"))
            {
                if (text.StartsWith("r"))
                    text = Key.RightAlt.ToString().ToLower();
                else
                    text = Key.LeftAlt.ToString().ToLower();
            }
            else if (text.Contains("ctrl"))
            {
                if (text.StartsWith("r"))
                    text = Key.RightCtrl.ToString().ToLower();
                else
                    text = Key.LeftCtrl.ToString().ToLower();
            }
            else if (text.Contains("win"))
            {
                if (text.StartsWith("r"))
                    text = Key.RWin.ToString().ToLower();
                else
                    text = Key.LWin.ToString().ToLower();
            }
            else if (text.Contains("app"))
            {
                if (text.EndsWith("1"))
                    text = Key.LaunchApplication1.ToString().ToLower();
                else
                    text = Key.LaunchApplication2.ToString().ToLower();
            }

            int[,] keyRange = new int[,] {
                { 18, 33 , 0},//Space
                { 34, 43 , 0},//A~Z
                { 44, 69 , 0},//A~Z
                { 70, 71 , 1},//LWin,RWin
                { 90, 113 , 0},//F0~F24
                { 116, 121 , 1},//LeftShift,RightShift,LeftCtrl,RightCtrl,LeftAlt,RightAlt
                { 138, 139 , 0},//LaunchApplication1,LaunchApplication2
            };

            for (int r0=0;r0<keyRange.GetLength(0);r0++)
            {
                for (int k = keyRange[r0, 0]; k <= keyRange[r0, 1]; k++)//A->Z
                {
                    Key key = (Key)k;
                    //Console.WriteLine(key.ToString());
                    if (key.ToString().ToLower().Equals(text))
                    {
                        isFlagKey = keyRange[r0, 2] == 1;
                        return key;
                    }
                }
            }
            
            return Key.None;
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
        private CMD_TYPE CheckCmd(string text,bool isSetParam, out string textResult)
        {
            string textLower = text.ToLower();
            textResult = "";
            if (!textLower.StartsWith("set")) return CMD_TYPE.None;

            var cmds = textLower.Split(' ');

            if (cmds.Length >= 2)
            {
                textResult = mKeyWordWindowsPos + "|" + mKeyWordHotkey + "|" + mKeyWordDelay;
                if (cmds[1].Equals(mKeyWordWindowsPos.ToLower()))
                {
                    if (cmds.Length == 2)
                    {
                        double posx, posy;
                        try
                        {
                            posx = Properties.Settings.Default.WindowPositionX;
                        }
                        catch (System.NullReferenceException)
                        {
                            posx = Double.NaN;
                        }

                        try
                        {
                            posy = Properties.Settings.Default.WindowPositionY;
                        }
                        catch (System.NullReferenceException)
                        {
                            posy = Double.NaN;
                        }

                        if (double.IsNaN(posx) && double.IsNaN(posy))
                            textResult = "Cur:Center";
                        else if (!double.IsNaN(posx) && !double.IsNaN(posy))
                            textResult = "Cur:" + (int)posx + "," + (int)posy;
                        else
                            textResult = "Cur:Mouse";
                        return CMD_TYPE.WindowPos_Default;
                    }

                    else if (cmds[2].ToLower().Equals("mouse"))
                    {
                        textResult = "Mouse";
                        if (isSetParam)
                        {
                            Properties.Settings.Default.WindowPositionX = Double.NaN;
                            Properties.Settings.Default.WindowPositionY = 0;
                            Properties.Settings.Default.Save();
                        }
                        return CMD_TYPE.WindowPos_Done;
                    }
                    else if (cmds[2].ToLower().Equals("center"))
                    {
                        textResult = "Center";
                        if (isSetParam)
                        {
                            Properties.Settings.Default.WindowPositionX = Double.NaN;
                            Properties.Settings.Default.WindowPositionY = Double.NaN;
                            Properties.Settings.Default.Save();
                        }
                        return CMD_TYPE.WindowPos_Done;
                    }
                    else if (cmds[2].ToLower().Equals("current"))
                    {
                        textResult = "Current";
                        
                        if (isSetParam)
                        {
                            Properties.Settings.Default.WindowPositionX = this.Left;
                            Properties.Settings.Default.WindowPositionY = this.Top;
                            Properties.Settings.Default.Save();
                        }
                        return CMD_TYPE.WindowPos_Done;
                    }
                    else
                    {
                        int posx = (int)this.Left, posy = (int)this.Top;
                        bool isNumX, isNumY;
                        isNumX = isNumY = false;

                        if (cmds.Length >= 3)
                            isNumX = int.TryParse(cmds[2], out posx);
                        if (cmds.Length >= 4)
                            isNumY = int.TryParse(cmds[3], out posy);

                        if (isSetParam && isNumX && isNumY)
                        {
                            Properties.Settings.Default.WindowPositionX = posx;
                            Properties.Settings.Default.WindowPositionY = posy;
                            Properties.Settings.Default.Save();
                        }
                        if (!isNumX && !isNumY)
                            textResult = "Center|Mouse|Current|X,Y";
                        else
                            textResult = (isNumX ? posx.ToString() : "X") + "," + (isNumY ? posy.ToString() : "Y");

                        return isNumX && isNumY ? CMD_TYPE.WindowPos_Done : CMD_TYPE.WindowPos_Detected;
                    }
                }
                else if (cmds[1].Equals(mKeyWordHotkey.ToLower()))
                {
                    bool isFlag0 = false, isFlag1 = false;

                    if (cmds.Length == 2)
                    {
                        textResult = "Cur:" + Properties.Settings.Default.GlobalHotkey0;
                        if (!Properties.Settings.Default.GlobalHotkey1.Equals("None"))
                            textResult += " + " + Properties.Settings.Default.GlobalHotkey1;
                        return CMD_TYPE.Hotkey_Default;
                    }

                    Key key0 = Key.None, key1 = Key.None;

                    if (cmds.Length >= 3)
                        key0 = CvtStringToKey(cmds[2], out isFlag0);
                    if (cmds.Length >= 4)
                        key1 = CvtStringToKey(cmds[3], out isFlag1);

                    textResult = key0.ToString();
                    if (key1 != Key.None)
                        textResult += " + " + key1.ToString();

                    if (key0 != Key.None)// || key1 != Key.None
                    {
                        if (key0 != Key.None && key1 != Key.None)
                        {
                            if (!isFlag0 && !isFlag1)
                                return CMD_TYPE.Hotkey_Detected;
                            if (!isFlag0 && isFlag1)
                            {
                                Swap(ref isFlag0, ref isFlag1);
                                Swap(ref key0, ref key1);
                            }
                        }
                        if (isFlag0 && isFlag1)
                            return CMD_TYPE.Hotkey_Detected;
                        else
                        {
                            if (isSetParam)
                            {
                                Properties.Settings.Default.GlobalHotkey0 = key0.ToString();
                                Properties.Settings.Default.GlobalHotkey1 = key1.ToString();
                                Properties.Settings.Default.Save();
                            }
                            return CMD_TYPE.Hotkey_Done;
                        }
                    }

                    return CMD_TYPE.Hotkey_Detected;
                }
                else if (cmds[1].Equals(mKeyWordDelay.ToLower()))
                {
                    if (cmds.Length == 2)
                    {
                        textResult = Properties.Settings.Default.GlobalDelay + " ms";
                        return CMD_TYPE.Delay_Default;
                    }
                    bool isNum = false;
                    int delay = -1;
                    if (cmds.Length >= 3)
                        isNum = int.TryParse(cmds[2], out delay);
                    textResult = (isNum ? delay.ToString() : "??") + " ms";
                    if (isNum && delay >= 0)
                    {
                        if (isSetParam)
                        {
                            Properties.Settings.Default.GlobalDelay = delay;
                            Properties.Settings.Default.Save();
                        }
                        return CMD_TYPE.Delay_Done;
                    }
                    return CMD_TYPE.WindowPos_Detected;
                }
            }
            return CMD_TYPE.Detected;
        }

        private bool UpdateResult()
        {
            if (mCalcCur == null || Double.IsNaN(mCalcCur.mCalcResult)) return false;

            if (MenuItemShowHex.IsChecked == true || MenuItemShowBinary.IsChecked == true)
            {
                long resVal = ((long)mCalcCur.mCalcResult);
                if ((double)resVal != mCalcCur.mCalcResult)
                {
                    mTextResult.Text = mCalcCur.mCalcResult.ToString();
                    return false;
                }

                if (MenuItemShowHex.IsChecked == true)
                {
                    string hexTex = resVal.ToString("X");
                    mTextResult.Text = "0x" + hexTex;
                }
                else if (MenuItemShowBinary.IsChecked == true)
                {
                    string binTex = Convert.ToString(resVal, 2);
                    if (binTex.Length > 36)
                    {
                        string hexTex = resVal.ToString("X");
                        mTextResult.Text = "0x" + hexTex;
                    }
                    else
                    {
                        for (int i = binTex.Length % 4; i < binTex.Length; i += 5)
                        {
                            binTex = binTex.Insert(i, " ");
                        }
                        if (binTex.StartsWith(" "))
                            binTex = binTex.Remove(0, 1);
                        mTextResult.Text = binTex + "b";
                    }
                }
                Size size = MeasureString(mTextResult.Text);
                mTextResult.Width = size.Width;
            }
            else
                mTextResult.Text = mCalcCur.mCalcResult.ToString();

            return true;
        }

        private int mDebugMode = 0;
        private void ComboBoxCalc_KeyUp(object sender, KeyEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb == null) return;

            Size size = MeasureString(cb.Text);
            cb.Width = size.Width + 17 + 2 + 24;

            MenuItemDelSelected.IsEnabled = false;

            string cmdResultMsg;
            CMD_TYPE cmdType = CheckCmd(cb.Text,false, out cmdResultMsg);
            if (e.Key == Key.Escape)
            {
                if (cb.Text.Length == 0)
                {
                    this.Hide();
                }
                else
                {
                    cb.Text = "";
                    mTextResult.Text = "----";
                    mTextResult.Opacity = 0.5;
                }
            }
            else if (e.Key == Key.Up && mComboBoxCalc.IsDropDownOpen)
            {
                mComboBoxCalc.IsDropDownOpen = false;
            }
            else if (e.Key == Key.Down && !mComboBoxCalc.IsDropDownOpen)
            {
                mComboBoxCalc.IsDropDownOpen = true;
            }
            else if (cmdType != CMD_TYPE.None)
            {
                if (e.Key == Key.Enter && (cmdType & CMD_TYPE.Done) != 0)
                {
                    cmdType = CheckCmd(cb.Text, true, out cmdResultMsg);
                    if ((cmdType & CMD_TYPE.Done) != 0)
                    {
                        cb.Text = "";
                        mTextResult.Text = "Saved";
                        cmdType = CMD_TYPE.None;
                    }
                    else
                    {
                        mTextResult.Text = "ERROR?!?!";
                    }
                }
                else
                    mTextResult.Text = cmdResultMsg;

                mTextResult.Opacity =
                    ((cmdType & CMD_TYPE.Done) != 0 || (cmdType & CMD_TYPE.Default) != 0) ? 1:0.5;
            }
            else
            {
                //MenuItemDebug.Header = "[Debug] " + (mDebugMode == 0?"Both":mDebugMode == 1? "3rd":"Native");
                double resultNative = double.NaN;
                double result3rd = double.NaN;

                mTextEqualitySign.Text = "=";
                bool isErrorDetected = false;
                if (mDebugMode == 0 || mDebugMode >= 2)
                {
                    try
                    {
                        RegexFormulas.EvalWarningFlags flags = RegexFormulas.EvalWarningFlags.None;
                        resultNative = RegexFormulas.Eval(cb.Text, ref flags);
                        if ((flags & RegexFormulas.EvalWarningFlags.DoubleToInt) != 0)
                        {
                            Console.WriteLine(RegexFormulas.EvalWarningFlags.DoubleToInt);
                            mTextEqualitySign.Text = "≈";
                        }

                    }
                    catch (ArgumentOutOfRangeException ea)
                    {
                        isErrorDetected = true;
                        mTextResult.Text = "OutOfRange:"+ea.Message;
                        resultNative = double.NaN;
                    }
                    catch (OverflowException eof)
                    {
                        isErrorDetected = true;
                        mTextResult.Text = "Overflow:" + eof.Message;
                        resultNative = double.NaN;
                    }
                    catch (DivideByZeroException ediv)
                    {
                        isErrorDetected = true;
                        mTextResult.Text = "DivideByZero";
                        resultNative = double.NaN;
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine(ex.Message);
                        resultNative = double.NaN;

                    }
                }

                if (mDebugMode == 0 || mDebugMode == 1)
                {
                    try
                    {
                        var result = mInterpreter.Eval(cb.Text);

                        if (result.GetType() == typeof(int))
                            result3rd = (int)result;
                        else if (result.GetType() == typeof(float))
                            result3rd = (double)((float)result);
                        else if (result.GetType() == typeof(double))
                            result3rd = (double)result;

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        result3rd = double.NaN;
                    }
                }

                if (double.IsNaN(resultNative) && double.IsNaN(result3rd))
                {
                    if (!isErrorDetected)
                        mTextResult.Text = "----"; ;
                    mTextResult.Opacity = 0.5;
                    if (mCalcCur != null)
                    {
                        mCalcCur.mCalcResult = Double.NaN;
                        mCalcCur.mCalcTextResult = "";
                    }
                }
                else
                {

                    if (mCalcCur == null)
                        mCalcCur = new CalcItem();
                    mCalcCur.mCalcText = cb.Text;

                    if (!double.IsNaN(resultNative))
                        mCalcCur.mCalcResult = resultNative;
                    else if (!double.IsNaN(result3rd))
                        mCalcCur.mCalcResult = result3rd;

                    mCalcCur.mCalcTextResult = mCalcCur.mCalcResult.ToString();

                    if (mDebugMode == 0)
                        Console.WriteLine("Native:" + resultNative + " 3rd:" + result3rd);

                    if (e.Key == Key.Enter)
                    {
                        cb.Text = "";
                        mTextResult.Text = "Saved";
                        mTextResult.Opacity = 0.5;
                        mCalcList.Add(mCalcCur);
                        mComboBoxCalc.ItemsSource = null;
                        mComboBoxCalc.ItemsSource = mCalcList;
                        mCalcCur = null;
                    }
                    else
                    {
                        UpdateResult();
                        if (mDebugMode == 0)
                        {

                            if ((!double.IsNaN(resultNative) && !double.IsNaN(result3rd) && Math.Abs(resultNative - result3rd) > 2) ||
                                (double.IsNaN(resultNative) && !double.IsNaN(result3rd)))
                                mTextResult.Text = "N:" + resultNative + " 3:" + result3rd;
                        }
                        
                            
                        mTextResult.Opacity = 1;
                    }
                }
            }

            size = MeasureString(mTextResult.Text);
            mTextResult.Width = size.Width;
            e.Handled = true;
        }

        private void ComboBoxCalc_DropDownOpened(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)((ComboBox)sender).Template.FindName("PART_EditableTextBox", (ComboBox)sender);
            textBox.SelectionStart = ((ComboBox)sender).Text.Length;
            textBox.SelectionLength = 0;
        }

#endregion

        private void BtnResult_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(mTextResult.Text);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender == MenuItemClear)
            {
                mCalcList.Clear();
                mComboBoxCalc.ItemsSource = null;
                mComboBoxCalc.ItemsSource = mCalcList;
                mComboBoxCalc.Text = "";
                mTextResult.Text = "----";
                mTextResult.Opacity = 0.5;
            }
            else if (sender == MenuItemDelSelected)
            {
                if (mComboBoxCalc.SelectedIndex >= 0 && mComboBoxCalc.SelectedIndex < mCalcList.Count)
                {
                    mCalcList.RemoveAt(mComboBoxCalc.SelectedIndex);
                    mComboBoxCalc.ItemsSource = null;
                    mComboBoxCalc.ItemsSource = mCalcList;
                    mTextResult.Text = "----";
                    mTextResult.Opacity = 0.5;
                }
            }
            else if (sender == MenuItemShowHex)
            {
                if (MenuItemShowHex.IsChecked == true)
                    MenuItemShowBinary.IsChecked = false;
                UpdateResult();
            }
            else if (sender == MenuItemShowBinary)
            {
                if (MenuItemShowBinary.IsChecked == true)
                    MenuItemShowHex.IsChecked = false;
                UpdateResult();
            }
            else if (sender == MenuItemCopyAll)
            {
                Clipboard.SetText(mComboBoxCalc.Text + " = " + mTextResult.Text);
            }
            else if (sender == MenuItemClose)
            {
                this.Close();
            }
            else if (sender == MenuItemDebug)
            {
                mDebugMode++;
                if (mDebugMode > 2)
                    mDebugMode = 0;
                MenuItemDebug.Header = "[Debug] " + (mDebugMode == 0?"Both":mDebugMode == 1? "3rd":"Native");
            }
        }

        private void ComboBoxCalc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MenuItemDelSelected.IsEnabled = true;
            if (mComboBoxCalc.SelectedIndex >= 0 && mComboBoxCalc.SelectedIndex < mCalcList.Count)
                mCalcCur = new CalcItem(mCalcList[mComboBoxCalc.SelectedIndex]);
        }


        private void ComboBoxItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ComboBoxItem cbItem = sender as ComboBoxItem;

            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (cbItem != null)
                {
                    CalcItem cItem = cbItem.DataContext as CalcItem;
                    if (cItem != null)
                    {
                        mCalcList.Remove(cItem);
                        mComboBoxCalc.ItemsSource = null;
                        mComboBoxCalc.ItemsSource = mCalcList;
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
