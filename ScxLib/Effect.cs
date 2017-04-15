using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTY.ScxLib
{
  public class Effect
  {
    public EffectType Type { get; set; }
    public List<int> Fields { get; }
    public byte[] Text { get; set; }
    public byte[] SoundFile { get; set; }
    public List<int> UnitIDs { get; set; }

    public Effect()
    {
      Fields = new List<int>(23);
      UnitIDs = new List<int>();
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
  /// Trigger effect types.
  /// </summary>
  public enum EffectType
  {
    ChangeDiplomacy = 1,
    ResearchTechnology,
    SendChat,
    PlaySound,
    SendTribute,
    UnlockGate,
    LockGate,
    ActivateTrigger,
    DeactivateTrigger,
    AIScriptGoal,
    CreateObject,
    TaskObject,
    DeclareVictory,
    KillObject,
    RemoveObject,
    ChangeView,
    Unload,
    ChangeOwnership,
    Patrol,
    DisplayInstructions,
    ClearInstructions,
    FreezeUnit,
    UseAdvancedButtons,
    DamageObject,
    PlaceFoundation,
    ChangeObjectName,
    ChangeObjectHP,
    ChangeObjectAttack,
    StopUnit,
    SnapView,
    EnableTech = 32,
    DisableTech,
    EnableUnit,
    DisableUnit,
    FlashObjects,
  }

  public enum EffectField
  {
    AIGoal,
    Amount,
    Resource,
    Diplomacy,
    NumSelected,
    LocationUnit,
    UnitID,
    PlayerSource,
    PlayerTarget,
    Technology,
    StringTableID,
    Unknown,
    DisplayTime,
    Trigger,
    LocationX,
    LocationY,
    AreaSouthWestX,
    AreaSouthWestY,
    AreaNorthEastX,
    AreaNorthEastY,
    UnitGroup,
    UnitType,
    InstructionPanel
  }
}
