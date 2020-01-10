namespace XamlMissingXMLNamespaces.Storage
{
    using System;
    using System.IO;

    /// <summary>
    /// Defines a helper class for file storage.
    /// </summary>
    public static class FileStorageHelper
    {
        /// <summary>
        /// Creates a valid path for a file in the application's domain at the root with the given file name, if the file does not exist.
        /// </summary>
        /// <param name="fileName">
        /// The name of the file to create.
        /// </param>
        /// <returns>
        /// The path to the file.
        /// </returns>
        public static string CreatePathForApplicationFile(string fileName)
        {
            return CreatePathForApplicationFile(fileName, default(string));
        }

        /// <summary>
        /// Creates a valid path for a file in the application's domain in the given folder with the given file name, if the file does not exist.
        /// </summary>
        /// <param name="fileName">
        /// The name of the file to create.
        /// </param>
        /// <param name="folderName">
        /// The name of the folder.
        /// </param>
        /// <returns>
        /// The path to the file.
        /// </returns>
        public static string CreatePathForApplicationFile(string fileName, string folderName)
        {
            string appFolderPath = AppDomain.CurrentDomain.BaseDirectory;
            string reportFolderPath = Path.Combine(appFolderPath, folderName);

            if (string.IsNullOrWhiteSpace(reportFolderPath))
            {
                return string.Empty;
            }

            if (!Directory.Exists(reportFolderPath))
            {
                Directory.CreateDirectory(reportFolderPath);
            }

            return Path.Combine(reportFolderPath, fileName);
        }
    }
}