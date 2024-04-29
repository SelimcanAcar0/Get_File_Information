namespace getFileInformation.Models
{
    public class ResponseModel
    {
        public string dosyaYolu { get; set; } = "Tespit Edilemedi";
        public string dosyaSahibi { get; set; } = "Tespit Edilemedi";
        public string dosyaYazari { get; set; } = "Tespit Edilemedi";
        public string baslik { get; set; } = "Tespit Edilemedi";
        public required string mesaj { get; set; }
    }
}
