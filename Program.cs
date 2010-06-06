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
                StartVolMixer();
        }

        private static void TrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    {
                        if (e.Clicks == 0)
                            StartVolControl();
                    } break;
                case MouseButtons.Middle:
                    {
                        DefaultMediaDevice.AudioEndpointVolume.Mute = !DefaultMediaDevice.AudioEndpointVolume.Mute;
                    } break;
            }
        }

        private static void StartVolControl()
        {
            Process.Start(SystemDir + @"sndvol.exe", "-f 67241586 17635");
        }

        private static void StartVolMixer()
        {
            Process.Start(SystemDir + "sndvol.exe", "-r 67241586 17635");
        }
    }
}
