// CtrlIt Work Timer - a small always-on-top work timer for Windows.
// Written in C# 5 so it compiles with the compiler built into Windows
// (C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe). See build.bat.

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: System.Reflection.AssemblyTitle(CtrlItWorkTimer.AppInfo.Name)]
[assembly: System.Reflection.AssemblyProduct(CtrlItWorkTimer.AppInfo.Name)]
[assembly: System.Reflection.AssemblyDescription("Always-on-top work timer that beeps every 15 minutes")]
[assembly: System.Reflection.AssemblyCopyright("Copyright (c) 2026 " + CtrlItWorkTimer.AppInfo.Author + " - MIT licence")]
[assembly: System.Reflection.AssemblyVersion(CtrlItWorkTimer.AppInfo.Version + ".0")]
[assembly: System.Reflection.AssemblyFileVersion(CtrlItWorkTimer.AppInfo.Version + ".0")]

namespace CtrlItWorkTimer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool createdNew;
            using (Mutex mutex = new Mutex(true, "CtrlItWorkTimer_SingleInstance_Mutex", out createdNew))
            {
                if (!createdNew)
                {
                    // Already running - don't start a second copy.
                    return;
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new TimerForm());
            }
        }
    }

    class Settings
    {
        public int Opacity = 100;        // percent, 20..100
        public bool AutoMode = false;
        public bool StayOnTop = true;
        public int Left = -1;
        public int Top = -1;

        static string Dir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CtrlItWorkTimer"); }
        }
        static string FilePath
        {
            get { return Path.Combine(Dir, "settings.ini"); }
        }
        // Settings from before the app was renamed
        static string OldFilePath
        {
            get { return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WorkTimer"), "settings.ini"); }
        }

        public static Settings Load()
        {
            Settings s = new Settings();
            try
            {
                string path = File.Exists(FilePath) ? FilePath : OldFilePath;
                if (!File.Exists(path)) return s;
                foreach (string raw in File.ReadAllLines(path))
                {
                    int eq = raw.IndexOf('=');
                    if (eq < 0) continue;
                    string key = raw.Substring(0, eq).Trim();
                    string val = raw.Substring(eq + 1).Trim();
                    int n;
                    switch (key)
                    {
                        case "Opacity": if (int.TryParse(val, out n)) s.Opacity = Math.Max(20, Math.Min(100, n)); break;
                        case "AutoMode": s.AutoMode = val == "1"; break;
                        case "StayOnTop": s.StayOnTop = val == "1"; break;
                        case "Left": if (int.TryParse(val, out n)) s.Left = n; break;
                        case "Top": if (int.TryParse(val, out n)) s.Top = n; break;
                    }
                }
            }
            catch { }
            return s;
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllLines(FilePath, new string[] {
                    "Opacity=" + Opacity,
                    "AutoMode=" + (AutoMode ? "1" : "0"),
                    "StayOnTop=" + (StayOnTop ? "1" : "0"),
                    "Left=" + Left,
                    "Top=" + Top
                });
            }
            catch { }
        }
    }

    // --------------------------------------------------------------- about

    // Edit these to change what the About window shows. Empty links are hidden.
    static class AppInfo
    {
        public const string Name = "CtrlIt Work Timer";
        public const string Version = "1.0.0";
        public const string Author = "ctrlittools";
        public const string Website = "https://github.com/ctrlittools/CtrlIt-WorkTimer";
        public const string Donate = "https://ko-fi.com/ctrlittools";
    }

    static class AppIcon
    {
        // The icon file is built into the program (see build.bat), so each
        // size uses its own hand-made image rather than a shrunk large one.
        public static Icon Load(Size size)
        {
            try
            {
                using (Stream st = System.Reflection.Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("CtrlItWorkTimer.ico"))
                {
                    if (st != null) return new Icon(st, size);
                }
            }
            catch (Exception) { }
            try { return Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch (Exception) { return SystemIcons.Application; }
        }
    }

    class AboutForm : Form
    {
        public AboutForm()
        {
            Text = "About " + AppInfo.Name;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            TopMost = true;
            StartPosition = FormStartPosition.CenterScreen;
            Font = SystemFonts.MessageBoxFont;
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(14, 12, 14, 12);
            Icon = AppIcon.Load(new Size(32, 32));

            TableLayoutPanel main = new TableLayoutPanel();
            main.AutoSize = true;
            main.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            main.ColumnCount = 3;
            main.RowCount = 3;
            main.Location = new Point(Padding.Left, Padding.Top);
            Controls.Add(main);

            PictureBox pic = new PictureBox();
            int iconPx = LogicalToDevice(48);
            using (Icon big = AppIcon.Load(new Size(iconPx, iconPx))) pic.Image = big.ToBitmap();
            pic.SizeMode = PictureBoxSizeMode.AutoSize;
            pic.Margin = new Padding(0, 2, 14, 0);
            main.Controls.Add(pic, 0, 0);
            main.SetRowSpan(pic, 2);

            Label title = new Label();
            title.Text = AppInfo.Name + "   " + AppInfo.Version;
            title.Font = new Font(Font.FontFamily, Font.SizeInPoints * 1.35f, FontStyle.Bold);
            title.AutoSize = true;
            title.Margin = new Padding(0, 2, 24, 2);
            main.Controls.Add(title, 1, 0);

            Label free = new Label();
            free.Text = "This program is free and open source (MIT licence)";
            free.AutoSize = true;
            free.Margin = new Padding(0, 2, 24, 2);
            main.Controls.Add(free, 1, 1);

            Button ok = new Button();
            ok.Text = "OK";
            ok.DialogResult = DialogResult.OK;
            ok.AutoSize = true;
            ok.MinimumSize = new Size(LogicalToDevice(80), 0);
            ok.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ok.Click += delegate { Close(); };
            main.Controls.Add(ok, 2, 0);
            main.SetRowSpan(ok, 2);
            AcceptButton = ok;
            CancelButton = ok;

            TableLayoutPanel info = new TableLayoutPanel();
            info.AutoSize = true;
            info.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            info.ColumnCount = 2;
            info.Margin = new Padding(0, 16, 0, 0);
            main.Controls.Add(info, 0, 2);
            main.SetColumnSpan(info, 3);

            AddRow(info, "Written by", AppInfo.Author, null);
            if (AppInfo.Website.Length > 0)
            {
                string latest = AppInfo.Website.TrimEnd(new char[] { '/' }) + "/releases/latest";
                AddRow(info, "Check for the latest version at:", latest, latest);
                AddRow(info, "Source code and help:", AppInfo.Website, AppInfo.Website);
            }
            if (AppInfo.Donate.Length > 0)
                AddRow(info, "If it saves you time, support it at:", AppInfo.Donate, AppInfo.Donate);
        }

        int LogicalToDevice(int px)
        {
            using (Graphics g = CreateGraphics()) return (int)Math.Round(px * g.DpiX / 96f);
        }

        static void AddRow(TableLayoutPanel t, string caption, string text, string url)
        {
            int row = t.RowCount;
            t.RowCount = row + 1;

            Label cap = new Label();
            cap.Text = caption;
            cap.AutoSize = true;
            cap.Anchor = AnchorStyles.Right;
            cap.Margin = new Padding(0, 3, 8, 3);
            t.Controls.Add(cap, 0, row);

            Control value;
            if (url == null)
            {
                Label l = new Label();
                l.Text = text;
                value = l;
            }
            else
            {
                LinkLabel link = new LinkLabel();
                link.Text = text;
                link.LinkClicked += delegate
                {
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo(url);
                        psi.UseShellExecute = true;
                        Process.Start(psi);
                    }
                    catch (Exception) { }
                };
                value = link;
            }
            value.AutoSize = true;
            value.Anchor = AnchorStyles.Left;
            value.Margin = new Padding(0, 3, 0, 3);
            t.Controls.Add(value, 1, row);
        }
    }

    class TimerForm : Form
    {
        const int BeepIntervalMinutes = 15;
        const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string RunValueName = "CtrlIt Work Timer";
        const string OldRunValueName = "WorkTimer";

        readonly Settings settings;
        readonly Stopwatch stopwatch = new Stopwatch();
        readonly System.Windows.Forms.Timer uiTimer = new System.Windows.Forms.Timer();

        Label timeLabel;
        Button startButton, stopButton, resetButton;
        TrackBar opacityBar;
        Label opacityLabel;

        int lastBeepBlock = 0;     // how many 15-minute blocks we've already beeped for
        bool sessionLocked = false;

        public TimerForm()
        {
            settings = Settings.Load();
            BuildUi();

            TopMost = settings.StayOnTop;
            Opacity = settings.Opacity / 100.0;

            if (settings.Left >= 0 && settings.Top >= 0 && IsOnScreen(settings.Left, settings.Top))
            {
                StartPosition = FormStartPosition.Manual;
                Location = new Point(settings.Left, settings.Top);
            }
            else
            {
                StartPosition = FormStartPosition.Manual;
                Rectangle wa = Screen.PrimaryScreen.WorkingArea;
                Location = new Point(wa.Right - Width - 20, wa.Top + 20);
            }

            uiTimer.Interval = 250;
            uiTimer.Tick += delegate { UpdateDisplay(); };
            uiTimer.Start();

            SystemEvents.SessionSwitch += OnSessionSwitch;
            SystemEvents.PowerModeChanged += OnPowerModeChanged;

            // Keep the Windows start-up entry in step with the setting
            // (also fixes the path if the exe has been moved).
            SetRunAtLogin(settings.AutoMode);

            // In auto mode the app is launched at login, so start counting straight away.
            if (settings.AutoMode) StartTimer();

            UpdateDisplay();
        }

        void BuildUi()
        {
            Text = AppInfo.Name;
            Icon = AppIcon.Load(new Size(32, 32));
            FormBorderStyle = FormBorderStyle.None;
            Padding = new Padding(1);
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            ClientSize = new Size(260, 122);
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.FromArgb(30, 30, 34);
            ForeColor = Color.White;

            timeLabel = new Label();
            timeLabel.Text = "00:00:00";
            timeLabel.Font = new Font("Consolas", 28F, FontStyle.Bold);
            timeLabel.TextAlign = ContentAlignment.MiddleCenter;
            timeLabel.SetBounds(0, 4, 260, 48);
            Controls.Add(timeLabel);

            startButton = MakeButton("Start", 10);
            stopButton = MakeButton("Stop", 95);
            resetButton = MakeButton("Reset", 180);
            startButton.Click += delegate { StartTimer(); };
            stopButton.Click += delegate { StopTimer(); };
            resetButton.Click += delegate { ResetTimer(); };

            opacityLabel = new Label();
            opacityLabel.SetBounds(10, 92, 90, 20);
            opacityLabel.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(opacityLabel);

            opacityBar = new TrackBar();
            opacityBar.Minimum = 20;
            opacityBar.Maximum = 100;
            opacityBar.TickStyle = TickStyle.None;
            opacityBar.AutoSize = false;
            opacityBar.SetBounds(100, 90, 150, 24);
            opacityBar.Value = settings.Opacity;
            opacityBar.ValueChanged += delegate
            {
                settings.Opacity = opacityBar.Value;
                Opacity = settings.Opacity / 100.0;
                opacityLabel.Text = "Opacity " + settings.Opacity + "%";
            };
            Controls.Add(opacityBar);
            opacityLabel.Text = "Opacity " + settings.Opacity + "%";

            // Close button (top-right corner)
            Label closeLabel = new Label();
            closeLabel.Text = "×";
            closeLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            closeLabel.ForeColor = Color.FromArgb(160, 160, 170);
            closeLabel.TextAlign = ContentAlignment.MiddleCenter;
            closeLabel.SetBounds(236, 2, 22, 22);
            closeLabel.Cursor = Cursors.Hand;
            closeLabel.MouseEnter += delegate { closeLabel.ForeColor = Color.FromArgb(255, 110, 110); };
            closeLabel.MouseLeave += delegate { closeLabel.ForeColor = Color.FromArgb(160, 160, 170); };
            closeLabel.Click += delegate { Close(); };
            Controls.Add(closeLabel);
            closeLabel.BringToFront();

            // Right-click menu
            ContextMenuStrip menu = new ContextMenuStrip();

            ToolStripMenuItem topItem = new ToolStripMenuItem("Stay on top");
            topItem.CheckOnClick = true;
            topItem.Checked = settings.StayOnTop;
            topItem.CheckedChanged += delegate
            {
                settings.StayOnTop = topItem.Checked;
                TopMost = settings.StayOnTop;
                settings.Save();
            };
            menu.Items.Add(topItem);

            ToolStripMenuItem autoItem = new ToolStripMenuItem("Auto (reset on lock/sleep, start at login)");
            autoItem.CheckOnClick = true;
            autoItem.Checked = settings.AutoMode;
            autoItem.ToolTipText =
                "Resets the timer when the PC is locked, put to sleep, hibernated or shut down,\n" +
                "and starts it when you log in / unlock. CtrlIt Work Timer also launches at Windows login.";
            autoItem.CheckedChanged += delegate
            {
                settings.AutoMode = autoItem.Checked;
                SetRunAtLogin(settings.AutoMode);
                settings.Save();
            };
            menu.Items.Add(autoItem);

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("About " + AppInfo.Name, null, delegate { ShowAbout(); });
            menu.Items.Add("Exit", null, delegate { Close(); });

            // Right-click anywhere on the window opens the menu
            ContextMenuStrip = menu;
            foreach (Control c in Controls) c.ContextMenuStrip = menu;

            // No title bar, so let the user drag the window by its background
            MakeDraggable(this);
            MakeDraggable(timeLabel);
            MakeDraggable(opacityLabel);

            ToolTip tips = new ToolTip();
            tips.SetToolTip(timeLabel, "Drag to move. Right-click for options.");
            tips.SetToolTip(closeLabel, "Close");
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 0x2;

        const int WM_SETICON = 0x80;
        Icon bigIcon, smallIcon;

        // WinForms leaves borderless windows without an icon, so the taskbar
        // shows a blank one. Give the window its icons directly.
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyTaskbarIcon();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyTaskbarIcon();
        }

        void ApplyTaskbarIcon()
        {
            if (bigIcon == null) bigIcon = AppIcon.Load(SystemInformation.IconSize);
            if (smallIcon == null) smallIcon = AppIcon.Load(SystemInformation.SmallIconSize);
            SendMessage(Handle, WM_SETICON, (IntPtr)1, bigIcon.Handle);    // ICON_BIG
            SendMessage(Handle, WM_SETICON, (IntPtr)0, smallIcon.Handle);  // ICON_SMALL
        }

        void MakeDraggable(Control c)
        {
            c.MouseDown += delegate(object s, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left) return;
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Thin outline so the borderless window is visible on any background
            using (Pen p = new Pen(Color.FromArgb(80, 80, 92)))
                e.Graphics.DrawRectangle(p, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }

        Button MakeButton(string text, int x)
        {
            Button b = new Button();
            b.Text = text;
            b.SetBounds(x, 56, 70, 28);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 100);
            b.BackColor = Color.FromArgb(50, 50, 58);
            b.ForeColor = Color.White;
            Controls.Add(b);
            return b;
        }

        // ---------- timer control ----------

        void StartTimer()
        {
            if (!stopwatch.IsRunning) stopwatch.Start();
            UpdateDisplay();
        }

        void StopTimer()
        {
            if (stopwatch.IsRunning) stopwatch.Stop();
            UpdateDisplay();
        }

        void ResetTimer()
        {
            bool wasRunning = stopwatch.IsRunning;
            stopwatch.Reset();
            lastBeepBlock = 0;
            if (wasRunning) stopwatch.Start();   // manual reset while running keeps it running from zero
            UpdateDisplay();
        }

        void AutoResetAndStop()
        {
            stopwatch.Reset();
            lastBeepBlock = 0;
            UpdateDisplay();
        }

        void UpdateDisplay()
        {
            TimeSpan t = stopwatch.Elapsed;
            timeLabel.Text = string.Format("{0:00}:{1:00}:{2:00}", (int)t.TotalHours, t.Minutes, t.Seconds);
            timeLabel.ForeColor = stopwatch.IsRunning ? Color.FromArgb(120, 230, 140) : Color.FromArgb(200, 200, 200);
            startButton.Enabled = !stopwatch.IsRunning;
            stopButton.Enabled = stopwatch.IsRunning;

            int block = (int)(t.TotalMinutes / BeepIntervalMinutes);
            if (block > lastBeepBlock)
            {
                lastBeepBlock = block;
                Beep();
            }
        }

        static void Beep()
        {
            // Run on a background thread so the UI doesn't freeze while beeping.
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    Console.Beep(1000, 250);
                    Thread.Sleep(120);
                    Console.Beep(1000, 250);
                }
                catch
                {
                    System.Media.SystemSounds.Exclamation.Play();
                }
            });
        }

        // ---------- Windows events (auto mode) ----------

        void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            RunOnUi(delegate
            {
                switch (e.Reason)
                {
                    case SessionSwitchReason.SessionLock:
                    case SessionSwitchReason.SessionLogoff:
                    case SessionSwitchReason.ConsoleDisconnect:
                    case SessionSwitchReason.RemoteDisconnect:
                        sessionLocked = true;
                        if (settings.AutoMode) AutoResetAndStop();
                        break;

                    case SessionSwitchReason.SessionUnlock:
                    case SessionSwitchReason.SessionLogon:
                    case SessionSwitchReason.ConsoleConnect:
                    case SessionSwitchReason.RemoteConnect:
                        sessionLocked = false;
                        if (settings.AutoMode) StartTimer();
                        break;
                }
            });
        }

        void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            RunOnUi(delegate
            {
                if (e.Mode == PowerModes.Suspend)       // sleep or hibernate
                {
                    if (settings.AutoMode) AutoResetAndStop();
                }
                else if (e.Mode == PowerModes.Resume)
                {
                    // If Windows asks for a password on wake, the unlock event starts the timer.
                    // If it doesn't lock, start now.
                    if (settings.AutoMode && !sessionLocked) StartTimer();
                }
            });
        }

        void RunOnUi(MethodInvoker action)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }

        // ---------- start with Windows ----------

        static void SetRunAtLogin(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
                {
                    if (key == null) return;
                    // Remove the entry used before the app was renamed
                    if (key.GetValue(OldRunValueName) != null) key.DeleteValue(OldRunValueName, false);
                    if (enable)
                        key.SetValue(RunValueName, "\"" + Application.ExecutablePath + "\"");
                    else if (key.GetValue(RunValueName) != null)
                        key.DeleteValue(RunValueName, false);
                }
            }
            catch { }
        }

        static bool IsOnScreen(int x, int y)
        {
            foreach (Screen s in Screen.AllScreens)
                if (s.WorkingArea.Contains(x + 20, y + 10)) return true;
            return false;
        }

        AboutForm about;

        void ShowAbout()
        {
            if (about == null || about.IsDisposed) about = new AboutForm();
            about.Show();
            about.Activate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (about != null && !about.IsDisposed) about.Close();
            // Static events must be unhooked or they keep the form alive.
            SystemEvents.SessionSwitch -= OnSessionSwitch;
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
            uiTimer.Stop();

            if (WindowState == FormWindowState.Normal)
            {
                settings.Left = Left;
                settings.Top = Top;
            }
            settings.Save();
            base.OnFormClosing(e);
        }
    }
}
