using MintPlayer.QuineMcCluskey;

namespace QuineMcCluskey.Test;

class Program
{
    static async Task Main(string[] args)
    {
        var random = new Random();
        var list = new List<int>();
        for (int i = 0; i < 16; i++)
        {
            var num = random.Next(16);
            if (!list.Contains(num)) list.Add(num);
        }

        while (true)
        {
            var loops = await QuineMcCluskeySolver.QMC_Solve(list, new int[] { });

            Console.ReadKey();
        }
    }
}
