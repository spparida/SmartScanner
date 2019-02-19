using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace SmartScannerAPI.App_Start
{
    public class FileUploadOperation : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            //if (operation.operationId == "AskHr_CreateTicketAsync")
            //{
            operation.consumes.Add("multipart/form-data");
            operation.parameters = new List<Parameter>
                {
                    GetOptionalFileParameter("file"),
                    GetRequiredStringParameter("name"),

                };
            //}
        }

        private Parameter GetOptionalFileParameter(string fileName)
        {
            return new Parameter()
            {
                name = fileName,
                required = false,
                type = "file",
                @in = "formData",
                vendorExtensions = new Dictionary<string, object> { { "x-ms-media-kind", "audio" } }
            };
        }

        private Parameter GetRequiredStringParameter(string paramName)
        {
            return new Parameter()
            {
                name = paramName,
                required = true,
                type = "string",
                @in = "formData"
            };
        }

    }
}