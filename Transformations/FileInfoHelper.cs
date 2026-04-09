using Transformations;

/// <summary>
/// Extension methods for the FileInfo and FileInfo-Array classes
/// </summary>
/// <remarks>
/// source: https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/FileInfoExtensions.cs
/// </remarks>
public static class FileInfoHelper
{
    #region Methods

    /// <summary>
    /// On Exception enumeration.
    /// </summary>
    public enum OnException
    {
        /// <summary>
        /// Consolidate and throw error.
        /// </summary>
        ConsolidateAndThrowError,

        /// <summary>
        /// Throw each error.
        /// </summary>
        ThrowEachError,

        /// <summary>
        /// Do not error.
        /// </summary>
        DoNotThrowError
    };

    /// <summary>
    /// Changes the files extension.
    /// </summary>
    /// <param name = "file">The file.</param>
    /// <param name = "newExtension">The new extension.</param>
    /// <returns>The renamed file</returns>
    /// <example>
    /// <code>
    /// var file = new FileInfo(@"c:\test.txt");
    /// file.ChangeExtension("xml");
    /// </code>
    /// </example>
    public static FileInfo ChangeExtension(this FileInfo file, string newExtension)
    {
        newExtension = newExtension.EnsureStartsWith(".");
        var fileName = string.Concat(Path.GetFileNameWithoutExtension(file.FullName), newExtension);
        file.Rename(fileName);
        return file;
    }

    /// <summary>
    /// Copies several files to a new folder at once and consolidates any exceptions.
    /// </summary>
    /// <param name = "files">The files.</param>
    /// <param name = "targetPath">The target path.</param>
    /// <returns>The newly created file copies</returns>
    /// <example>
    /// <code>
    /// var files = directory.GetFiles("*.txt", "*.xml");
    /// var copiedFiles = files.CopyTo(@"c:\temp\");
    /// </code>
    /// </example>
    public static FileInfo[] CopyTo(this FileInfo[] files, string targetPath)
    {
        return files.CopyTo(targetPath, OnException.ConsolidateAndThrowError);
    }

    /// <summary>
    /// Copies several files to a new folder at once and optionally consolidates any exceptions.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="targetPath">The target path.</param>
    /// <param name="exceptionOption">The exception option.</param>
    /// <returns>
    /// The newly created file copies
    /// </returns>
    /// <exception cref="Exception">Error while copying one or several files, see InnerExceptions array for details.</exception>
    /// <example>
    ///   <code>
    /// var files = directory.GetFiles("*.txt", "*.xml");
    /// var copiedFiles = files.CopyTo(@"c:\temp\");
    /// </code>
    /// </example>
    public static FileInfo[] CopyTo(this FileInfo[] files, string targetPath, OnException exceptionOption)
    {
        var copiedfiles = new List<FileInfo>();
        List<Exception>? exceptions = null;

        foreach (var file in files)
        {
            try
            {
                var fileName = Path.Combine(targetPath, file.Name);
                copiedfiles.Add(file.CopyTo(fileName));
            }
            catch (Exception e)
            {
                if (exceptionOption == OnException.ThrowEachError)
                {
                    throw;
                }
                else if (exceptionOption == OnException.ConsolidateAndThrowError)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(e);
                }
            }
        }

        if (exceptions != null && exceptions.Count > 0)
        {
            if (exceptions.Count == 1)
            {
                throw new Exception("Errors while copying files. See InnerException for details.", exceptions[0]);
            }
            else
            {
                throw new Exception($"{exceptions.Count} errors while copying files. See InnerExceptions for Example details.", exceptions[0]);
            }
        }

        return copiedfiles.ToArray();
    }

    /// <summary>
    /// Deletes several files at once and consolidates any exceptions.
    /// </summary>
    /// <param name = "files">The files.</param>
    /// <example>
    /// <code>
    /// var files = directory.GetFiles("*.txt", "*.xml");
    /// files.Delete()
    /// </code>
    /// </example>
    public static void Delete(this FileInfo[] files)
    {
        if (files != null && files.Length > 0)
        {
            files.Delete(OnException.ConsolidateAndThrowError);
        }
    }

    /// <summary>
    /// Deletes several files at once and optionally consolidates any exceptions.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="exceptionOption">The exception option.</param>
    /// <exception cref="Exception">
    /// Errors deleting files. See InnerException for details.
    /// or
    /// or
    /// Error deleting files
    /// </exception>
    /// <example>
    ///   <code>
    /// var files = directory.GetFiles("*.txt", "*.xml");
    /// files.Delete()
    /// </code>
    /// </example>
    public static void Delete(this FileInfo[] files, OnException exceptionOption)
    {
        if (exceptionOption == OnException.ConsolidateAndThrowError)
        {
            List<Exception> exceptions = new List<Exception>();

            foreach (var file in files)
            {
                try
                {
                    file.Delete();
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Any())
            {
                if (exceptions.Count == 1)
                {
                    throw new Exception("Errors deleting files. See InnerException for details.", exceptions[0]);
                }
                else
                {
                    throw new Exception($"{exceptions.Count} errors deleting files. See InnerExceptions for Example details.", exceptions[0]);
                }
            }
        }
        else
        {
            foreach (var file in files)
            {
                try
                {
                    file.Delete();
                }
                catch (Exception e)
                {
                    if (exceptionOption == OnException.ThrowEachError)
                    {
                        throw new Exception("Error deleting files", e);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Moves several files to a new folder at once and optionally consolidates any exceptions.
    /// </summary>
    /// <param name = "files">The files.</param>
    /// <param name = "targetPath">The target path.</param>
    /// <returns>The moved files</returns>
    /// <example>
    /// <code>
    /// var files = directory.GetFiles("*.txt", "*.xml");
    /// files.MoveTo(@"c:\temp\");
    /// </code>
    /// </example>
    public static FileInfo[] MoveTo(this FileInfo[] files, string targetPath)
    {
        return files.MoveTo(targetPath, OnException.ConsolidateAndThrowError);
    }

    /// <summary>
    /// Movies several files to a new folder at once and optionally consolidates any exceptions.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="targetPath">The target path.</param>
    /// <param name="exceptionOption">The exception option.</param>
    /// <returns>
    /// The moved files
    /// </returns>
    /// <exception cref="Exception">
    /// Error moving files. See InnerException for details.
    /// or
    /// </exception>
    /// <example>
    ///   <code>
    /// var files = directory.GetFiles("*.txt", "*.xml");
    /// files.MoveTo(@"c:\temp\");
    /// </code>
    /// </example>
    public static FileInfo[] MoveTo(this FileInfo[] files, string targetPath, OnException exceptionOption)
    {
        List<Exception>? exceptions = null;

        foreach (var file in files)
        {
            try
            {
                var fileName = Path.Combine(targetPath, file.Name);
                file.MoveTo(fileName);
            }
            catch (Exception e)
            {
                if (exceptionOption == OnException.ConsolidateAndThrowError)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(e);
                }
                else if (exceptionOption == OnException.ThrowEachError)
                {
                    throw;
                }
            }
        }

        if (exceptions != null && exceptions.Count > 0)
        {
            if (exceptions.Count == 1)
            {
                throw new Exception("Error moving files. See InnerException for details.", exceptions[0]);
            }
            else
            {
                throw new Exception($"{exceptions.Count} errors moving files. See InnerExceptions for Example details.", exceptions[0]);
            }
        }

        return files;
    }

    /// <summary>
    /// Renames a file.
    /// </summary>
    /// <param name = "file">The file.</param>
    /// <param name = "newName">The new name.</param>
    /// <returns>The renamed file</returns>
    /// <example>
    /// <code>
    /// var file = new FileInfo(@"c:\test.txt");
    /// file.Rename("test2.txt");
    /// </code>
    /// </example>
    public static FileInfo Rename(this FileInfo file, string newName)
    {
        if (!string.IsNullOrEmpty(newName))
        {
            var filePath = Path.Combine(Path.GetDirectoryName(file.FullName) ?? string.Empty, newName);
            file.MoveTo(filePath);
        }

        return file;
    }

    /// <summary>
    ///  Renames a without changing its extension.
    /// </summary>
    /// <param name = "file">The file.</param>
    /// <param name = "newName">The new name.</param>
    /// <returns>The renamed file</returns>
    /// <example>
    /// <code>
    /// var file = new FileInfo(@"c:\test.txt");
    /// file.RenameFileWithoutExtension("test3");
    /// </code>
    /// </example>
    public static FileInfo RenameFileWithoutExtension(this FileInfo file, string newName)
    {
        if (!string.IsNullOrEmpty(newName))
        {
            var fileName = string.Concat(newName, file.Extension);
            file.Rename(fileName);
        }

        return file;
    }

    /// <summary>
    /// Sets file attributes for several files at once
    /// </summary>
    /// <param name = "files">The files.</param>
    /// <param name = "attributes">The attributes to be set.</param>
    /// <returns>The changed files</returns>
    /// <example>
    /// <code>
    /// var files = directory.GetFiles("*.txt", "*.xml");
    /// files.SetAttributes(FileAttributes.Archive);
    /// </code>
    /// </example>
    public static FileInfo[] SetAttributes(this FileInfo[] files, FileAttributes attributes)
    {
        if (files != null && files.Length > 0)
        {
            foreach (var file in files)
            {
                file.Attributes = attributes;
            }
        }

        return files!;
    }

    /// <summary>
    /// Appends file attributes for several files at once (additive to any existing attributes)
    /// </summary>
    /// <param name = "files">The files.</param>
    /// <param name = "attributes">The attributes to be set.</param>
    /// <returns>The changed files</returns>
    /// <example>
    /// <code>
    /// var files = directory.GetFiles("*.txt", "*.xml");
    /// files.SetAttributesAdditive(FileAttributes.Archive);
    /// </code>
    /// </example>
    public static FileInfo[] SetAttributesAdditive(this FileInfo[] files, FileAttributes attributes)
    {
        if (files != null && files.Length > 0)
        {
            foreach (var file in files)
            {
                file.Attributes = file.Attributes | attributes;
            }
        }

        return files!;
    }

    #endregion Methods
}