using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DHAFacilitationAPIs.Application.Common.Interfaces;
using DHAFacilitationAPIs.Application.Common.Models; // <-- added to resolve FileStorageOptions
using DHAFacilitationAPIs.Application.Interface.Service;
using DHAFacilitationAPIs.Application.ViewModels;
using DHAFacilitationAPIs.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;

namespace DHAFacilitationAPIs.Infrastructure.Service;

public class FileStorageService : IFileStorageService
{
    private static readonly string[] DefaultAllowedExt = new[] { ".jpg", ".jpeg", ".png", ".webp", ".mp3", ".aac", ".pdf" };
    private readonly IWebHostEnvironment _env;   // <-- add this
    private readonly FileStorageOptions _opt;
    private readonly FileExtensionContentTypeProvider _mime = new();
    public FileStorageService(IWebHostEnvironment env, IOptions<FileStorageOptions> opt) // <-- inject here
    {
        _env = env;
        _opt = opt.Value;
    }

    // Simple overload → validated overload with defaults
    public Task<string> SaveFileAsync(IFormFile file, string folderName, CancellationToken ct)
        => SaveFileAsync(file, folderName, ct, 10 * 1024 * 1024, DefaultAllowedExt);
    public async Task<string> SaveFileNonMemeberAsync(
     IFormFile file,
     string folderName,
     CancellationToken ct,
     long maxBytes = 10 * 1024 * 1024,           // default 10 MB (adjust as you like)
     string[]? allowedExtensions = null)         // e.g. new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }
    {
        // Basic validations
        if (file is null || file.Length == 0)
            throw new ArgumentException("Empty file.");

        if (file.Length > maxBytes)
            throw new InvalidOperationException($"File exceeds {maxBytes / (1024 * 1024)} MB limit.");

        // Extension checks
        var ext = Path.GetExtension(file.FileName);
        var allowed = allowedExtensions ?? DefaultAllowedExt; // keep your existing default list
        if (allowed is not null && !allowed.Contains(ext, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Extension '{ext}' not allowed.");

        // MIME check (based on filename mapping)
        if (_mime.TryGetContentType(file.FileName, out var mappedType))
        {
            if (!mappedType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Only image uploads are allowed. Detected: {mappedType}");
        }

        // Base physical path: <WebRoot>\uploads
        var webRoot = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            throw new InvalidOperationException("WebRootPath is not configured.");

        var baseUploads = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(baseUploads);

        // Normalize/sanitize folderName (prevent traversal)
        var relFolder = (folderName ?? string.Empty).Trim().TrimStart('/', '\\');
        if (relFolder.Contains("..", StringComparison.Ordinal))
            throw new InvalidOperationException("Invalid folder name.");

        var absFolder = string.IsNullOrEmpty(relFolder) ? baseUploads : Path.Combine(baseUploads, relFolder);
        Directory.CreateDirectory(absFolder);

        // Generate a unique file name and save
        var safeExt = ext.ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{safeExt}";
        var absPath = Path.Combine(absFolder, fileName);

        await using (var stream = new FileStream(absPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true))
            await file.CopyToAsync(stream, ct);

        // Return a web-relative path like: "uploads/{folder}/{file}"
        var relPath = string.IsNullOrEmpty(relFolder)
            ? Path.Combine("uploads", fileName)
            : Path.Combine("uploads", relFolder, fileName);

        // Normalize to URL-style slashes
        relPath = relPath.Replace('\\', '/').Replace("//", "/");
        return relPath;
    }

    public async Task<string> SaveFileAsync(
    IFormFile file,
    string folderName,
    CancellationToken ct,
    long maxBytes,
    string[]? allowedExtensions)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("Empty file.");
        if (file.Length > maxBytes)
            throw new InvalidOperationException($"File exceeds {maxBytes / (1024 * 1024)} MB limit.");

        var ext = Path.GetExtension(file.FileName);
        var allowed = allowedExtensions ?? DefaultAllowedExt;
        if (!allowed.Contains(ext, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Extension '{ext}' not allowed.");

        if (_mime.TryGetContentType(file.FileName, out var mappedType))
        {
            if (!mappedType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Only image uploads are allowed. Detected: {mappedType}");
        }

        // Physical base = <ContentRoot>\{RequestPathTrimmed}  e.g., C:\...\YourApp\CBMS
        var baseDir = _env.ContentRootPath;
        if (string.IsNullOrWhiteSpace(baseDir))
            baseDir = AppContext.BaseDirectory;

        var requestFolder = string.IsNullOrWhiteSpace(_opt.RequestPath)
            ? "CBMS"
            : _opt.RequestPath.Trim('/', '\\'); // "CBMS"

        var basePhysical = Path.Combine(baseDir, requestFolder);
        Directory.CreateDirectory(basePhysical);

        // Subfolder under CBMS (e.g., rooms/{roomId})
        var relFolder = (folderName ?? string.Empty).Trim().TrimStart('/', '\\');
        var absFolder = string.IsNullOrEmpty(relFolder) ? basePhysical : Path.Combine(basePhysical, relFolder);
        Directory.CreateDirectory(absFolder);

        // Save file
        var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var absPath = Path.Combine(absFolder, fileName);

        await using (var stream = new FileStream(absPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true))
            await file.CopyToAsync(stream, ct);

        // Public relative URL under /CBMS
        var relUrl = $"/{requestFolder}/{(string.IsNullOrEmpty(relFolder) ? "" : relFolder.Replace('\\', '/') + "/")}{fileName}";
        // collapse any accidental double slashes
        relUrl = relUrl.Replace("//", "/");
        return relUrl;
    }

    public async Task<string> SaveAudioAsync(
    IFormFile file,
    string folderName,
    CancellationToken ct,
    long maxBytes = 10 * 1024 * 1024,
    string[]? allowedExtensions = null)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Empty audio file.");

        if (file.Length > maxBytes)
            throw new InvalidOperationException($"Audio file exceeds {maxBytes / (1024 * 1024)} MB limit.");

        // ✅ Allow only mp3 & aac
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = allowedExtensions ?? new[] { ".mp3", ".aac" };

        if (!allowed.Contains(ext))
            throw new InvalidOperationException($"Audio extension '{ext}' not allowed.");

        // ✅ Validate MIME is audio/*
        if (_mime.TryGetContentType(file.FileName, out var mappedType))
        {
            if (!mappedType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Only audio uploads allowed. Detected: {mappedType}");
        }

        // ===== SAME STORAGE LOGIC AS IMAGE =====

        var baseDir = _env.ContentRootPath;
        if (string.IsNullOrWhiteSpace(baseDir))
            baseDir = AppContext.BaseDirectory;

        var requestFolder = string.IsNullOrWhiteSpace(_opt.RequestPath)
            ? "CBMS"
            : _opt.RequestPath.Trim('/', '\\');

        var basePhysical = Path.Combine(baseDir, requestFolder);
        Directory.CreateDirectory(basePhysical);

        var relFolder = (folderName ?? string.Empty).Trim().TrimStart('/', '\\');
        var absFolder = string.IsNullOrEmpty(relFolder)
            ? basePhysical
            : Path.Combine(basePhysical, relFolder);

        Directory.CreateDirectory(absFolder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var absPath = Path.Combine(absFolder, fileName);

        await using (var stream = new FileStream(
            absPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            64 * 1024,
            useAsync: true))
        {
            await file.CopyToAsync(stream, ct);
        }

        var relUrl = $"/{requestFolder}/{(string.IsNullOrEmpty(relFolder) ? "" : relFolder.Replace('\\', '/') + "/")}{fileName}";
        return relUrl.Replace("//", "/");
    }

    public async Task<List<string>> SaveFilesAsync(
        IEnumerable<IFormFile> files,
        string folderName,
        CancellationToken ct,
        long maxBytes = 10 * 1024 * 1024,
        string[]? allowedExtensions = null)
    {
        var results = new List<string>();
        foreach (var f in files)
            results.Add(await SaveFileAsync(f, folderName, ct, maxBytes, allowedExtensions));
        return results;
    }

    // ✅ Save single complaint file
    public async Task<string> SaveComplaintFileAsync(
    IFormFile file,
    Guid complaintId, // still passed for naming, if needed
    CancellationToken ct,
    long maxBytes = 10 * 1024 * 1024,
    string[]? allowedExtensions = null)
    {
        // 🧩 Basic validation
        if (file is null || file.Length == 0)
            throw new ArgumentException("Empty file.");

        if (file.Length > maxBytes)
            throw new InvalidOperationException($"File exceeds {maxBytes / (1024 * 1024)} MB limit.");

        // 🧩 Validate extension
        var ext = Path.GetExtension(file.FileName);
        var allowed = allowedExtensions ?? new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(ext, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Extension '{ext}' not allowed.");

        // 🧩 Validate MIME type (must be image)
        if (_mime.TryGetContentType(file.FileName, out var mappedType))
        {
            if (!mappedType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Only image uploads are allowed. Detected: {mappedType}");
        }

        // 🧩 Base folder: wwwroot/uploads/complaints/
        var webRoot = _env.WebRootPath ?? throw new InvalidOperationException("WebRootPath is not configured.");
        var absFolder = Path.Combine(webRoot, "uploads", "complaints");
        Directory.CreateDirectory(absFolder);

        // 🧩 Unique file name (optionally prefix complaint ID)
        var safeExt = ext.ToLowerInvariant();
        var fileName = $"{complaintId:N}_{Guid.NewGuid():N}{safeExt}";

        var absPath = Path.Combine(absFolder, fileName);

        // 🧩 Save asynchronously
        await using (var stream = new FileStream(absPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true))
            await file.CopyToAsync(stream, ct);

        // 🧩 Return relative path like: "uploads/complaints/filename.jpg"
        var relPath = Path.Combine("uploads", "complaints", fileName)
            .Replace("\\", "/");

        return relPath;
    }

    // ✅ Save multiple complaint files
    public async Task<List<string>> SaveComplaintFilesAsync(
        IEnumerable<IFormFile> files,
        Guid complaintId,
        CancellationToken ct,
        long maxBytes = 10 * 1024 * 1024,
        string[]? allowedExtensions = null)
    {
        if (files is null || !files.Any())
            throw new ArgumentException("No files provided.");

        var results = new List<string>();
        foreach (var file in files)
        {
            var path = await SaveComplaintFileAsync(file, complaintId, ct, maxBytes, allowedExtensions);
            results.Add(path);
        }
        return results;
    }

    public async Task<bool> DeleteFileAsync(string relativePath, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return false;

        // Accept with/without RequestPath prefix
        var path = relativePath.Replace('\\', '/').Trim();
        if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            // strip base url if someone passed absolute
            var baseUrl = (_opt.PublicBaseUrl ?? string.Empty).TrimEnd('/');
            if (!string.IsNullOrEmpty(baseUrl) && path.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
                path = path.Substring(baseUrl.Length);
        }
        if (path.StartsWith(_opt.RequestPath, StringComparison.OrdinalIgnoreCase))
            path = path.Substring(_opt.RequestPath.Length);

        path = path.TrimStart('/');

        var full = Path.Combine(_opt.RootPath, path.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(full)) return false;

        ct.ThrowIfCancellationRequested();
        try { File.Delete(full); await Task.CompletedTask; return true; }
        catch { return false; }
    }
    public string GetPublicUrl(string relativePath, string? baseUrl = null)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return string.Empty;
        var urlPath = relativePath.Replace('\\', '/');
        if (!urlPath.StartsWith("/")) urlPath = "/" + urlPath;

        var host = baseUrl ?? _opt.PublicBaseUrl;
        return string.IsNullOrWhiteSpace(host) ? urlPath : $"{host.TrimEnd('/')}{urlPath}";
    }
    public string GetPublicUrlOfComplaint(string relativePath, string? baseUrl = null)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return string.Empty;

        // Normalize slashes
        var urlPath = relativePath.Replace('\\', '/');
        if (!urlPath.StartsWith("/"))
            urlPath = "/" + urlPath;

        // Resolve base URL (from parameter or app settings)
        var host = baseUrl ?? _opt.PublicBaseUrl;

        // Get physical wwwroot path
        var webRoot = _env.WebRootPath ?? throw new InvalidOperationException("WebRootPath is not configured.");

        // Build absolute physical path (wwwroot + relative path)
        var absPath = Path.Combine(webRoot, relativePath.TrimStart('/', '\\'))
            .Replace('\\', '/');

        // Combine with baseUrl (for external/public access)
        var fullUrl = string.IsNullOrWhiteSpace(host)
            ? absPath
            : $"{host.TrimEnd('/')}/wwwroot{urlPath}";

        return fullUrl;
    }
    public async Task<(string Path, PanicDispatchMediaType MediaType)> SaveImageOrVideoAsync(
     IFormFile file,
     string folderName,
     CancellationToken ct,
     long maxImageBytes = 10 * 1024 * 1024,   // 10 MB
     long maxVideoBytes = 50 * 1024 * 1024    // 50 MB
 )
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("Empty file.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        var imageExts = new[] { ".jpg", ".jpeg", ".png" };
        var videoExts = new[] { ".mp4" };

        PanicDispatchMediaType mediaType;
        long maxBytes;

        if (imageExts.Contains(ext))
        {
            mediaType = PanicDispatchMediaType.Image;
            maxBytes = maxImageBytes;
        }
        else if (videoExts.Contains(ext))
        {
            mediaType = PanicDispatchMediaType.Video;
            maxBytes = maxVideoBytes;
        }
        else
        {
            throw new InvalidOperationException($"Extension '{ext}' not allowed.");
        }

        if (file.Length > maxBytes)
            throw new InvalidOperationException(
                $"File exceeds {maxBytes / (1024 * 1024)} MB limit.");

        // ✅ MIME validation
        if (_mime.TryGetContentType(file.FileName, out var mappedType))
        {
            if (mediaType == PanicDispatchMediaType.Image &&
                !mappedType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Only image uploads allowed. Detected: {mappedType}");
            }

            if (mediaType == PanicDispatchMediaType.Video &&
                !mappedType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Only video uploads allowed. Detected: {mappedType}");
            }
        }

        // ===== SAME STORAGE LOGIC (UNCHANGED) =====

        var baseDir = _env.ContentRootPath;
        if (string.IsNullOrWhiteSpace(baseDir))
            baseDir = AppContext.BaseDirectory;

        var requestFolder = string.IsNullOrWhiteSpace(_opt.RequestPath)
            ? "CBMS"
            : _opt.RequestPath.Trim('/', '\\');

        var basePhysical = Path.Combine(baseDir, requestFolder);
        Directory.CreateDirectory(basePhysical);

        var relFolder = (folderName ?? string.Empty).Trim().TrimStart('/', '\\');
        var absFolder = string.IsNullOrEmpty(relFolder)
            ? basePhysical
            : Path.Combine(basePhysical, relFolder);

        Directory.CreateDirectory(absFolder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var absPath = Path.Combine(absFolder, fileName);

        await using (var stream = new FileStream(
            absPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            64 * 1024,
            useAsync: true))
        {
            await file.CopyToAsync(stream, ct);
        }

        var relUrl =
            $"/{requestFolder}/{(string.IsNullOrEmpty(relFolder) ? "" : relFolder.Replace('\\', '/') + "/")}{fileName}";

        return (relUrl.Replace("//", "/"), mediaType);
    }

    public async Task<string> SaveFileMemeberrequestAsync(
    IFormFile file,
    string folderName,
    CancellationToken ct,
    long maxBytes = 10 * 1024 * 1024,           // default 10 MB (adjust as you like)
    string[]? allowedExtensions = null)         // e.g. new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }
    {
        // Basic validations
        if (file is null || file.Length == 0)
            throw new ArgumentException("Empty file.");

        if (file.Length > maxBytes)
            throw new InvalidOperationException($"File exceeds {maxBytes / (1024 * 1024)} MB limit.");

        // Extension checks
        var ext = Path.GetExtension(file.FileName);
        var allowed = allowedExtensions ?? DefaultAllowedExt; // keep your existing default list
        if (allowed is not null && !allowed.Contains(ext, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Extension '{ext}' not allowed.");

        // MIME check (based on filename mapping)
        if (_mime.TryGetContentType(file.FileName, out var mappedType))
        {
            if (!mappedType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Only image uploads are allowed. Detected: {mappedType}");
        }

        // Base physical path: <WebRoot>\uploads
        var webRoot = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            throw new InvalidOperationException("WebRootPath is not configured.");

        var baseUploads = Path.Combine(webRoot, "MemberShipRequestFiles");
        Directory.CreateDirectory(baseUploads);

        // Normalize/sanitize folderName (prevent traversal)
        var relFolder = (folderName ?? string.Empty).Trim().TrimStart('/', '\\');
        if (relFolder.Contains("..", StringComparison.Ordinal))
            throw new InvalidOperationException("Invalid folder name.");

        var absFolder = string.IsNullOrEmpty(relFolder) ? baseUploads : Path.Combine(baseUploads, relFolder);
        Directory.CreateDirectory(absFolder);

        // Generate a unique file name and save
        var safeExt = ext.ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{safeExt}";
        var absPath = Path.Combine(absFolder, fileName);

        await using (var stream = new FileStream(absPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true))
            await file.CopyToAsync(stream, ct);

        // Return a web-relative path like: "uploads/{folder}/{file}"
        var relPath = string.IsNullOrEmpty(relFolder)
            ? Path.Combine("uploads", fileName)
            : Path.Combine("uploads", relFolder, fileName);

        // Normalize to URL-style slashes
        relPath = relPath.Replace('\\', '/').Replace("//", "/");
        return relPath;
    }

    public async Task<(string Path, FMType MediaType)> FemugationSaveImageOrVideoAsync(
     IFormFile file,
     string folderName,
     CancellationToken ct,
     long maxImageBytes = 10 * 1024 * 1024,   // 10 MB
     long maxVideoBytes = 50 * 1024 * 1024    // 50 MB
 )
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("Empty file.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        var imageExts = new[] { ".jpg", ".jpeg", ".png" };
        var videoExts = new[] { ".mp4" };

        FMType mediaType;
        long maxBytes;

        if (imageExts.Contains(ext))
        {
            mediaType = FMType.Image;
            maxBytes = maxImageBytes;
        }
        else if (videoExts.Contains(ext))
        {
            mediaType = FMType.Video;
            maxBytes = maxVideoBytes;
        }
        else
        {
            throw new InvalidOperationException($"Extension '{ext}' not allowed.");
        }

        if (file.Length > maxBytes)
            throw new InvalidOperationException(
                $"File exceeds {maxBytes / (1024 * 1024)} MB limit.");

        // ✅ MIME validation
        if (_mime.TryGetContentType(file.FileName, out var mappedType))
        {
            if (mediaType == FMType.Image &&
                !mappedType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Only image uploads allowed. Detected: {mappedType}");
            }

            if (mediaType == FMType.Video &&
                !mappedType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Only video uploads allowed. Detected: {mappedType}");
            }
        }

        // ===== SAME STORAGE LOGIC (UNCHANGED) =====

        var baseDir = _env.ContentRootPath;
        if (string.IsNullOrWhiteSpace(baseDir))
            baseDir = AppContext.BaseDirectory;

        var requestFolder = string.IsNullOrWhiteSpace(_opt.RequestPath)
            ? "SmartDHA"
            : _opt.RequestPath.Trim('/', '\\');

        var basePhysical = Path.Combine(baseDir, requestFolder);
        Directory.CreateDirectory(basePhysical);

        var relFolder = (folderName ?? string.Empty).Trim().TrimStart('/', '\\');
        var absFolder = string.IsNullOrEmpty(relFolder)
            ? basePhysical
            : Path.Combine(basePhysical, relFolder);

        Directory.CreateDirectory(absFolder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var absPath = Path.Combine(absFolder, fileName);

        await using (var stream = new FileStream(
            absPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            64 * 1024,
            useAsync: true))
        {
            await file.CopyToAsync(stream, ct);
        }

        var relUrl =
            $"/{requestFolder}/{(string.IsNullOrEmpty(relFolder) ? "" : relFolder.Replace('\\', '/') + "/")}{fileName}";

        return (relUrl.Replace("//", "/"), mediaType);
    }

    public async Task<string> SavePMSDocumentAsync(
    IFormFile file,
    string folderName,
    CancellationToken ct,
    long maxBytes,
    string[]? allowedExtensions)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Empty file.");

        if (file.Length > maxBytes)
            throw new InvalidOperationException(
                $"File exceeds {maxBytes / (1024 * 1024)} MB limit.");

        // ✅ Allowed extensions
        var allowed = allowedExtensions ?? new[] { ".jpg", ".jpeg", ".png", ".pdf" };

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            throw new InvalidOperationException($"Extension '{ext}' not allowed.");

        // ✅ Allowed MIME types mapped to extensions
        var allowedMimeTypes = new Dictionary<string, string[]>
        {
            [".jpg"] = new[] { "image/jpeg" },
            [".jpeg"] = new[] { "image/jpeg" },
            [".png"] = new[] { "image/png" },
            [".pdf"] = new[] { "application/pdf" }
        };

        if (_mime.TryGetContentType(file.FileName, out var mappedType))
        {
            if (!allowedMimeTypes.TryGetValue(ext, out var validMimes) ||
                !validMimes.Contains(mappedType, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Invalid file type. Detected MIME: {mappedType}");
            }
        }

        // ================= STORAGE =================

        var baseDir = _env.ContentRootPath;
        if (string.IsNullOrWhiteSpace(baseDir))
            baseDir = AppContext.BaseDirectory;

        var requestFolder = string.IsNullOrWhiteSpace(_opt.RequestPath)
            ? "PMS"
            : _opt.RequestPath.Trim('/', '\\');

        var basePhysical = Path.Combine(baseDir, requestFolder);
        Directory.CreateDirectory(basePhysical);

        var relFolder = (folderName ?? string.Empty).Trim().TrimStart('/', '\\');
        var absFolder = string.IsNullOrEmpty(relFolder)
            ? basePhysical
            : Path.Combine(basePhysical, relFolder);

        Directory.CreateDirectory(absFolder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var absPath = Path.Combine(absFolder, fileName);

        await using (var stream = new FileStream(
            absPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            64 * 1024,
            useAsync: true))
        {
            await file.CopyToAsync(stream, ct);
        }

        var relUrl =
            $"/{requestFolder}/" +
            $"{(string.IsNullOrEmpty(relFolder) ? "" : relFolder.Replace('\\', '/') + "/")}" +
            $"{fileName}";

        return relUrl.Replace("//", "/");
    }

}
