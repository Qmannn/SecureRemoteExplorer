using System;
using System.Windows;
using ExplorerClient.Gui.View;

namespace ExplorerClient.Gui
{
    public enum Views
    {
        Main,
        Login
    }

    public static class ViewsShow
    {
        public static void ShowView(Views view, bool showDialog)
        {
            switch (view)
            {
                case Views.Main:
                    Window wnd = new MainView();
                    if (showDialog)
                    {
                        wnd.ShowDialog();
                    }
                    else
                    {
                        wnd.Show();
                    }
                    break;
                case Views.Login:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(view), view, null);
            }
        }
    }
}