using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using SndVolPlus.Properties;


namespace SndVolPlus
{
    static class Program
    {
        static NotifyIcon TrayIcon;
        static MMDevice DefaultMediaDevice;
        static string SystemDir = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\";

        static Timer SingleClickWindow;

        [STAThread]
        private static void Main()
        {
            DefaultMediaDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            DefaultMediaDevice.AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);

            TrayIcon = new NotifyIcon();
            TrayIcon.Icon = IconFromVolume();
            TrayIcon.Text = ToolTipFromVolume();
            TrayIcon.MouseClick += new MouseEventHandler(TrayIcon_MouseClick);
            TrayIcon.MouseDoubleClick += new MouseEventHandler(TrayIcon_MouseDoubleClick);
            TrayIcon.Visible = true;

            TrayIcon.ContextMenu = new ContextMenu();
            TrayIcon.ContextMenu.MenuItems.Add(new MenuItem("Open Volume Mixer", (o, e) => { Process.Start(SystemDir + "sndvol.exe"); }));
            TrayIcon.ContextMenu.MenuItems.Add(new MenuItem("-"));
            TrayIcon.ContextMenu.MenuItems.Add(new MenuItem("Playback devices", (o, e) => { Process.Start(SystemDir + "rundll32.exe", @"Shell32.dll,Control_RunDLL mmsys.cpl,,playback"); }));
            TrayIcon.ContextMenu.MenuItems.Add(new MenuItem("Recording devices", (o, e) => { Process.Start(SystemDir + "rundll32.exe", @"Shell32.dll,Control_RunDLL mmsys.cpl,,recording"); }));
            TrayIcon.ContextMenu.MenuItems.Add(new MenuItem("Sounds", (o, e) => { Process.Start(SystemDir + "rundll32.exe", @"Shell32.dll,Control_RunDLL mmsys.cpl,,sounds"); }));
            TrayIcon.ContextMenu.MenuItems.Add(new MenuItem("-"));
            TrayIcon.ContextMenu.MenuItems.Add(new MenuItem("Volume control options", (o, e) => { Process.Start(SystemDir + "sndvol.exe", "-p"); }));

            SingleClickWindow = new Timer();
            SingleClickWindow.Interval = SystemInformation.DoubleClickTime;
            SingleClickWindow.Tick += (o, e) =>
                {
                    SingleClickWindow.Stop();
                    StartVolControl();
                };

            Application.Run();
        }

        private static string ToolTipFromVolume()
        {
            if (DefaultMediaDevice.AudioEndpointVolume.Mute)
                return "Speakers: Muted";
            else
                return string.Format("Speakers: {0:F0}%", DefaultMediaDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
        }

        private static Icon IconFromVolume()
        {
            if (DefaultMediaDevice.AudioEndpointVolume.Mute)
                return Resources.mute;
            else
            {
                if (DefaultMediaDevice.AudioEndpointVolume.MasterVolumeLevelScalar > 0.65)
                    return Resources.vol68;
                else if (DefaultMediaDevice.AudioEndpointVolume.MasterVolumeLevelScalar > 0.32)
                    return Resources.vol56;
                else if (DefaultMediaDevice.AudioEndpointVolume.MasterVolumeLevelScalar > 0)
                    return Resources.vol44;
                else
                    return Resources.vol32;
            }
        }

        private static void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            TrayIcon.Text = ToolTipFromVolume();
            TrayIcon.Icon = IconFromVolume();
        }

        private static void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //We've recieved a second click, so disable the timer to prevent the single click event from occurring
                //SingleClickWindow.Stop();
                StartVolMixer();
            }
        }

        private static void TrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    {
                        if (SingleClickWindow.Enabled)
                            SingleClickWindow.Stop();
                        else
                            SingleClickWindow.Start();
                    } break;
                case MouseButtons.Middle:
                    {
                        DefaultMediaDevice.AudioEndpointVolume.Mute = !DefaultMediaDevice.AudioEndpointVolume.Mute;
                    } break;
            }
        }

        private static void StartVolControl()
        {
            Process SndVol = Process.Start(SystemDir + @"sndvol.exe", "-f");
            MoveSndVolWindow(SndVol.Id);
        }

        private static void StartVolMixer()
        {
            Process SndVol = Process.Start(SystemDir + "sndvol.exe", "-r");
            MoveSndVolWindow(SndVol.Id);
        }

        private static void MoveSndVolWindow(int SndVolId)
        {
            //Wait a little bit for the window to be created, with a max wait time of 100 * 10ms, or 1 second
            for (int i = 0; i < 100; i++)
            {
                System.Threading.Thread.Sleep(10);

                foreach (Process p in Process.GetProcessesByName("sndvol"))
                {
                    if (p.Id == SndVolId && p.MainWindowHandle != IntPtr.Zero)
                    {
                        Unmanaged.RECT SndVolRect = new Unmanaged.RECT();
                        Unmanaged.GetWindowRect(p.MainWindowHandle, out SndVolRect);

                        int margin = 10;
                        int x = 0, y = 0;
                        switch (Helper.GetTaskbarEdge())
                        {
                            case DockStyle.Right:
                            case DockStyle.Bottom:
                                {
                                    x = Screen.PrimaryScreen.WorkingArea.Right - (SndVolRect.right - SndVolRect.left) - margin;
                                    y = Screen.PrimaryScreen.WorkingArea.Bottom - (SndVolRect.bottom - SndVolRect.top) - margin;
                                } break;
                            case DockStyle.Left:
                                {
                                    x = Screen.PrimaryScreen.WorkingArea.Left + margin;
                                    y = Screen.PrimaryScreen.WorkingArea.Bottom - (SndVolRect.bottom - SndVolRect.top) - margin;
                                } break;
                            case DockStyle.Top:
                                {
                                    x = Screen.PrimaryScreen.WorkingArea.Right - (SndVolRect.right - SndVolRect.left) - margin;
                                    y = Screen.PrimaryScreen.WorkingArea.Top + margin;
                                } break;
                        }
                        Unmanaged.SetWindowPos(p.MainWindowHandle.ToInt32(), 0, x, y, 0, 0, 0x4000 | 0x0001);
                    }
                }
            }
        }
    }
}
