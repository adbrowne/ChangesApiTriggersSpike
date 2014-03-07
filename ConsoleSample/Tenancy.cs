namespace ConsoleSample
{
    internal class Tenancy
    {
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string DbName { get { return "tenancy-" + SystemName; } }
    }
}