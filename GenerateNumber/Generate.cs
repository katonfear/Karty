namespace GenerateNumber
{
    public class Generate
    {
        public string GenerateGuid() 
        {
            Guid g = Guid.NewGuid();
            return g.ToString();
        }
    }
}
