using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTY.ScxLib
{
  public class Unit
  {
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    public int Id { get; set; }
    public short UnitClass { get; set; }
    public byte State { get; set; }
    public float Rotation { get; set; }
    public short Frame { get; set; }
    public int Garrison { get; set; }
  }
}
