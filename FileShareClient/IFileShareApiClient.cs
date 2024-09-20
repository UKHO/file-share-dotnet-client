using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UKHO.FileShareClient.Models;

namespace UKHO.FileShareClient
{
    public interface IFileShareApiClient
    {
        Task<BatchStatusResponse> GetBatchStatusAsync(string batchId);
        Task<BatchSearchResponse> Search(string searchQuery);
        Task<BatchSearchResponse> Search(string searchQuery, int? pageSize);
        Task<BatchSearchResponse> Search(string searchQuery, int? pageSize, int? start);
        Task<IResult<BatchSearchResponse>> Search(string searchQuery, int? pageSize, int? start, CancellationToken cancellationToken);
        Task<IResult<BatchAttributesSearchResponse>> BatchAttributeSearch(string searchQuery, CancellationToken cancellationToken);
        Task<IResult<BatchAttributesSearchResponse>> BatchAttributeSearch(string searchQuery, int maxAttributeValueCount, CancellationToken cancellationToken);
        Task<Stream> DownloadFileAsync(string batchId, string filename);
        Task<IResult<DownloadFileResponse>> DownloadFileAsync(string batchId, string fileName, Stream destinationStream, long fileSizeInBytes = 0, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetUserAttributesAsync();
        Task<IResult<Stream>> DownloadZipFileAsync(string batchId, CancellationToken cancellationToken);
    }
}
