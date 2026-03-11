using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ExerciseAPI.Models;
using static ExerciseAPI.Operation;
namespace ExerciseAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OperationsController : ControllerBase
    {
        private readonly Operation operation = new Operation();
        private readonly IConfiguration _configuration;

        public OperationsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost(Name = "CalculationFuncPost")]
        public IActionResult CalculationFuncPost([FromBody] CalculationData calaData)
        {
            OperationService func = calaData.Function == "sum" ? Sum : Multiply;
            // pass a different method (Sum/Multiply) to the same service function
            int result = operation.PerformOperation(calaData.Value1, calaData.Value2, func);
            calaData.Result = result.ToString();

            //insert to db history
            InsertToCalculationsHistory(calaData);

            return Ok($"The result of {calaData.Function} is: {result}");
        }

        [HttpGet(Name = "CalculationFuncGet")] //For testing the API
        public IActionResult CalculationFuncGet([FromQuery] int Value1, [FromQuery] int Value2, [FromQuery] string funcName)
        {
            OperationService func = funcName == "sum" ? Sum : Multiply;
            // pass a different method (Sum/Multiply) to the same service function
            int result = operation.PerformOperation(Value1, Value2, func);
            
            return Ok($"The result of {funcName} is: {result}");
        }

        [HttpGet("last3records")]
        public IActionResult GetLast3History([FromQuery] string funcName)
        {
            var list = new List<CalculationRecord>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT TOP 3 * FROM CalculationsHistory " +
                    "WHERE Operation = @str ORDER BY CreatedAt DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@str", funcName);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new CalculationRecord
                        {
                            Id = (int)reader["Id"],
                            Number1 = (int)reader["Number1"],
                            Number2 = (int)reader["Number2"],
                            Operation = reader["Operation"].ToString(),
                            Result = (int)reader["Result"],
                            CreatedAt = (DateTime)reader["CreatedAt"]
                        });
                    }
                }
            }

            return Ok(list);
        }

        [HttpGet("monthcount")]
        public IActionResult GetMonthCountHistory([FromQuery] string funcName)
        {
            int monthCount = 0;

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT count(*) AS MonthCount FROM CalculationsHistory " +
                    "WHERE Operation = @str AND MONTH(CreatedAt) = MONTH(GetDate())";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@str", funcName);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        monthCount = (int)reader["MonthCount"];
                    }
                }
            }

            return Ok(monthCount);
        }
        private static int Sum(int a, int b) => a + b;
        private static int Multiply(int a, int b) => a * b;

        private void InsertToCalculationsHistory(CalculationData calaData)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO CalculationsHistory 
                        (Number1, Number2, Operation, Result)
                        VALUES (@n1, @n2, @str, @res)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@n1", calaData.Value1);
                cmd.Parameters.AddWithValue("@n2", calaData.Value2);
                cmd.Parameters.AddWithValue("@str", calaData.Function);
                cmd.Parameters.AddWithValue("@res", calaData.Result);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

    }
}
