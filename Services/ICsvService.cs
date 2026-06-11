using ZunoksBackend.Models;

namespace ZunoksBackend.Services;

public interface ICsvService
{
    byte[] GenerateLongCsv(IEnumerable<ZunoksSubmission> submissions);
}
