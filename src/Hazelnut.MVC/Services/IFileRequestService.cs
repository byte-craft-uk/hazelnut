using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Hazelnut.Entities;
using System;
using System.Threading.Tasks;

namespace Hazelnut.Mvc
{
    public interface IFileRequestService
    {
        Task DeleteTempFiles(TimeSpan olderThan);
        Task<Blob> Bind(string fileKey);
        Task<object> TempSaveUploadedFile(IFormFile file, bool allowUnsafeExtension = false);
        Task<object> CreateDownloadAction(byte[] data, string filename);
        Task<ActionResult> Download(string key);
    }
}