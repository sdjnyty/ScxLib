using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTY.ScxLib
{
  public class Trigger
  {
    public int IsEnabled { get; set; }
    public int IsLooping { get; set; }
    public byte IsObjective { get; set; }
    public int DiscriptionOrder { get; set; }
    public byte[] Discription { get; set; }
    public byte[] Name { get; set; }
    public List<Effect> Effects { get; } = new List<Effect>();
    public List<int> EffectsOrder { get; } = new List<int>();
    public List<Condition> Conditions { get; } = new List<Condition>();
    public List<int> ConditionsOrder { get; } = new List<int>();
  }
}
