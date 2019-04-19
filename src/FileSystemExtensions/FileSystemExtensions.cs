using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace FileSystemExtensions
{
    public static class MyFileSystemExtensions
    {
        public static string FullNameOrNull(this FileSystemInfo info)
        {
            try
            {
                return info.FullName;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void SafeFullNameDo(this FileSystemInfo info, Action<string> action)
        {
            try
            {
                string name = info.FullName;
                if (name != null) { action(info.FullName); }
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        public static string NormalizedPath(this FileSystemInfo info)
        {
            // Code taken from: http://stackoverflow.com/a/21058121
            return Path.GetFullPath(new Uri(info.FullName).LocalPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .ToUpperInvariant();
        }

        public static bool PathEqual(this FileSystemInfo info, FileSystemInfo other)
        {
            try
            {
                return info.NormalizedPath().Equals(other.NormalizedPath());
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsRoot(this DirectoryInfo directory)
        {
            return directory.PathEqual(directory.Root);
        }

        public static DriveInfo Drive(this DirectoryInfo directory)
        {
            DirectoryInfo root = directory.Root;
            return DriveInfo.GetDrives()
                .FirstOrDefault((drive) => drive.Name.Equals(root.Name, StringComparison.OrdinalIgnoreCase));
        }

        public static DriveInfo NextSibling(this DriveInfo drive)
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            int index = Array.FindIndex(drives, (d) => d.Name.Equals(drive.Name, StringComparison.OrdinalIgnoreCase));
            return drives.ElementAtOrDefault(index + 1);
        }

        public static DirectoryInfo NextSibling(this DirectoryInfo directory)
        {
            DirectoryInfo parent;
            try
            {
                parent = directory.Parent;
            }
            catch (Exception)
            {
                parent = null;
            }
            if (parent == null)
            {
                DriveInfo drive = directory.Drive().NextSibling();
                return drive == null ? null : new DirectoryInfo(drive.Name);
            }
            IEnumerable<DirectoryInfo> siblings = parent.EnumerateDirectories();
            int nextIndex = 0;
            int i = 0;
            foreach (DirectoryInfo d in siblings)
            {
                if (d.PathEqual(directory))
                {
                    nextIndex = i + 1;
                    break;
                }
                i++;
            }
            do
            {
                DirectoryInfo next = siblings.ElementAtOrDefault(nextIndex++);
                if (next == null)
                {
                    // We reach the end with no luck
                    return null;
                }
                else if (next.FullNameOrNull() != null)
                {
                    // We found a valid directory
                    return next;
                }
            } while (true);
        }

        public static bool Contains(this DirectoryInfo root, DirectoryInfo directory)
        {
            try
            {
                DirectoryInfo dir = directory.Parent;
                while (dir != null)
                {
                    if (dir.PathEqual(root)) return true;
                    dir = dir.Parent;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// This method is similar to the base DirectoryInfo.EnumerateFiles(..) but the difference
        /// is that it will hide any exception occurred while traversing the file system and simply
        /// continue with the next directory.
        /// 
        /// The process is completely iterative. But beware, it can take a very long time to traverse
        /// a big directory (specially if it contains many subdirectories). This override doesn't allow 
        /// you to specify which directories to include/exclude from the search so if you ask it to 
        /// enumerate all the files in C:\ it will do exactly that. You have been warned!
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static IEnumerable<FileInfo> SafeEnumerateFiles(
            this DirectoryInfo dir,
            string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return SafeEnumerateFiles(dir, new DirectoryInfo[0], searchPattern, searchOption);
        }

        /// <summary>
        /// This method is similar to the base DirectoryInfo.EnumerateFiles(..) but the difference
        /// is that it will hide any exception occurred while traversing the file system and simply
        /// continue with the next directory.
        /// 
        /// The process is completely iterative. But beware, it can take a very long time to traverse
        /// a big directory (specially if it contains many subdirectories).
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="exclusions"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static IEnumerable<FileInfo> SafeEnumerateFiles(
            this DirectoryInfo dir,
            IEnumerable<DirectoryInfo> exclusions,
            string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            DirectoryInfo current = dir;
            while (current != null)
            {
                var excluded = exclusions.Any(exc => exc.PathEqual(current) || exc.Contains(current));
                if (!excluded)
                {
                    IEnumerable<FileInfo> files = null;
                    try
                    {
                        files = current.EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly)
                            .Where((file) => file.FullNameOrNull() != null);
                    }
                    catch (Exception)
                    {
                        // Do nothing
                    }

                    if (files != null)
                    {
                        foreach (FileInfo file in files)
                        {
                            yield return file;
                        }
                    }
                }

                if (searchOption == SearchOption.TopDirectoryOnly)
                {
                    current = null;
                }
                else
                {
                    DirectoryInfo next = null;

                    // If the current dir is excluded we shouldn't go inside
                    if (!excluded)
                    {
                        // Try with the first subdirectory
                        try
                        {
                            next = current.EnumerateDirectories("*", SearchOption.TopDirectoryOnly)
                                .FirstOrDefault((each) => each.FullNameOrNull() != null);
                        }
                        catch (Exception) { /* Do nothing */ }
                    }

                    if (next == null)
                    {
                        /*
                        If the current directory is excluded or it doesn't have any subdirectories, 
                        we should try with the next sibling.
                        If the next sibling is null, it means we are the last sibling, so we should
                        walk up the hierarchy until we find our parent's next sibling.
                        */
                        DirectoryInfo parent = current;
                        while (parent != null)
                        {
                            try
                            {
                                next = parent.NextSibling();
                                if (next == null)
                                {
                                    // Walk up
                                    parent = parent.Parent;
                                }
                                else
                                {
                                    // Sibling found! Break out of the loop.
                                    parent = null;
                                }
                            }
                            catch (Exception)
                            {
                                // An error occurred. Break out of the loop.
                                //
                                parent = next = null;
                            }
                        }
                    }

                    // If the next directory is not inside the root, then stop here
                    if (next != null && dir.Contains(next))
                    {
                        current = next;
                    }
                    else
                    {
                        current = null;
                    }
                }
            }
        }

        /// <summary>
        /// Code taken from: http://stackoverflow.com/a/937558
        /// 
        /// Beware that even though you can test if a file is locked before opening
        /// it, the file could get locked between the moment you test and you try
        /// to open it. So you can still get an exception.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsLocked(this FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}
