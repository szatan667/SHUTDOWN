using System;
using System.Diagnostics;
using System.Windows.Forms;

/// <summary>
/// Shutdown app running in system tray
/// </summary>
public class SHUTDOWNX : ApplicationContext
{
    static void Main()
    {
        Application.Run(new SHUTDOWNX());
    }

    /// <summary>
    /// Enumerates available shutdown options
    /// </summary>
    enum ShutdownOptions
    {
        Shutdown,
        Abort
    }
    /// <summary>
    /// Enumerates available shutdown delay times [s]
    /// </summary>
    enum ShutdownTime
    {
        /// <summary>
        /// "Now" value is actually 10 seconds instead of 0 which gives user time to abort
        /// </summary>
        Now = 10,
        Hrs1 = 3600,
        Hrs2 = 7200,
        Hrs6 = 21600
    }

    private readonly NotifyIcon trayIcon;

    /// <summary>
    /// Shutdown app running in system tray
    /// </summary>
    public SHUTDOWNX()
    {
        //Must have - we need tray icon for tray-icon-only app
        trayIcon = new NotifyIcon()
        {
            Icon = SHUTDOWN.Properties.Resources.GREEN,
            Text = "Click for shutdown options",
            BalloonTipTitle = "Shutdown options",
            BalloonTipText = "You may shutdown your computer here",
            BalloonTipIcon = ToolTipIcon.Info,
            Visible = true,
            ContextMenu = new ContextMenu(new MenuItem[]
            //Add menu items and set their properties and events
            {
                new MenuItem("Pick schedule:")
                {
                    Enabled = false,
                },
                new MenuItem("Now", EventCheck)
                {
                    Tag = ShutdownTime.Now,
                    RadioCheck = true,
                },
                new MenuItem("1 hour", EventCheck)
                {
                    Tag = ShutdownTime.Hrs1,
                    RadioCheck = true,
                    Checked = true
                },
                new MenuItem("2 hours", EventCheck)
                {
                    Tag = ShutdownTime.Hrs2,
                    RadioCheck = true,
                },
                new MenuItem("6 hours", EventCheck)
                {
                    Tag = ShutdownTime.Hrs6,
                    RadioCheck = true
                },
                new MenuItem("-"),
                new MenuItem("SHUTDOWN!", EventShutdown),
                new MenuItem("Abort shutdown", EventAbort),
                new MenuItem("-"),
                new MenuItem("Exit", EventExit)
            })
        }; //tray icon constructor end

        //Register tray icon click event
        trayIcon.MouseClick += EventIconClick;
    }
    //Shutdown time menu interval handler
    private void EventCheck(object ClickedItem, EventArgs e)
    {
        //Don't look for currently checked item - just clear them all first...
        foreach (MenuItem mi in trayIcon.ContextMenu.MenuItems)
        {
            mi.Checked = false;
        }

        //...and then mark desired as checked
        //(ClickedItem is menu item that triggered this event, we can set it's property here directly)
        (ClickedItem as MenuItem).Checked = true;
    }
    //Tray icon click handler
    private void EventIconClick(object s, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            trayIcon.ShowBalloonTip(0);
    }
    //Shutdown menu handler
    void EventShutdown(object s, EventArgs e)
    {
        //Find checked menu item and fire up shutdown with tagged time value
        //(Tag property is set to ShutdownTime type in tray icon constructor)
        foreach (MenuItem mi in trayIcon.ContextMenu.MenuItems)
            if (mi.Checked)
            {
                trayIcon.Icon = SHUTDOWN.Properties.Resources.ORANGE;
                trayIcon.Text = "Shutdown scheduled at " +
                    (DateTime.Now.AddSeconds((int)mi.Tag).ToShortTimeString());
                trayIcon.BalloonTipTitle = trayIcon.Text;
                trayIcon.BalloonTipText = "Right-click tray icon to abort scheduled shutdown.";

                Shutdown(ShutdownOptions.Shutdown, (ShutdownTime)mi.Tag);

                break;
            }
    }
    //Abort menu handler
    void EventAbort(object s, EventArgs e)
    {
        trayIcon.Icon = SHUTDOWN.Properties.Resources.GREEN;
        trayIcon.Text = "Click for shutdown options";
        trayIcon.BalloonTipTitle = "Shutdown options";
        trayIcon.BalloonTipText = "You may shutdown your computer here";

        Shutdown(ShutdownOptions.Abort, ShutdownTime.Now);
    }
    //Exit menu handler
    void EventExit(object s, EventArgs e)
    {
        trayIcon.Dispose();
        Application.Exit();
    }
    /// <summary>
    /// Execute "shutdown" system command.
    /// </summary>
    /// <param name="ShutdownOption">Shutdown action to execute.</param>
    /// <param name="ShutdownTime">Time to execute the option.</param>
    void Shutdown(ShutdownOptions ShutdownOption, ShutdownTime ShutdownTime)
    {
        //Run system "shutdown.exe" command with desired option and time
        using (Process p = new Process())
        {
            p.StartInfo.FileName = Environment.GetEnvironmentVariable("windir") + @"\system32\shutdown.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;

            //Each option requires different set of arguments and triggers different GUI setup
            //so we can't just put one-liner command execute here
            switch (ShutdownOption)
            {
                case ShutdownOptions.Shutdown:
                    p.StartInfo.Arguments = string.Join(" ", "/s", "/t", ((int)ShutdownTime).ToString());
                    break;
                case ShutdownOptions.Abort:
                    p.StartInfo.Arguments = "/a";
                    break;
                default:
                    break;
            }

            //Shut me down!
            p.Start();
        }
    }
}