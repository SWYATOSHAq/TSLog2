namespace TSLog.Yard.DbConnectionTester
{
    internal class CustomException: Exception
    {
        public CustomException() 
            :base("Custom exception, it will be ignored!")
        { }
    }
}
