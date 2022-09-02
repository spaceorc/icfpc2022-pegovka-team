using System.Collections.Generic;

namespace lib.Enchancers;

public interface ISolutionEnchancer
{
    List<Move> Enchance(Screen problem, List<Move> moves);
}
