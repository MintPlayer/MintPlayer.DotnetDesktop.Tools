using System.Collections.Generic;
using System.Threading.Tasks;

namespace MintPlayer.QuineMcCluskey.Abstractions;

public interface IQuineMcCluskeySolver
{
    Task<IEnumerable<IRequiredLoop>> QMC_Solve(IEnumerable<int> minterms, IEnumerable<int> dontcares);
}
