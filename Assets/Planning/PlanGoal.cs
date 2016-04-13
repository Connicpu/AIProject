using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Planning
{
    public class PlanGoal : PlanState
    {
        public override bool Equivalent(PlanState other)
        {
            return
                other.Food >= Food &&
                other.Wood >= Wood;
        }
    }
}
