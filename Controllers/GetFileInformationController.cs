using DocumentFormat.OpenXml.Packaging;
using getFileInformation.Models;
using Microsoft.AspNetCore.Mvc;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Drawing.Imaging;
using System.Security.Principal;
using System.Text;


namespace getFileInformation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GetFileInformationController : ControllerBase
    {
        [HttpPost("[action]")]
        public async Task<ResponseModel> getInformation(IFormFile file)
        {


            if (file == null || file.Length <= 0)
            {
                return new ResponseModel
                {
                    mesaj = "Dosya Bulunamadý veya boþ"
                };
            }
            var filePath = Path.GetTempFileName();
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            var response = fileExtension switch
            {
                ".pdf" => GetPdfMetaData(filePath),
                ".docx" => GetWordMetaData(filePath),
                ".xlsx" => GetExcelMetaData(filePath),
                ".pptx" => GetPowerPointMetaData(filePath),
                ".jpg" => GetImageMetaData(filePath),
                ".jpeg" => GetImageMetaData(filePath),
                ".png" => GetImageMetaData(filePath),
                _ => new ResponseModel { mesaj = "Desteklenmeyen dosya türü." }
            };

            System.IO.File.Delete(filePath);
            return response;
        }
        //TODO: itxsharp
        private ResponseModel GetPdfMetaData(string filePath)
        {
            PdfDocument document = PdfReader.Open(filePath, PdfDocumentOpenMode.ReadOnly);
            string fileOwner = getFileOwner(filePath);
            return new ResponseModel
            {
                baslik = document.Info.Title,
                dosyaYazari = document.Info.Author,
                dosyaSahibi = fileOwner,
                dosyaYolu = filePath,
                mesaj = "PDF meta verileri baþarýyla alýndý."
            };
        }

        private ResponseModel GetWordMetaData(string filePath)
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, false))
            {
                var props = doc.PackageProperties;
                string fileOwner = getFileOwner(filePath);
                return new ResponseModel
                {
                    baslik = props.Title ?? "Bilgi Yok",
                    dosyaSahibi = fileOwner,
                    dosyaYazari = props.Creator ?? "Bilgi Yok",
                    dosyaYolu = filePath,
                    mesaj = "Word meta verileri baþarýyla alýndý."
                };
            }
        }

        private ResponseModel GetImageMetaData(string filePath)
        {
            using (System.Drawing.Image image = System.Drawing.Image.FromFile(filePath))
            {
                PropertyItem artistItem = image.PropertyItems.FirstOrDefault(item => item.Id == 0x013B);
                string artist = artistItem != null ? Encoding.Default.GetString(artistItem.Value).Trim('\0') : "Yazar bilgisi yok";

                string fileOwner = getFileOwner(filePath);

                return new ResponseModel
                {
                    dosyaYolu = filePath,
                    baslik = Path.GetFileNameWithoutExtension(filePath),
                    dosyaYazari = artist,
                    dosyaSahibi = fileOwner,
                    mesaj = "Resim meta verileri baþarýyla alýndý."
                };
            }
        }
        private ResponseModel GetExcelMetaData(string filePath)
        {
            using (var doc = SpreadsheetDocument.Open(filePath, false))
            {
                var props = doc.PackageProperties;
                string fileOwner = getFileOwner(filePath);
                return new ResponseModel
                {
                    dosyaYolu = filePath,
                    baslik = props.Title ?? "Bilgi Yok",
                    dosyaYazari = props.Creator ?? "Bilgi Yok",
                    dosyaSahibi = fileOwner,
                    mesaj = "Excel meta verileri baþarýyla alýndý."
                };
            }
        }

        private ResponseModel GetPowerPointMetaData(string filePath)
        {
            using (var doc = PresentationDocument.Open(filePath, false))
            {
                var props = doc.PackageProperties;
                string fileOwner = getFileOwner(filePath);
                return new ResponseModel
                {
                    dosyaYolu = filePath,
                    baslik = props.Title ?? "Bilgi Yok",
                    dosyaYazari = props.Creator ?? "Bilgi Yok",
                    dosyaSahibi = fileOwner,
                    mesaj = "PowerPoint meta verileri baþarýyla alýndý."
                };
            }
        }
        private string getFileOwner(string filePath)
        {
            try
            {
                var fileSecurity = new FileInfo(filePath).GetAccessControl();
                var sid = fileSecurity.GetOwner(typeof(SecurityIdentifier)) as SecurityIdentifier;
                return (sid != null) ? sid.Translate(typeof(NTAccount)).ToString() : "Bilinmiyor";
            }
            catch (Exception ex)
            {
                return "Bilgi alýnamadý: " + ex.Message;  // Hata mesajý ile bilgi döndürme
            }
        }
    }

}
