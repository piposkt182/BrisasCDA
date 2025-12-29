namespace CDABrisasAPI.Dto
{
    public class ImageAnalysisResultDto
    {
        public string? Caption { get; set; }
        public double? CaptionConfidence { get; set; }
        public List<ImageTagDto> Tags { get; set; } = new();
        public List<ImageObjectDto> Objects { get; set; } = new();
    }
}
