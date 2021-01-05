﻿using System.IO;
using System.Windows;
using System.Windows.Controls;
using TEKLauncher.ARK;
using TEKLauncher.Data;
using static System.IO.Directory;
using static System.Windows.Application;
using static TEKLauncher.App;
using static TEKLauncher.CommunismMode;
using static TEKLauncher.Data.LocalizationManager;
using static TEKLauncher.Data.Settings;
using static TEKLauncher.UI.Message;
using static TEKLauncher.Utils.UtilFunctions;

namespace TEKLauncher.Pages
{
    public partial class LauncherPage : Page
    {
        public LauncherPage()
        {
            InitializeComponent();
            if (LocCulture == "ar")
                DTCGrid.FlowDirection = PSGrid.FlowDirection = FlowDirection.RightToLeft;
            GamePath.SetPath(Game.Path);
            AutoRetry.IsChecked = Settings.AutoRetry;
            CloseOnGameRun.IsChecked = Settings.CloseOnGameRun;
            Communism.IsChecked = Settings.CommunismMode;
        }
        private void ChangeGamePath(object Sender, RoutedEventArgs Args)
        {
            if (FileExists($@"{GamePath.Text}\ShooterGame\Binaries\Win64\ShooterGame.exe"))
            {
                if (ShowOptions("Warning", LocString(LocCode.GamePathPrompt)))
                {
                    ARKPath = GamePath.Text;
                    Current.Shutdown();
                }
                else
                    GamePath.SetPath(Game.Path);
            }
            else
            {
                Show("Warning", LocString(LocCode.CantUsePath));
                GamePath.SetPath(Game.Path);
            }
        }
        private void CleanDownloadCache(object Sender, RoutedEventArgs Args)
        {
            if (ShowOptions("Warning", LocString(LocCode.CleanDwCachePrompt)))
            {
                DeleteDirectory(DownloadsDirectory);
                CreateDirectory(DownloadsDirectory).Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
        }
        private void DeleteLauncherSettings(object Sender, RoutedEventArgs Args)
        {
            if (ShowOptions("Warning", LocString(LocCode.DelSettingsPrompt)))
            {
                DeleteSettings = true;
                Current.Shutdown();
            }
        }
        private void SetAutoRetry(object Sender, RoutedEventArgs Args)
        {
            if (IsLoaded)
                Settings.AutoRetry = (bool)AutoRetry.IsChecked;
        }
        private void SetCloseOnGameRun(object Sender, RoutedEventArgs Args)
        {
            if (IsLoaded)
                Settings.CloseOnGameRun = (bool)CloseOnGameRun.IsChecked;
        }
        private void SetCommunism(object Sender, RoutedEventArgs Args)
        {
            if (IsLoaded)
                Set(Settings.CommunismMode = (bool)Communism.IsChecked);
        }
    }
}