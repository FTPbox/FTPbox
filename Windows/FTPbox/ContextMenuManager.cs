using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FTPboxLib;
using Microsoft.Win32;

namespace FTPbox
{
    internal static class ContextMenuManager
    {
        private static DateTime _dtLastContextAction = DateTime.Now;

        public static bool IsServerRunning => Directory.GetFiles(@"\\.\pipe\").Any(x => x.Contains("FTPboxServer"));

        /// <summary>
        ///     Connect to our named-pipe server, send arguements and exit
        /// </summary>
        public static void RunClient(string file, string param)
        {
            if (!IsServerRunning)
            {
                MessageBox.Show("FTPbox must be running to use the context menus!", "FTPbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RemoveContextMenu();
                Process.GetCurrentProcess().Kill();
            }

            var pipeClient = new NamedPipeClientStream(".", "FTPboxServer", PipeDirection.Out, PipeOptions.None,
                System.Security.Principal.TokenImpersonationLevel.Impersonation);
            
            pipeClient.Connect();
            Log.Write(l.Info, "[PipeClient] Connected");

            using (var writer = new StreamWriter(pipeClient))
            {
                var message = $"{param} {file}";

                writer.WriteAsync(message).ContinueWith(s =>
                {
                    Log.Write(l.Info, "[PipeClient] Message sent");

                    pipeClient.Close();
                    Process.GetCurrentProcess().Kill();
                });
            }
        }

        /// <summary>
        ///     Run the named-pipe server and wait for clients
        /// </summary>
        public static async void RunServer()
        {
            Log.Write(l.Info, "[PipeServer] Starting up");

            while (true)
            {

                var pipeServer = new NamedPipeServerStream("FTPboxServer", PipeDirection.In, 5,
                    PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                var threadID = Thread.CurrentThread.ManagedThreadId;

                await Task.Factory.FromAsync(pipeServer.BeginWaitForConnection, pipeServer.EndWaitForConnection, null)
                    .ContinueWith(s => Log.Write(l.Client, "[PipeServer] Connection from {0}", threadID));

                var reader = new StreamReader(pipeServer);

                var message = await reader.ReadToEndAsync();

                var action = message.Substring(0, 4);
                var file = message.Substring(5);

                Log.Write(l.Info, $"[PipeServer] Action: {action} File: {file}");

                switch (action)
                {
                    case "copy":
                        CopyArgLink(file);
                        break;
                    case "sync":
                        SyncArgItem(file);
                        break;
                    case "open":
                        OpenArgItemInBrowser(file);
                        break;
                    case "move":
                        await MoveArgItem(file);
                        break;
                }

                if (pipeServer.IsConnected)
                {
                    pipeServer.Disconnect();
                }
            }
        }

        /// <summary>
        ///     Called when 'Copy HTTP link' is clicked from the context menus
        /// </summary>
        private static void CopyArgLink(string path)
        {
            if (!path.StartsWith(Program.Account.Paths.Local))
            {
                MessageBox.Show("You cannot use this for files that are not inside the FTPbox folder.",
                    "FTPbox - Invalid file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var link = Program.Account.GetHttpLink(path);

                // Append the link to clipboard?
                if ((DateTime.Now - _dtLastContextAction).TotalSeconds < 2)
                    Clipboard.SetText(Clipboard.GetText() + Environment.NewLine + link);
                else
                    Clipboard.SetText(link);
            }
            catch (Exception e)
            {
                Common.LogError(e);
            }
            _dtLastContextAction = DateTime.Now;
        }

        /// <summary>
        ///     Called when 'Synchronize this file/folder' is clicked from the context menus
        /// </summary>
        /// <param name="path">path to file or folder</param>
        private static async void SyncArgItem(string path)
        {
            if (!path.StartsWith(Program.Account.Paths.Local))
            {
                MessageBox.Show("You cannot use this for files that are not inside the FTPbox folder.",
                    "FTPbox - Invalid file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var cpath = Program.Account.GetCommonPath(path, true);
            var exists = Program.Account.Client.Exists(cpath);

            if (Common.PathIsFile(path) && File.Exists(path))
            {
                await Program.Account.SyncQueue.Add(new SyncQueueItem(Program.Account)
                {
                    Item = new ClientItem
                    {
                        FullPath = path,
                        Name = Common._name(cpath),
                        Type = ClientItemType.File,
                        Size = exists ? Program.Account.Client.SizeOf(cpath) : new FileInfo(path).Length,
                        LastWriteTime = exists ? Program.Account.Client.TryGetModifiedTime(cpath) : File.GetLastWriteTime(path)
                    },
                    ActionType = ChangeAction.changed,
                    SyncTo = exists ? SyncTo.Local : SyncTo.Remote
                });
            }
            else if (!Common.PathIsFile(path) && Directory.Exists(path))
            {
                var di = new DirectoryInfo(path);
                await Program.Account.SyncQueue.Add(new SyncQueueItem(Program.Account)
                {
                    Item = new ClientItem
                    {
                        FullPath = di.FullName,
                        Name = di.Name,
                        Type = ClientItemType.Folder,
                        Size = 0x0,
                        LastWriteTime = DateTime.MinValue
                    },
                    ActionType = ChangeAction.changed,
                    SyncTo = exists ? SyncTo.Local : SyncTo.Remote,
                    SkipNotification = true
                });
            }
        }

        /// <summary>
        ///     Open the link to a file in browser when 
        ///     'Open in browser' is clicked from the context menus
        /// </summary>
        public static void OpenArgItemInBrowser(string path)
        {
            var link = Program.Account.GetHttpLink(path);
            try
            {
                Process.Start(link);
            }
            catch (Exception e)
            {
                Common.LogError(e);
            }
        }

        /// <summary>
        ///     Move all items in path to the local folder when 
        ///     'Move to FTPbox folder' is clicked from the context menus
        /// </summary>
        public static async Task MoveArgItem(string path)
        {
            if (path.StartsWith(Program.Account.Paths.Local)) return;

            if (File.Exists(path))
            {
                await CopyFileAsync(path);
            }
            else if (Directory.Exists(path))
            {
                foreach (var dir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
                {
                    var name = dir.Substring(path.Length);
                    Directory.CreateDirectory(Path.Combine(Program.Account.Paths.Local, name));
                }

                var copyTasks =
                    Directory.GetFiles(path, "*", SearchOption.AllDirectories).Select(CopyFileAsync);

                await Task.WhenAll(copyTasks);
            }
        }

        /// <summary>
        ///     Asynchronously copies the given file to the local folder
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns></returns>
        private static async Task CopyFileAsync(string path)
        {
            using (var source = File.Open(path, FileMode.Open))
            {
                var name = new FileInfo(path).Name;
                var destPath = Path.Combine(Program.Account.Paths.Local, name);
                using (var destination = File.Create(destPath))
                {
                    await source.CopyToAsync(destination);
                }
            }
        }

        public static void AddContextMenu()
        {
            var list = new List<ContextMenuItem>()
            {
                new ContextMenuItem()
                {
                    RegistryPath = "FTPbox",
                    IconPath = $"\"{Path.Combine(Application.StartupPath, "ftpboxnew.ico")}\"",
                    MuiVerb = "FTPbox",
                    SubCommands = true
                },
                new ContextMenuItem()
                {
                    RegistryPath = "FTPbox\\Shell\\Copy",
                    MuiVerb = "Copy HTTP link",
                    AppliesTo = GetAppliesTo(false),
                    Command = $"\"{Application.ExecutablePath}\" \"%1\" \"copy\""
                },
                new ContextMenuItem()
                {
                    RegistryPath = "FTPbox\\Shell\\Open",
                    MuiVerb = "Open file in browser",
                    AppliesTo = GetAppliesTo(false),
                    Command = $"\"{Application.ExecutablePath}\" \"%1\" \"open\""
                },
                new ContextMenuItem()
                {
                    RegistryPath = "FTPbox\\Shell\\Sync",
                    MuiVerb = "Synchronize this file",
                    AppliesTo = GetAppliesTo(false),
                    Command = $"\"{Application.ExecutablePath}\" \"%1\" \"sync\""
                },
                new ContextMenuItem()
                {
                    RegistryPath = "FTPbox\\Shell\\Move",
                    MuiVerb = "Move to FTPbox folder",
                    AppliesTo = GetAppliesTo(true),
                    Command = $"\"{Application.ExecutablePath}\" \"%1\" \"move\""
                }
            };

            foreach (var rootkey in new[] { "Software\\Classes\\*\\Shell", "Software\\Classes\\Directory\\Shell" })
            {
                foreach (var item in list)
                {
                    var keyPath = $"{rootkey}\\{item.RegistryPath}";
                    Registry.CurrentUser.CreateSubKey(keyPath);
                    var key = Registry.CurrentUser.OpenSubKey(keyPath, true);

                    key?.SetValue("MUIVerb", item.MuiVerb);

                    if (!string.IsNullOrEmpty(item.IconPath))
                    {
                        key?.SetValue("Icon", item.IconPath);
                    }
                    if (item.SubCommands)
                    {
                        key?.SetValue("SubCommands", "");
                    }
                    if (!string.IsNullOrEmpty(item.AppliesTo))
                    {
                        key?.SetValue("AppliesTo", item.AppliesTo);
                    }

                    if (!string.IsNullOrEmpty(item.Command))
                    {
                        key?.CreateSubKey("Command");
                        key = key?.OpenSubKey("Command", true);
                        key?.SetValue("", item.Command);
                    }

                    key?.Close();
                }
            }

        }

        /// <summary>
        ///     Remove the FTPbox context menu (delete the registry files).
        ///     Called when application is exiting.
        /// </summary>
        public static void RemoveContextMenu()
        {
            var key = Registry.CurrentUser.OpenSubKey("Software\\Classes\\*\\Shell\\", true);
            key?.DeleteSubKeyTree("FTPbox", false);
            key?.Close();

            key = Registry.CurrentUser.OpenSubKey("Software\\Classes\\Directory\\Shell\\", true);
            key?.DeleteSubKeyTree("FTPbox", false);
            key?.Close();
        }

        /// <summary>
        ///     Gets the value of the AppliesTo String Value that will be put to registry and determine on which files' right-click
        ///     menus each FTPbox menu item will show.
        ///     If the local path is inside a library folder, it has to check for another path (short_path), because
        ///     System.ItemFolderPathDisplay will, for example, return
        ///     Documents\FTPbox instead of C:\Users\Username\Documents\FTPbox
        /// </summary>
        /// <param name="isForMoveItem">
        ///     If the AppliesTo value is for the Move-to-FTPbox item, it adds 'NOT' to make sure it shows
        ///     anywhere but in the local syncing folder.
        /// </param>
        /// <returns></returns>
        private static string GetAppliesTo(bool isForMoveItem)
        {
            var path = Program.Account.Paths.Local;
            var appliesTo = (isForMoveItem)
                ? $"NOT System.ItemFolderPathDisplay:~< \"{path}\""
                : $"System.ItemFolderPathDisplay:~< \"{path}\"";

            var libraries = new[]
            {
                Environment.SpecialFolder.MyDocuments, Environment.SpecialFolder.MyMusic,
                Environment.SpecialFolder.MyPictures, Environment.SpecialFolder.MyVideos
            };
            var userpath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\";

            if (path.StartsWith(userpath) &&
                libraries.Any(s => path.StartsWith(Environment.GetFolderPath(s)) && s != Environment.SpecialFolder.UserProfile))
            {
                var shortPath = path.Substring(userpath.Length);

                appliesTo += (isForMoveItem)
                    ? $" AND NOT System.ItemFolderPathDisplay: \"*{shortPath}*\""
                    : $" OR System.ItemFolderPathDisplay: \"*{shortPath}*\"";
            }

            return appliesTo;
        }
    }
}

internal sealed class ContextMenuItem
{
    public string RegistryPath;

    public string MuiVerb;

    public string AppliesTo;

    public string IconPath;

    public string Command;

    public bool SubCommands;
}