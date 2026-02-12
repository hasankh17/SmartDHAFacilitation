using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DHAFacilitationAPIs.Application.Common.DependencyResolver;
using Microsoft.AspNetCore.Http;

namespace DHAFacilitationAPIs.Application.Interface.Service;
public interface IFileStorageService : IServicesType.IScopedService
{
    Task<string> SaveFileAsync(IFormFile file, string folderName, CancellationToken ct);
    Task<bool> DeleteFileAsync(string relativePath, CancellationToken ct);
    string GetPublicUrl(string relativePath, string? baseUrl = null);
}
