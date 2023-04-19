using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AbacusViewer.Models
{
    public class AbacusJointSelection
    {
        public AbacusJointSelection()
        {
            Selections = new List<AbacusSelection>();
        }

        public List<AbacusSelection> Selections { get; set; }

        public double Probability
        {
            get
            {
                // calc from bytearrays

                if (!Selections.Any())
                    return 0;
                
                int numsims = Selections.First().Outcomes.Count();
                int numwins = 0;

                for (int i = 0; i < numsims; i++)
                {
                    bool win = true;
                    foreach(AbacusSelection s in Selections)
                    {
                        if (s.Outcomes[i] != 1)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win)
                        numwins++;
                }

                return (double)numwins / (double)numsims;
            }
        }

        public double Price { get { return Probability > 0 ? 1 / Probability : 0; } }
    }
}