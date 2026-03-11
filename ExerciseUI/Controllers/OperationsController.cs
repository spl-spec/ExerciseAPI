using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using ExerciseUI.Models;

namespace ExerciseUI.Controllers
{
    public class OperationsController : Controller
    {
        private readonly IConfiguration _configuration;
        public OperationsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            ViewBag.Result = TempData["Result"];
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendData(CalculationData calcData)
        {
            var data = new
            {
                value1 = calcData.Value1,
                value2 = calcData.Value2,
                function = calcData.Function,
                result = calcData.Result
            };

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();

            var response = await client.PostAsync( "http://localhost:5138/Operations", content);

            var result = await response.Content.ReadAsStringAsync();

            calcData.Result = result;

            // get last 3 records history of same operation
            var historyLast3Response = await client.GetAsync(
                "http://localhost:5138/Operations/last3records?funcName=" + calcData.Function);

            var historyLast3Json = await historyLast3Response.Content.ReadAsStringAsync();

            if (historyLast3Json != null || historyLast3Json != "")
            {
                calcData.calculationLast3History = JsonSerializer.Deserialize<List<CalculationRecord>>(
                    historyLast3Json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            // get month count history of same operation
            var historyMonthResponse = await client.GetAsync(
                "http://localhost:5138/Operations/monthcount?funcName=" + calcData.Function);

            var historyMonthCount = await historyMonthResponse.Content.ReadAsStringAsync();

            calcData.calculationMonthCount = Int32.Parse(historyMonthCount);

            return View("Index", calcData);
        }
    }
}
