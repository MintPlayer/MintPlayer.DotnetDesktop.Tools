namespace QuineMcCluskey
{
    public class RequiredLoop
    {
        internal RequiredLoop(Data.QuineMcCluskey.Table1.Loop loop)
        {
            MinTerms = loop.MinTerms;
            this.loop = loop;
        }

        private Data.QuineMcCluskey.Table1.Loop loop;

        public int[] MinTerms { get; private set; }

        public override string ToString()
        {
            return loop.ToString();
        }
        public string ToString(string[] inputVariables)
        {
            return loop.ToString(inputVariables);
        }
    }
}
