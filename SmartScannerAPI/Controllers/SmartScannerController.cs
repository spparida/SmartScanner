using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
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
        public async Task<string> Post()
        {
            string uploadPath = "~/ScanImage/";
            var httpRequest = HttpContext.Current.Request;
            var filePath = string.Empty;

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
            var crmresult = CreateLeadInCrm(results);
            return $"Your lead has been created in Crm with leadId : {crmresult}";
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
        /// Returns Vision API response
        /// </summary>
        /// <param name="imageFilePath"></param>
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

        private string CreateLeadInCrm(Results result)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string connectionString = "Url=https://appfactoryindia.crm8.dynamics.com; Username=satya@appfactoryindia.onmicrosoft.com; Password=Test@1234; authtype=Office365";
            CrmServiceClient service = new CrmServiceClient(connectionString);


            Entity leadRecord = new Entity("lead");

            if (!string.IsNullOrWhiteSpace(result.Info["Name"]))
            {
                string[] ssize = result.Info["Name"].Split(null);
                leadRecord.Attributes.Add("firstname", ssize[0]);
                leadRecord.Attributes.Add("lastname", ssize[1]);
            }

            leadRecord.Attributes.Add("emailaddress1", result.Info["Email"]);
            leadRecord.Attributes.Add("companyname", result.Info["Company"]);
            leadRecord.Attributes.Add("websiteurl", result.Info["Website"]);
            leadRecord.Attributes.Add("telephone1", result.Info["Phone"]);

            var guid = service.Create(leadRecord);
            return guid.ToString();
        }
    }
}
