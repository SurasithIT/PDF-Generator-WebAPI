using System.Net;
using Microsoft.AspNetCore.Mvc;
using PDF.Generator.WebAPI.Models.PdfGenerators;
using PDF.Generator.WebAPI.Models.Requests.PdfGenerators;
using PDF.Generator.WebAPI.Services.Interfaces;

namespace PDF.Generator.WebAPI.Controllers;

public class PdfGeneratorController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IPdfGeneratorService _pdfGeneratorService;

    public PdfGeneratorController(IConfiguration configuration, IPdfGeneratorService pdfGeneratorService)
    {
        _configuration = configuration;
        _pdfGeneratorService = pdfGeneratorService;
    }

    [HttpPost("GeneratePDF")]
    public async Task<IActionResult> GeneratePDF([FromBody] GeneratePdfReq req)
    {
        try
        {
            string templatePath = _configuration.GetValue<string>("ReportTemplatePath");
            string templateFullPath = Path.Combine(Environment.CurrentDirectory, templatePath, req.TemplateName);
            byte[] pdfResult = await _pdfGeneratorService.GeneratePdf(templateFullPath: templateFullPath,
                templateData: req.TemplateDataObject, headerText: req.HeaderText,
                hasPageNumber: req.HasPageNumber ?? true);

            // Save the pdf to the disk
            if (req.SaveAsFile == true)
            {
                string outputPath = _configuration.GetValue<string>("ReportOutputPath");
                if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
                string outputFileName = !String.IsNullOrEmpty(req.OutputFileName) ? req.OutputFileName : "output.pdf";
                string outputFullPath = Path.Combine(Environment.CurrentDirectory, outputPath, outputFileName);

                await System.IO.File.WriteAllBytesAsync(outputFullPath, pdfResult);
            }

            var result = new {statusCode = HttpStatusCode.OK, success = true, message = "Success", data = pdfResult};
            return Ok(result);
        }
        catch (Exception ex)
        {
            var result = new
            {
                statusCode = HttpStatusCode.NotImplemented,
                success = false,
                message = "the server has an error, please try again later.",
            };
            return Ok(result);
        }
    }

    [HttpGet("TestGeneratePDF")]
    public async Task<IActionResult> TestGeneratePDF()
    {
        try
        {
            string templateName = "greeting-template.html";

            List<GreetingModel> greetings = new List<GreetingModel>()
            {
                new GreetingModel("French", "Bonjour", "Salut"),
                new GreetingModel("Spanish", "Hola", "??Qu?? tal? (What???s up?)"),
                new GreetingModel("Italian", "Buongiorno", "Ciao"),
                new GreetingModel("Chinese", "??????!", null),
                new GreetingModel("Bulgarian", "??????????????!", "??????????????????!"),
                new GreetingModel("Japanese", "???????????????!", "?????????!"),
                new GreetingModel("Hebrew", "!????????", null),
                new GreetingModel("Hindi", "??????????????????", null),
                new GreetingModel("Korean", "???????????????", null),
                new GreetingModel("Thai", "??????????????????", null),
            };

            var templateDataObject = new {Greetings = greetings};

            string templatePath = _configuration.GetValue<string>("ReportTemplatePath");
            string templateFullPath = Path.Combine(Environment.CurrentDirectory, templatePath, templateName);
            byte[] pdfResult = await _pdfGeneratorService.GeneratePdf(templateFullPath: templateFullPath,
                templateData: templateDataObject);

            // Save the pdf to the disk
            string outputPath = _configuration.GetValue<string>("ReportOutputPath");
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, outputPath))) Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, outputPath));
            string outputFileName = "test_output.pdf";
            string outputFullPath = Path.Combine(Environment.CurrentDirectory, outputPath, outputFileName);

            await System.IO.File.WriteAllBytesAsync(outputFullPath, pdfResult);

            var result = new {statusCode = HttpStatusCode.OK, success = true, message = "Success", data = pdfResult};
            // return Ok(result);
            return File(pdfResult, "application/pdf", outputFileName);
        }
        catch (Exception ex)
        {
            var result = new
            {
                statusCode = HttpStatusCode.InternalServerError,
                success = false,
                message = "the server has an error, please try again later.",
            };
            return Ok(result);
        }
    }

    [HttpPost("MergePDF")]
    public async Task<IActionResult> MergePDF([FromBody] MergePDFReq req)
    {
        try
        {
            byte[] pdfResult = null;
            byte[] mergedResult = _pdfGeneratorService.ConcatenatePdfs(req.Documents);
            pdfResult = mergedResult;
            if (req.HasPageNumber == true)
            {
                byte[] stampPageNumberResult = _pdfGeneratorService.StampMergedPdfNumber(mergedResult);
                pdfResult = stampPageNumberResult;
            }

            // Save the pdf to the disk
            if (req.SaveAsFile == true)
            {
                string outputPath = _configuration.GetValue<string>("ReportOutputPath");
                if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
                string outputFileName = !String.IsNullOrEmpty(req.OutputFileName) ? req.OutputFileName : "output.pdf";
                string outputFullPath = Path.Combine(Environment.CurrentDirectory, outputPath, outputFileName);

                await System.IO.File.WriteAllBytesAsync(outputFullPath, pdfResult);
            }

            var result = new {statusCode = HttpStatusCode.OK, success = true, message = "Success", data = pdfResult};
            return Ok(result);
        }
        catch (Exception ex)
        {
            var result = new
            {
                statusCode = HttpStatusCode.InternalServerError,
                success = false,
                message = "the server has an error, please try again later.",
            };
            return Ok(result);
        }
    }
}