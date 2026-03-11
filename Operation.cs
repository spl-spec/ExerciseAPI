namespace ExerciseAPI
{
    public class Operation
    {
        public delegate int OperationService(int a, int b);
        public int PerformOperation(int a, int b, OperationService operationHandler)
        {
            // the service logic uses the passed method (delegate) to perform the calculation
            return operationHandler(a, b);
        }
    }
}

