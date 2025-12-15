using Application.Utilities.Dtos;
using System.Text.Json;

namespace Application.Utilities.Mapper
{
    public static class VerificationAgreementMapper
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static List<VerificationAgreementDto> Map(
            List<Dictionary<string, string>> rows)
        {
            return rows
                .Select(row =>
                    JsonSerializer.Deserialize<VerificationAgreementDto>(
                        JsonSerializer.Serialize(row),
                        Options
                    )!
                )
                .ToList();
        }
    }
}
