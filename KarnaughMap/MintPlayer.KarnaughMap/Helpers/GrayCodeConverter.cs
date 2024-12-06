namespace KarnaughMap
{
    internal static class GrayCodeConverter
    {
        public static int Decimal2Gray(int n)
        {
            return n ^ (n >> 1);
        }

        public static int Gray2Decimal(int n)
        {
            var mask = n >> 1;
            while (mask != 0)
            {
                n ^= mask;
                mask >>= 1;
            }
            return n;
        }
    }
}
