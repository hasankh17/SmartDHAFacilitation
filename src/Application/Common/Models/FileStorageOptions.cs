using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHAFacilitationAPIs.Application.Common.Models;
public class FileStorageOptions
{
    /// <summary>
    /// Subfolder under the application content root where files are written.
    /// Default: "SmartDHA"
    /// </summary>
    public string RequestPath { get; set; } = "SmartDHA";

    /// <summary>
    /// Root path used when deleting files or building absolute physical paths.
    /// Default: application base directory (can be overridden by configuration).
    /// </summary>
    public string RootPath { get; set; } = AppContext.BaseDirectory;

    /// <summary>
    /// Public base URL (e.g. https://cdn.example.com) used by GetPublicUrl to produce full URLs.
    /// If null or empty GetPublicUrl returns a path-only value.
    /// </summary>
    public string? PublicBaseUrl { get; set; }
}
