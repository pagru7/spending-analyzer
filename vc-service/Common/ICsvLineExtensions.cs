using Csv;
using SpendingAnalyzer.Entities;
using System.Globalization;
using static SpendingAnalyzer.Services.InteligoTransactionImportDataParser;

namespace SpendingAnalyzer.Common
{
    public static class ICsvLineExtensions
    {
        internal static bool TryGetInt(
            this ICsvLine line,
            ImportedTransactionDataAdnotation index,
            out int value)
            => int.TryParse(line[((int)index)], out value);

        internal static bool TryGetString(
            this ICsvLine line,
            ImportedTransactionDataAdnotation index,
            out string value)
        {
            value = line[((int)index)];
            return !string.IsNullOrEmpty(value);
        }

        internal static bool TryGetDate(
            this ICsvLine line,
            ImportedTransactionDataAdnotation index,
            out DateTime issueDate)
            => DateTime.TryParse(
                line[((int)index)],
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out issueDate);

        internal static bool TryGetCurrency(
            this ICsvLine line,
            ImportedTransactionDataAdnotation index,
            out Currency currency)
            => Enum.TryParse<Currency>(
                line[((int)index)],
                true,
                out currency);

        internal static bool TryGetDecimal(
            this ICsvLine line,
            ImportedTransactionDataAdnotation index,
            out decimal value)
            => decimal.TryParse(
                line[((int)index)],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value);

        internal static bool TryGetTransactionType(
            this ICsvLine csvLine,
            ImportedTransactionDataAdnotation index,
            out TransactionType result)
            => ImportTransactionTypeMapping.Mapping.TryGetValue(csvLine[((int)index)], out result);
    }
}
