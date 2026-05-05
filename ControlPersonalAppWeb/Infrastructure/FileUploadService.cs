using System;
using System.IO;
using System.Linq;
using System.Web;

namespace ControlPersonalAppWeb.Infrastructure
{
    public static class FileUploadService
    {
        private static readonly string[] AllowedExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".csv", ".xlsx", ".xls", ".pdf" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        public static string SaveFile(HttpPostedFileBase file, string subFolder)
        {
            if (file == null || file.ContentLength == 0)
                return null;

            if (file.ContentLength > MaxFileSize)
                throw new InvalidOperationException("El archivo excede el tamaño máximo permitido (10 MB).");

            string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
                throw new InvalidOperationException($"Extensión de archivo no permitida: {extension}");

            // Sanitize filename: remove path components, use GUID to avoid collisions
            string safeFileName = Guid.NewGuid().ToString("N") + extension;

            string basePath = AppSettings.FileStoragePath;
            string fullDir = Path.Combine(basePath, subFolder);

            // Prevent path traversal in subFolder
            string resolvedDir = Path.GetFullPath(fullDir);
            string resolvedBase = Path.GetFullPath(basePath);
            if (!resolvedDir.StartsWith(resolvedBase, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Ruta de destino no válida.");

            if (!Directory.Exists(resolvedDir))
                Directory.CreateDirectory(resolvedDir);

            string filePath = Path.Combine(resolvedDir, safeFileName);
            file.SaveAs(filePath);

            return safeFileName;
        }

        public static string GetOriginalFileName(HttpPostedFileBase file)
        {
            if (file == null) return null;
            return Path.GetFileName(file.FileName); // strips path components
        }
    }
}
