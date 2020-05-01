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
        Restart,
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
            //Use "Tag" property to store custom parameters (shutdown time or shutdown option)
            {
                new MenuItem("Pick schedule:")
                {
                    Enabled = false,
                },
                new MenuItem("Now", EventCheckMark)
                {
                    Tag = ShutdownTime.Now,
                    RadioCheck = true,
                },
                new MenuItem("1 hour", EventCheckMark)
                {
                    Tag = ShutdownTime.Hrs1,
                    RadioCheck = true,
                    Checked = true
                },
                new MenuItem("2 hours", EventCheckMark)
                {
                    Tag = ShutdownTime.Hrs2,
                    RadioCheck = true,
                },
                new MenuItem("6 hours", EventCheckMark)
                {
                    Tag = ShutdownTime.Hrs6,
                    RadioCheck = true
                },
                new MenuItem("-"),
                new MenuItem("Shutdown action", new MenuItem[] //this is menu item with submenu
                {
                    new MenuItem("Shutdown", EventCheckMark) {RadioCheck = true, Checked = true, Tag = ShutdownOptions.Shutdown },
                    new MenuItem("Restart", EventCheckMark) {RadioCheck = true, Tag = ShutdownOptions.Restart }
                })
                {
                    Name = "ShutdownAction"
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

    //Set check mark for menu items within one group
    private void EventCheckMark(object ClickedItem, EventArgs e)
    {
        //Go through event sender parent's menu items
        //Don't look for currently checked item - just clear them all first...
        foreach (MenuItem mi in (ClickedItem as MenuItem).Parent.MenuItems)
            mi.Checked = false;

        //...and then mark desired as checked
        (ClickedItem as MenuItem).Checked = true;
    }

    //Tray icon click handler
    private void EventIconClick(object s, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            trayIcon.ShowBalloonTip(0);
    }

    //Shutdown action menu handler
    void EventShutdown(object s, EventArgs e)
    {
        //Find checked menu item and fire up shutdown with proper option and time
        //(get option and time from enu item "Tag" properties)
        foreach (MenuItem mi in trayIcon.ContextMenu.MenuItems)
            if (mi.Checked)
            {
                trayIcon.Icon = SHUTDOWN.Properties.Resources.ORANGE;
                trayIcon.Text = "Shutdown scheduled at " +
                    (DateTime.Now.AddSeconds((int)mi.Tag).ToShortTimeString());
                trayIcon.BalloonTipTitle = trayIcon.Text;
                trayIcon.BalloonTipText = "Right-click tray icon to abort scheduled shutdown.";

                //Now get picked item from shutdown options
                foreach (MenuItem mi2 in trayIcon.ContextMenu.MenuItems["ShutdownAction"].MenuItems)
                    if (mi2.Checked)
                    {
                        Shutdown((ShutdownOptions)mi2.Tag, (ShutdownTime)mi.Tag);
                        break;
                    }

                break;
            }
    }

    //Abort action menu handler
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
                case ShutdownOptions.Restart:
                    p.StartInfo.Arguments = string.Join(" ", "/r", "/t", ((int)ShutdownTime).ToString());
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