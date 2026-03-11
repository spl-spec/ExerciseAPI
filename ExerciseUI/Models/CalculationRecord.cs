namespace ExerciseUI.Models
{
    public class CalculationRecord
    {
        public int Id { get; set; }
        public int Number1 { get; set; }
        public int Number2 { get; set; }
        public string Operation { get; set; } 
        public int Result { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
