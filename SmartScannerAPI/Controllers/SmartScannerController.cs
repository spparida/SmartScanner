using Newtonsoft.Json;
using SmartScanner.OCR.JSONResultsParser;
using SmartScannerAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace SmartScannerAPI.Controllers
{
    [RoutePrefix("api/SmartScanner")]
    public class SmartScannerController : ApiController
    {
        // Replace <Subscription Key> with your valid subscription key.
        const string subscriptionKey = "dca4bfca648145fb82849840e7281667";

        const string endPoint = "https://westus.api.cognitive.microsoft.com/vision/v1.0/ocr";

        [HttpPost]
        [Route("PostImage")]
        [AllowAnonymous]
        public async Task<Results> Post()
        {
            string uploadPath = "~/ScanImage/";
            var httpRequest = HttpContext.Current.Request;
            var filePath= string.Empty;
            
            foreach (string file in httpRequest.Files)
            {
                var postedFile = httpRequest.Files[file];
                 filePath = HttpContext.Current.Server.MapPath(uploadPath + postedFile.FileName);
                postedFile.SaveAs(filePath);
                // NOTE: To store in memory use postedFile.InputStream
            }
            
            var ci = new ContactInfo();
            ImageInfoViewModel responeData = new ImageInfoViewModel();
            string extractedResult = "";
            var errors = new List<string>();

            HttpResponseMessage response = await GetAzureVisionAPIResponse(filePath);
            // Get the JSON response.
            string result = await response.Content.ReadAsStringAsync();

            //If it is success it will execute further process.
            if (response.IsSuccessStatusCode)
            {
                // The JSON response mapped into respective view model.
                responeData = JsonConvert.DeserializeObject<ImageInfoViewModel>(result,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs earg)
                        {
                            errors.Add(earg.ErrorContext.Member.ToString());
                            earg.ErrorContext.Handled = true;
                        }
                    }
                );

                var linesCount = responeData.regions[0].lines.Count;
                for (int i = 0; i < linesCount; i++)
                {
                    var wordsCount = responeData.regions[0].lines[i].words.Count;
                    for (int j = 0; j < wordsCount; j++)
                    {
                        //Appending all the lines content into one.
                        extractedResult += responeData.regions[0].lines[i].words[j].text + " ";
                    }
                    extractedResult += Environment.NewLine;
                }
                //dynamic data = await req.Content.ReadAsAsync<object>();
                OCRData ocr = JsonConvert.DeserializeObject<OCRData>(result.ToString());

                Parallel.ForEach(ocr.Regions, (r) =>
                {
                    ci.Parse(r);
                });

            }

            var results = new Results(ci.Parsers.Select(p => new { p.Name, p.Value }).ToDictionary(d => d.Name, d => d.Value), ci.UnKnown);
            return results;
        }

        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<HttpResponseMessage> GetAzureVisionAPIResponse(string imageFilePath)
        {
            //string imageFilePath = @"C:\Users\satprpa\Desktop\Temp\Meher.jpg";
            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", subscriptionKey);

            // Request parameters.
            string requestParameters = "language=unk&detectOrientation=true";

            // Assemble the URI for the REST API Call.
            string uri = endPoint + "?" + requestParameters;

            HttpResponseMessage response;

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json"
                // and "multipart/form-data".
                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

                // Make the REST API call.
                response = await client.PostAsync(uri, content);
            }
            return response;
        }
    }
}
