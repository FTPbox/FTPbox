using System;
using System.Collections.Generic;
using System.IO;

namespace FTPboxLib
{
    public abstract class SyncFilter
    {
        public abstract bool IsIgnored(ClientItem item);

        public virtual bool IsIgnored(FileInfo fInfo)
        {
            var cItem = new ClientItem(fInfo);
            return IsIgnored(cItem);
        }
    }

    public class ExtensionFilter : SyncFilter
    {
        List<string> IgnoredExtensions;

        public ExtensionFilter(List<string> extensions)
        {
            IgnoredExtensions = extensions;
        }

        public override bool IsIgnored(ClientItem item)
        {
            var name = item.Name;

            var ext = name.Contains(".")
                ? name.Substring(name.LastIndexOf(".", StringComparison.Ordinal) + 1)
                : string.Empty;

            if (string.IsNullOrEmpty(ext))
                return false;

            if (IgnoredExtensions.Contains(ext) || IgnoredExtensions.Contains("." + ext))
            {
                Log.Write(l.Debug, $"File ignored because of its extension: {item.Name}");
                return true;
            }
            return false;
        }
    }

    public class DateFilter : SyncFilter
    {
        DateTime MinimumLastModified;

        public DateFilter(DateTime threshold)
        {
            MinimumLastModified = threshold;
        }

        public override bool IsIgnored(ClientItem item)
        {
            if (item.LastWriteTime < MinimumLastModified)
            {
                Log.Write(l.Debug, $"File ignored because it is older than {MinimumLastModified}: {item.FullPath}");
                return true;
            }
            return false;
        }

        public override bool IsIgnored(FileInfo fInfo) => fInfo.Exists && base.IsIgnored(fInfo);
    }

    public class CustomFilter : SyncFilter
    {
        bool IgnoreDotFiles;
        bool IgnoreTempFiles;

        public CustomFilter(bool dotfiles, bool tempfiles)
        {
            IgnoreDotFiles = dotfiles;
            IgnoreTempFiles = tempfiles;
        }

        public override bool IsIgnored(ClientItem item)
        {
            var name = item.Name;

            // are dotfiles ignored?
            if (IgnoreDotFiles && name.StartsWith("."))
            {
                Log.Write(l.Debug, $"File ignored because it is a dotfile: {item.FullPath}");
                return true;
            }
            // are temporary files ignored?
            if (IgnoreTempFiles && (name.ToLower().EndsWith(".tmp") || name.EndsWith("~") || name.StartsWith(".goutputstream") || name.StartsWith("~") || name.Equals("Thumbs.db")))
            {
                Log.Write(l.Debug, $"File ignored because it is a temp file: {item.FullPath}");
                return true;
            }

            return false;
        }
    }
}
