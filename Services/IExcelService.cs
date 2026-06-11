using ZunoksBackend.Models;

namespace ZunoksBackend.Services;

public interface IExcelService
{
    byte[] GenerateExcel(ZunoksSubmission submission);
    byte[] GenerateMasterWorkbook(IEnumerable<ZunoksSubmission> submissions);
    byte[] GenerateCompareMatrixExcel(IEnumerable<ZunoksSubmission> submissions, string moduleName);
}
