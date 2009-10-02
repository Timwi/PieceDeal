﻿using System;
using System.Windows;
using RT.Util;

namespace PieceDeal
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SettingsUtil.LoadSettings(out Program.Settings, "PieceDeal");
            if (Program.Settings == null || !Program.Settings.IsValid)
            {
#if DEBUG
                throw new Exception("Settings file is invalid!");
#else
                Program.Settings = new Settings();
#endif
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SettingsUtil.SaveSettings(Program.Settings, "PieceDeal", SettingsUtil.OnFailure.DoNothing);
            base.OnExit(e);
        }
    }
}
