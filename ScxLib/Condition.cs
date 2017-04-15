using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTY.ScxLib
{
  public class Condition
  {
    public ConditionType Type { get; set; }
    public List<int> Fields { get; }

    public Condition()
    {
      Fields = new List<int>(16);
    }

    public int GetField(EffectField field)
    {
      return Fields[(int)field];
    }

    public void SetField(EffectField field, int value)
    {
      Fields[(int)field] = value;
    }
  }

  /// <summary>
  /// Trigger condition types.
  /// </summary>
  public enum ConditionType
  {
    BringObjectToArea,
    BringObjectToObject,
    OwnObjects,
    OwnFewerObjects,
    ObjectsInArea,
    DestroyObject,
    CaptureObject,
    AccumulateAttribute,
    ResearchTechnology,
    Timer,
    ObjectSelected,
    AiSignal,
    PlayerDefeated,
    ObjectHasTarget,
    ObjectVisible,
    ObjectNotVisible,
    ResearchingTechnology,
    UnitsGarrisoned,
    DifficultyLevel,
    OwnFewerFoundations,
    SelectedObjectsInArea,
    PoweredObjectsInArea,
    UnitsQueuedPastPopCap,
  }
}
