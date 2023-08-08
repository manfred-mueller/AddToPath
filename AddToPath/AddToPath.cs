using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace AddToPath
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.Directory)]
    public class AddToPath : SharpContextMenu
    {
        // Get the path of the selected item (first selected path).
        private string SelectedItemPath => SelectedItemPaths.First();

        // Construct a regular expression pattern to match the selected item in the PATH.
        private string ItemRegexPattern =>  $"(?<=^|;){Regex.Escape(SelectedItemPath)}(?=$|;)";

        // Override method to determine whether to show the context menu.
        protected override bool CanShowMenu()
        {
            return true; // Always show the menu.
        }

        // Override method to create the context menu.
        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();
            var isInPath = IsInPath();

            // Create a menu item based on whether the selected item is in the PATH.
            var itemCountLines = new ToolStripMenuItem()
            {
                Text = isInPath ? "Remove from PATH (User)" : "Add to PATH (User)",
            };

            // Handle the click event for the menu item.
            itemCountLines.Click += (sender, args) =>
            {
                if (isInPath)
                {
                    RunInSeparateThread(RemoveSelectedItemFromPath);
                }
                else
                {
                    RunInSeparateThread(AddSelectedItemToPath);
                }
            };

            // Add the menu item to the context menu.
            menu.Items.Add(itemCountLines);

            return menu;
        }

        // Run the specified method in a separate thread.
        private void RunInSeparateThread(ThreadStart start)
        {
            var worker = new Thread(start);
            worker.SetApartmentState(ApartmentState.STA);
            worker.Start();
        }

        // Add the selected item to the PATH.
        private void AddSelectedItemToPath()
        {
            var oldPath = GetPath();
            var newPath = $"{oldPath};{SelectedItemPath}";
            SetPath(newPath);
        }
        
        // Remove the selected item from the PATH.
        private void RemoveSelectedItemFromPath()
        {
            var oldPath = GetPath();
            var newPath = Regex.Replace(oldPath, ItemRegexPattern, "");
            SetPath(newPath);
        }

        // Check if the selected item is already in the PATH.
        private bool IsInPath()
        {
            return Regex.Match(GetPath(), ItemRegexPattern).Success;
        }
        
        // Get the current PATH environment variable.
        private static string GetPath()
        {
            return Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
        }

        // Set the PATH environment variable to the specified new path.
        private static void SetPath(string newPath)
        {
            Environment.SetEnvironmentVariable("Path", newPath, EnvironmentVariableTarget.User);
        }
    }
}
