using Csv;
using System.Text;

namespace SpendingAnalyzer.Services
{
    internal class TransactionImportProcessor
    {
        public async Task<ICsvLine[]> GetContent(IFormFile transactions, CancellationToken ct)
        {
            using var stream = transactions.OpenReadStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);
            var bytes = ms.ToArray();

            string text;
            var textUtf8 = Encoding.UTF8.GetString(bytes);
            if (textUtf8.Contains('\uFFFD'))
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var cp1250 = Encoding.GetEncoding(1250);
                text = cp1250.GetString(bytes);
            }
            else
            {
                text = textUtf8;
            }

            return CsvReader.ReadFromText(text, new CsvOptions
            {
                Separator = ',',
                HeaderMode = HeaderMode.HeaderPresent,
            }).ToArray();
        }
    }
}
