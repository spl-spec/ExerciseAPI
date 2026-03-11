namespace ExerciseUI.Models
{
    public class CalculationData
    {
        public int Value1 { get; set; }
        public int Value2 { get; set; }
        public string Function { get; set; }
        public string Result { get; set; } = string.Empty;
        public List<CalculationRecord> calculationLast3History { get; set; } = new();
        public int calculationMonthCount { get; set; }
    }
}
