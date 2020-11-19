using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ImageCaptionTest
{
    public partial class _Default : Page
    {
        public const string VisionServiceUrl = "[YOUR CV API RESOURCE ENDPOINT]";
        public const string CaptionServiceKey = "[YOUR API KEY]";

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string imagesPath = "Images";
            if (!Directory.Exists(Server.MapPath(imagesPath)))
            {
                Directory.CreateDirectory(Server.MapPath(imagesPath));
            }

            // Load image
            var imgName = FileUpload1.FileName;
            var imgPath = Path.Combine(imagesPath, imgName);
            if (FileUpload1.PostedFile?.FileName != "")
            {
                FileUpload1.SaveAs(Server.MapPath(imgPath));
                Image1.ImageUrl = "~/" + imgPath;

                // Predict with image
                var captionResult = GetCaptionAsync(Server.MapPath(imgPath)).GetAwaiter().GetResult();

                // Show captioning result
                if (captionResult.Description != null && captionResult.Description.Captions.Any() && captionResult.Description.Captions[0] != null)
                {
                    Text1.InnerText = "I think this is " + captionResult.Description.Captions[0].Text;
                }
                else 
                {
                    Text1.InnerText = "I can't describe this picture";
                }

            }
        }

        protected async Task<CaptionServiceResult> GetCaptionAsync(string imageFilePath)
        {
            var result = new CaptionServiceResult();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", CaptionServiceKey);
                    byte[] byteData = GetImageAsByteArry(imageFilePath);
                    HttpResponseMessage response;

                    using (ByteArrayContent content = new ByteArrayContent(byteData))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        response = await client.PostAsync(VisionServiceUrl + "vision/v3.1/describe", content).ConfigureAwait(false);
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsAsync<CaptionServiceResult>().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
            return result;
        }

        protected byte[] GetImageAsByteArry(string imageFilePath)
        {
            using (FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }

    public class CaptionServiceResult
    {
        public string RequestId { get; set; }
        public CaptionDescription Description { get; set; }
    }

    public class CaptionDescription
    {
        public List<string> Tags { get; set; }
        public List<Caption> Captions { get; set; }
    }

    public class Caption
    {
        public string Text { get; set; }
        public double Confidence { get; set; }
    }
}