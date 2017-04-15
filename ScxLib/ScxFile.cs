using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace YTY.ScxLib
{
  public class ScxFile
  {
    public string FileName { get; set; }
    public byte[] Version { get; set; }
    public int FormatVersion { get; set; }
    public int LastSave { get; set; }
    public byte[] Instruction { get; set; }
    public int PlayerCount { get; set; }
    public int NextUid { get; set; }
    public float Version2 { get; set; }
    public List<Player> Players { get; } = new List<Player>(16);
    public byte[] OriginalFileName { get; set; }
    public List<int> StringTableInfos { get; } = new List<int>(6);
    public List<byte[]> StringInfos { get; set; }
    public int HasBitmap { get; set; }
    public int BitmapX { get; set; }
    public int BitmapY { get; set; }
    public BITMAPDIB Bitmap { get; set; }
    public long Conquest { get; set; }
    public long Relics { get; set; }
    public long Explored { get; set; }
    public int AllMustMeet { get; set; }
    public VictoryMode VictoryMode { get; set; }
    public int Score { get; set; }
    public int Time { get; set; }
    public byte LockTeams { get; set; }
    public byte PlayerChooseTeams { get; set; }
    public byte RandomStartPoints { get; set; }
    public byte MaxTeams { get; set; }
    public int AllTechs { get; set; }
    public int CameraX { get; set; }
    public int CameraY { get; set; }
    public MapType MapType { get; set; }
    public int MapX { get; set; }
    public int MapY { get; set; }
    public Terrain[,] Map { get; set; }
    public List<Resource> Resources { get; } = new List<Resource>(8);
    public List<List<Unit>> Units { get; } = new List<List<Unit>>(9);
    public List<PlayerMisc> PlayerMiscs { get; } = new List<PlayerMisc>(8);
    public List<Trigger> Triggers { get; }
    public List<int> TriggersOrder { get; }
    public int HasAiFile { get; set; }
    public Dictionary<byte[], byte[]> AiFiles { get; }

    private List<int> unknownInt32s;
    private int unknownFlag;
    private byte[] unknownBlock;

    private ScxFile()
    {

    }

    public ScxFile(string fileName) : this(new FileStream(fileName, FileMode.Open))
    {
      FileName = fileName;
    }

    public ScxFile(Stream stream) : this()
    {
      using (var br = new BinaryReader(stream))
      {
        Version = br.ReadBytes(4);
        br.ReadBytes(4);
        FormatVersion = br.ReadInt32();
        LastSave = br.ReadInt32();
        Instruction = br.ReadBytes(br.ReadInt32());
        br.ReadInt32();
        PlayerCount = br.ReadInt32();
        if (FormatVersion == 3)
        {
          br.ReadBytes(8);
          var unknownCount = br.ReadInt32();
          unknownInt32s = new List<int>(unknownCount);
          for (var i = 0; i < unknownCount; i++)
            unknownInt32s.Add(br.ReadInt32());
        }
        using (var ds = new DeflateStream(stream, CompressionMode.Decompress))
        {
          using (var dr = new BinaryReader(ds))
          {
            NextUid = dr.ReadInt32();
            Version2 = dr.ReadSingle();
            for (var i = 0; i < 16; i++)
            {
              Players.Add(new Player()
              {
                Name = dr.ReadBytes(256)
              });
            }
            for (var i = 0; i < 16; i++)
            {
              Players[i].Name_StringTable = dr.ReadInt32();
            }
            for (var i = 0; i < 16; i++)
            {
              Players[i].IsActive = dr.ReadInt32();
              Players[i].IsHuman = dr.ReadInt32();
              Players[i].Civilization = dr.ReadInt32();
              Debug.Assert(dr.ReadInt32() == 4);
            }
            dr.ReadBytes(9);
            OriginalFileName = dr.ReadBytes(dr.ReadInt16());
            for (var i = 0; i < 5; i++)
            {
              StringTableInfos.Add(dr.ReadInt32());
            }
            if (GetVersion() >= ScxVersion.Version122)
              StringTableInfos.Add(dr.ReadInt32());
            StringInfos = new List<byte[]>(10);
            for (var i = 0; i < 9; i++)
            {
              StringInfos.Add(dr.ReadBytes(dr.ReadInt16()));
            }
            if (GetVersion() >= ScxVersion.Version122)
              StringInfos.Add(dr.ReadBytes(dr.ReadInt16()));
            HasBitmap = dr.ReadInt32();
            BitmapX = dr.ReadInt32();
            BitmapY = dr.ReadInt32();
            dr.ReadInt16();
            if (BitmapX > 0 && BitmapY > 0)
            {
              Bitmap = new BITMAPDIB()
              {
                Size = dr.ReadInt32(),
                Width = dr.ReadInt32(),
                Height = dr.ReadInt32(),
                Planes = dr.ReadInt32(),
                BitCount = dr.ReadInt32(),
                Compression = dr.ReadInt32(),
                SizeImage = dr.ReadInt32(),
                XPelsPerMeter = dr.ReadInt32(),
                YPelsPerMeter = dr.ReadInt32(),
                ClrUsed = dr.ReadInt32(),
                ClrImportant = dr.ReadInt32(),
                Colors = new List<RGB>(256)
              };
              for (var i = 0; i < 256; i++)
              {
                Bitmap.Colors.Add(new RGB()
                {
                  Red = dr.ReadByte(),
                  Green = dr.ReadByte(),
                  Blue = dr.ReadByte(),
                });
                dr.ReadByte();
              }
              Bitmap.ImageData = dr.ReadBytes(((BitmapX - 1) / 4 + 1) * 4 * BitmapY);
            }
            for (var i = 0; i < 32; i++)
            {
              dr.ReadBytes(dr.ReadInt16());
            }
            for (var i = 0; i < 16; i++)
            {
              Players[i].Ai = dr.ReadBytes(dr.ReadInt16());
            }
            for (var i = 0; i < 16; i++)
            {
              Debug.Assert(dr.ReadInt64() == 0L);
              Players[i].AiFile = dr.ReadBytes(dr.ReadInt32());
            }
            for (var i = 0; i < 16; i++)
            {
              Players[i].Personality = (Personality)dr.ReadByte();
            }
            Debug.Assert(dr.ReadInt32() == -99);
            for (var i = 0; i < 16; i++)
            {
              Players[i].Gold = dr.ReadInt32();
              Players[i].Wood = dr.ReadInt32();
              Players[i].Food = dr.ReadInt32();
              Players[i].Stone = dr.ReadInt32();
              Players[i].Ore = dr.ReadInt32();
              Debug.Assert(dr.ReadInt32() == 0);
              if (GetVersion() >= ScxVersion.Version124)
                Players[i].PlayerNumber = dr.ReadInt32();
            }
            Debug.Assert(dr.ReadInt32() == -99);
            Conquest = dr.ReadInt64();
            Relics = dr.ReadInt64();
            Explored = dr.ReadInt64();
            AllMustMeet = dr.ReadInt32();
            VictoryMode = (VictoryMode)dr.ReadInt32();
            Score = dr.ReadInt32();
            Time = dr.ReadInt32();
            for (var i = 0; i < 16; i++)
              for (var j = 0; j < 16; j++)
                Players[i].Diplomacies.Add((DiplomacyInt)dr.ReadInt32());
            dr.ReadBytes(11520);
            Debug.Assert(dr.ReadInt32() == -99);
            for (var i = 0; i < 16; i++)
              Players[i].AlliedVictory = dr.ReadInt32();
            if (GetVersion() >= ScxVersion.Version123)
            {
              LockTeams = dr.ReadByte();
              PlayerChooseTeams = dr.ReadByte();
              RandomStartPoints = dr.ReadByte();
              MaxTeams = dr.ReadByte();
            }
            for (var i = 0; i < 16; i++)
              Players[i].NumDisabledTechs = dr.ReadInt32();
            for (var i = 0; i < 16; i++)
              for (var j = 0; j < 30; j++)
                Players[i].DisabledTechs.Add(dr.ReadInt32());
            for (var i = 0; i < 16; i++)
              Players[i].NumDisabledUnits = dr.ReadInt32();
            for (var i = 0; i < 16; i++)
              for (var j = 0; j < 30; j++)
                Players[i].DisabledUnits.Add(dr.ReadInt32());
            for (var i = 0; i < 16; i++)
              Players[i].NumDisabledBuildings = dr.ReadInt32();
            for (var i = 0; i < 16; i++)
            {
              var countJ = GetVersion() >= ScxVersion.Version126 ? 30 : 20;
              for (var j = 0; j < countJ; j++)
                Players[i].DisabledBuildings.Add(dr.ReadInt32());
            }
            Debug.Assert(dr.ReadInt64() == 0L);
            AllTechs = dr.ReadInt32();
            for (var i = 0; i < 16; i++)
            {
              Players[i].StartAge = (StartAge)dr.ReadInt32();
              if (GetVersion() >= ScxVersion.Version126)
                Players[i].StartAge -= 2;
            }
            Debug.Assert(dr.ReadInt32() == -99);
            CameraX = dr.ReadInt32();
            CameraY = dr.ReadInt32();
            if (GetVersion() >= ScxVersion.Version122)
              MapType = (MapType)dr.ReadInt32();
            if (GetVersion() >= ScxVersion.Version124)
              dr.ReadBytes(16);
            MapX = dr.ReadInt32();
            MapY = dr.ReadInt32();
            Map = new Terrain[MapX, MapY];
            for (var i = 0; i < MapX; i++)
            {
              for (var j = 0; j < MapY; j++)
              {
                Map[i, j] = new Terrain();
                Map[i, j].Id = dr.ReadByte();
                Map[i, j].Elevation = dr.ReadInt16();
              }
            }
            Debug.Assert(dr.ReadInt32() == 9);
            for (var i = 0; i < 8; i++)
            {
              Resources.Add(new Resource()
              {
                Food = dr.ReadSingle(),
                Wood = dr.ReadSingle(),
                Gold = dr.ReadSingle(),
                Stone = dr.ReadSingle(),
                Ore = dr.ReadSingle(),
                UnknownInt = dr.ReadInt32()
              });
              if (GetVersion() >= ScxVersion.Version122)
                Resources[i].PopulationLimit = dr.ReadSingle();
            }
            for (var i = 0; i < 9; i++)
            {
              var unitsCount = dr.ReadInt32();
              Units.Add(new List<Unit>(unitsCount));
              for (var j = 0; j < unitsCount; j++)
              {
                Units[i].Add(new Unit()
                {
                  PosX = dr.ReadSingle(),
                  PosY = dr.ReadSingle(),
                  PosZ = dr.ReadSingle(),
                  Id = dr.ReadInt32(),
                  UnitClass = dr.ReadInt16(),
                  State = dr.ReadByte(),
                  Rotation = dr.ReadSingle(),
                  Frame = dr.ReadInt16(),
                  Garrison = dr.ReadInt32()
                });
              }
            }
            Debug.Assert(dr.ReadInt32() == 9);
            for (var i = 0; i < 8; i++)
            {
              PlayerMiscs.Add(new PlayerMisc());
              PlayerMiscs[i].Name = dr.ReadBytes(dr.ReadInt16());
              PlayerMiscs[i].CameraX = dr.ReadSingle();
              PlayerMiscs[i].CameraY = dr.ReadSingle();
              PlayerMiscs[i].UnknownInt16_1 = dr.ReadInt16();
              PlayerMiscs[i].UnknownInt16_2 = dr.ReadInt16();
              PlayerMiscs[i].AlliedVictory = dr.ReadByte();
              dr.ReadInt16();
              for (var j = 0; j < 9; j++)
                PlayerMiscs[i].Diplomacies.Add((DiplomacyByte)dr.ReadByte());
              for (var j = 0; j < 9; j++)
                PlayerMiscs[i].Diplomacies2.Add((Diplomacy2)dr.ReadInt32());
              PlayerMiscs[i].Color = (PlayerColor)dr.ReadInt32();
              dr.ReadBytes((dr.ReadSingle() == 2.0f ? 8 : 0) + dr.ReadInt16() * 44 + 11);
            }
            var someDouble = dr.ReadDouble();
            if (someDouble == 1.6d)
              dr.ReadByte();
            var numTriggers = dr.ReadInt32();
            Triggers = new List<Trigger>(numTriggers);
            for (var i = 0; i < numTriggers; i++)
            {
              Triggers.Add(new Trigger()
              {
                IsEnabled = dr.ReadInt32(),
                IsLooping = dr.ReadInt32()
              });
              dr.ReadByte();
              Triggers[i].IsObjective = dr.ReadByte();
              Triggers[i].DiscriptionOrder = dr.ReadInt32();
              if (someDouble == 1.6d)
                dr.ReadInt32();
              Triggers[i].Discription = dr.ReadBytes(dr.ReadInt32());
              Triggers[i].Name = dr.ReadBytes(dr.ReadInt32());
              var numEffects = dr.ReadInt32();
              Triggers[i].Effects.Capacity = numEffects;
              for (var j = 0; j < numEffects; j++)
              {
                Triggers[i].Effects.Add(new Effect() { Type = (EffectType)dr.ReadInt32() });
                var numFields = dr.ReadInt32();
                for (var k = 0; k < numFields; k++)
                {
                  Triggers[i].Effects[j].Fields.Add(dr.ReadInt32());
                }
                Triggers[i].Effects[j].Text = dr.ReadBytes(dr.ReadInt32());
                Triggers[i].Effects[j].SoundFile = dr.ReadBytes(dr.ReadInt32());
                //if (Triggers[i].Effects[j].GetField(EffectField.NumSelected) > -1)
                //  Triggers[i].Effects[j].UnitIDs = new List<int>(Triggers[i].Effects[j].GetField(EffectField.NumSelected));
                for (var k = 0; k < Triggers[i].Effects[j].GetField(EffectField.NumSelected); k++)
                {
                  Triggers[i].Effects[j].UnitIDs.Add(dr.ReadInt32());
                }
              }
              Triggers[i].EffectsOrder.Capacity = numEffects;
              for (var j = 0; j < numEffects; j++)
              {
                Triggers[i].EffectsOrder.Add(dr.ReadInt32());
              }
              var numConditions = dr.ReadInt32();
              Triggers[i].Conditions.Capacity = numConditions;
              for (var j = 0; j < numConditions; j++)
              {
                Triggers[i].Conditions.Add(new Condition() { Type = (ConditionType)dr.ReadInt32() });
                var numFields = dr.ReadInt32();
                for (var k = 0; k < numFields; k++)
                {
                  Triggers[i].Conditions[j].Fields.Add(dr.ReadInt32());
                }
              }
              Triggers[i].ConditionsOrder.Capacity = numConditions;
              for (var j = 0; j < numConditions; j++)
              {
                Triggers[i].ConditionsOrder.Add(dr.ReadInt32());
              }
            }
            TriggersOrder = new List<int>(numTriggers);
            for (var i = 0; i < numTriggers; i++)
            {
              TriggersOrder.Add(dr.ReadInt32());
            }
            HasAiFile = dr.ReadInt32();
            unknownFlag = dr.ReadInt32();
            if (unknownFlag == 1)
              unknownBlock = dr.ReadBytes(396);
            if (HasAiFile == 1)
            {
              var numAiFiles = dr.ReadInt32();
              AiFiles = new Dictionary<byte[], byte[]>(numAiFiles);
              for (var i = 0; i < numAiFiles; i++)
              {
                AiFiles.Add(dr.ReadBytes(dr.ReadInt32()), dr.ReadBytes(dr.ReadInt32()));
              }
            }
          }
        }
      }
    }

    public MemoryStream GetStream()
    {
      var ret = new MemoryStream();
      var bw1 = new BinaryWriter(ret);
      bw1.Write(Version);
      bw1.Write(Instruction.Length + 20);
      bw1.Write(FormatVersion);
      bw1.Write(LastSave);
      bw1.Write(Instruction.Length);
      bw1.Write(Instruction);
      bw1.Write(0);
      bw1.Write(PlayerCount);
      if (FormatVersion == 3)
      {
        bw1.Write(1000);
        bw1.Write(1);
        bw1.Write(unknownInt32s.Count);
        foreach (var u in unknownInt32s)
          bw1.Write(u);
      }
      using (var ms = new MemoryStream())
      using (var bw = new BinaryWriter(ms))
      using (var cs = new DeflateStream(ret, CompressionMode.Compress, true))
      {
        bw.Write(NextUid);
        bw.Write(Version2);
        foreach (var p in Players)
          bw.Write(ZeroAppend(p.Name, 256));
        foreach (var p in Players)
          bw.Write(p.Name_StringTable);
        foreach (var p in Players)
        {
          bw.Write(p.IsActive);
          bw.Write(p.IsHuman);
          bw.Write(p.Civilization);
          bw.Write(4);
        }
        bw.Write(1);
        bw.Write((byte)0);
        bw.Write(-1.0f);
        bw.Write((short)OriginalFileName.Length);
        bw.Write(OriginalFileName);
        foreach (var s in StringTableInfos)
          bw.Write(s);
        foreach (var s in StringInfos)
        {
          bw.Write((short)s.Length);
          bw.Write(s);
        }
        bw.Write(HasBitmap);
        bw.Write(BitmapX);
        bw.Write(BitmapY);
        bw.Write((short)1);
        if (BitmapX > 0 && BitmapY > 0)
        {
          bw.Write(Bitmap.Size);
          bw.Write(Bitmap.Width);
          bw.Write(Bitmap.Height);
          bw.Write(Bitmap.Planes);
          bw.Write(Bitmap.BitCount);
          bw.Write(Bitmap.Compression);
          bw.Write(Bitmap.SizeImage);
          bw.Write(Bitmap.XPelsPerMeter);
          bw.Write(Bitmap.YPelsPerMeter);
          bw.Write(Bitmap.ClrUsed);
          bw.Write(Bitmap.ClrImportant);
          foreach (var rgb in Bitmap.Colors)
          {
            bw.Write(rgb.Red);
            bw.Write(rgb.Green);
            bw.Write(rgb.Blue);
            bw.Write((byte)0);
          }
          bw.Write(Bitmap.ImageData);
        }
        bw.Seek(64, SeekOrigin.Current);
        foreach (var p in Players)
        {
          bw.Write((short)p.Ai.Length);
          bw.Write(p.Ai);
        }
        foreach (var p in Players)
        {
          bw.Write(0L);
          bw.Write(p.AiFile.Length);
          bw.Write(p.AiFile);
        }
        foreach (var p in Players)
          bw.Write((byte)p.Personality);
        bw.Write(-99);
        foreach (var p in Players)
        {
          bw.Write(p.Gold);
          bw.Write(p.Wood);
          bw.Write(p.Food);
          bw.Write(p.Stone);
          bw.Write(p.Ore);
          bw.Write(0);
          if (GetVersion() >= ScxVersion.Version124)
            bw.Write(p.PlayerNumber);
        }
        bw.Write(-99);
        bw.Write(Conquest);
        bw.Write(Relics);
        bw.Write(Explored);
        bw.Write(AllMustMeet);
        bw.Write((int)VictoryMode);
        bw.Write(Score);
        bw.Write(Time);
        foreach (var p in Players)
          foreach (var d in p.Diplomacies)
            bw.Write((int)d);
        bw.Seek(11520, SeekOrigin.Current);
        bw.Write(-99);
        foreach (var p in Players)
          bw.Write(p.AlliedVictory);
        if (GetVersion() >= ScxVersion.Version123)
        {
          bw.Write(LockTeams);
          bw.Write(PlayerChooseTeams);
          bw.Write(RandomStartPoints);
          bw.Write(MaxTeams);
        }
        foreach (var p in Players)
          bw.Write(p.NumDisabledTechs);
        foreach (var p in Players)
          foreach (var d in p.DisabledTechs)
            bw.Write(d);
        foreach (var p in Players)
          bw.Write(p.NumDisabledUnits);
        foreach (var p in Players)
          foreach (var d in p.DisabledUnits)
            bw.Write(d);
        foreach (var p in Players)
          bw.Write(p.NumDisabledBuildings);
        foreach (var p in Players)
          foreach (var d in p.DisabledBuildings)
            bw.Write(d);
        bw.Write(0L);
        bw.Write(AllTechs);
        foreach (var p in Players)
          bw.Write((int)p.StartAge + (GetVersion() > ScxVersion.Version126 ? 2 : 0));
        bw.Write(-99);
        bw.Write(CameraX);
        bw.Write(CameraY);
        if (GetVersion() >= ScxVersion.Version122)
          bw.Write((int)MapType);
        if (GetVersion() >= ScxVersion.Version124)
          bw.Write(0m);
        bw.Write(MapX);
        bw.Write(MapY);
        foreach (var m in Map)
        {
          bw.Write(m.Id);
          bw.Write(m.Elevation);
        }
        bw.Write(9);
        foreach (var r in Resources)
        {
          bw.Write(r.Food);
          bw.Write(r.Wood);
          bw.Write(r.Gold);
          bw.Write(r.Stone);
          bw.Write(r.Ore);
          bw.Write(r.UnknownInt);
          if (GetVersion() >= ScxVersion.Version122)
            bw.Write(r.PopulationLimit);
        }
        foreach (var units in Units)
        {
          bw.Write(units.Count);
          foreach (var u in units)
          {
            bw.Write(u.PosX);
            bw.Write(u.PosY);
            bw.Write(u.PosZ);
            bw.Write(u.Id);
            bw.Write(u.UnitClass);
            bw.Write(u.State);
            bw.Write(u.Rotation);
            bw.Write(u.Frame);
            bw.Write(u.Garrison);
          }
        }
        bw.Write(9);
        foreach (var m in PlayerMiscs)
        {
          bw.Write((short)m.Name.Length);
          bw.Write(m.Name);
          bw.Write(m.CameraX);
          bw.Write(m.CameraY);
          bw.Write(m.UnknownInt16_1);
          bw.Write(m.UnknownInt16_2);
          bw.Write(m.AlliedVictory);
          bw.Write((short)9);
          foreach (var d in m.Diplomacies)
            bw.Write((byte)d);
          foreach (var d in m.Diplomacies2)
            bw.Write((int)d);
          bw.Write((int)m.Color);
          bw.Write(2.0f);
          bw.Seek(17, SeekOrigin.Current);
          bw.Write(-1);
        }
        bw.Write(1.6);
        bw.Write((byte)0);
        bw.Write(Triggers.Count);
        foreach (var t in Triggers)
        {
          bw.Write(t.IsEnabled);
          bw.Write(t.IsLooping);
          bw.Write((byte)0);
          bw.Write(t.IsObjective);
          bw.Write(t.DiscriptionOrder);
          bw.Write(0);
          bw.Write(t.Discription.Length);
          bw.Write(t.Discription);
          bw.Write(t.Name.Length);
          bw.Write(t.Name);
          bw.Write(t.Effects.Count);
          foreach (var e in t.Effects)
          {
            bw.Write((int)e.Type);
            bw.Write(e.Fields.Count);
            foreach (var f in e.Fields)
              bw.Write(f);
            bw.Write(e.Text.Length);
            bw.Write(e.Text);
            bw.Write(e.SoundFile.Length);
            bw.Write(e.SoundFile);
            foreach (var u in e.UnitIDs)
              bw.Write(u);
          }
          foreach (var e in t.EffectsOrder)
            bw.Write(e);
          bw.Write(t.Conditions.Count);
          foreach (var c in t.Conditions)
          {
            bw.Write((int)c.Type);
            bw.Write(c.Fields.Count);
            foreach (var f in c.Fields)
              bw.Write(f);
          }
          foreach (var c in t.ConditionsOrder)
            bw.Write(c);
        }
        foreach (var t in TriggersOrder)
          bw.Write(t);
        bw.Write(HasAiFile);
        bw.Write(unknownFlag);
        if (unknownFlag == 1)
          bw.Write(unknownBlock);
        if (HasAiFile == 1)
        {
          bw.Write(AiFiles.Count);
          foreach (var a in AiFiles)
          {
            bw.Write(a.Key.Length);
            bw.Write(a.Key);
            bw.Write(a.Value.Length);
            bw.Write(a.Value);
          }
        }
        ms.Seek(0, SeekOrigin.Begin);
        ms.CopyTo(cs);
      }
      ret.Seek(0, SeekOrigin.Begin);
      return ret;
    }

    public void Save()
    {
      using (var fs = new FileStream(FileName, FileMode.Create))
      {
        GetStream().CopyTo(fs);
      }
    }

    public void SaveAs(string fileName)
    {
      using (var fs = new FileStream(fileName, FileMode.Create))
      {
        GetStream().CopyTo(fs);
      }
    }

    private ScxVersion GetVersion()
    {
      if (Version[2] == 0x31)
        return ScxVersion.Version118;
      else if (Version2 < 1.2201f)
        return ScxVersion.Version122;
      else if (Version2 < 1.2301f)
        return ScxVersion.Version123;
      else if (Version2 < 1.2401f)
        return ScxVersion.Version124;
      else
        return ScxVersion.Version126;
    }

    private static byte[] ZeroAppend(byte[] input, int totalLength)
    {
      return input.Concat(new byte[totalLength - input.Length]).ToArray();
    }
  }

  public class Player
  {
    public byte[] Name { get; set; }
    public int Name_StringTable { get; set; }
    public int IsActive { get; set; }
    public int IsHuman { get; set; }
    public int Civilization { get; set; }
    public byte[] Ai { get; set; }
    public byte[] AiFile { get; set; }
    public Personality Personality { get; set; }
    public int Gold { get; set; }
    public int Wood { get; set; }
    public int Food { get; set; }
    public int Stone { get; set; }
    public int Ore { get; set; }
    public int PlayerNumber { get; set; }
    public List<DiplomacyInt> Diplomacies { get; }
    public int AlliedVictory { get; set; }
    public int NumDisabledTechs { get; set; }
    public List<int> DisabledTechs { get; }
    public int NumDisabledUnits { get; set; }
    public List<int> DisabledUnits { get; }
    public int NumDisabledBuildings { get; set; }
    public List<int> DisabledBuildings { get; }
    public StartAge StartAge { get; set; }

    public Player()
    {
      Diplomacies = new List<DiplomacyInt>(16);
      DisabledTechs = new List<int>(30);
      DisabledUnits = new List<int>(30);
      DisabledBuildings = new List<int>(30);
    }
  }

  public class BITMAPDIB
  {
    public int Size { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Planes { get; set; }
    public int BitCount { get; set; }
    public int Compression { get; set; }
    public int SizeImage { get; set; }
    public int XPelsPerMeter { get; set; }
    public int YPelsPerMeter { get; set; }
    public int ClrUsed { get; set; }
    public int ClrImportant { get; set; }
    public List<RGB> Colors { get; set; }
    public byte[] ImageData { get; set; }
  }

  public class RGB
  {
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
  }

  public class Terrain
  {
    public byte Id { get; set; }
    public short Elevation { get; set; }
  }

  public class Resource
  {
    public float Food { get; set; }
    public float Wood { get; set; }
    public float Gold { get; set; }
    public float Stone { get; set; }
    public float Ore { get; set; }
    public int UnknownInt { get; set; }
    public float PopulationLimit { get; set; }
  }

  public class PlayerMisc
  {
    public byte[] Name { get; set; }
    public float CameraX { get; set; }
    public float CameraY { get; set; }

    public short UnknownInt16_1 { get; set; }
    public short UnknownInt16_2 { get; set; }
    public byte AlliedVictory { get; set; }
    public List<DiplomacyByte> Diplomacies { get; }
    public List<Diplomacy2> Diplomacies2 { get; }
    public PlayerColor Color { get; set; }

    public PlayerMisc()
    {
      Diplomacies = new List<DiplomacyByte>(9);
      Diplomacies2 = new List<Diplomacy2>(9);
    }
  }



  /// <summary>
  /// Player personality.
  /// </summary>
  public enum Personality : byte
  {
    Custom,
    Standard,
    None
  }

  /// <summary>
  /// Victory mode.
  /// </summary>
  public enum VictoryMode
  {
    Standard,
    Conquest,
    Score,
    Time,
    Custom,
  }

  public enum DiplomacyInt
  {
    Allied,
    Neutral,
    Enemy = 3
  }

  public enum DiplomacyByte : byte
  {
    Allied,
    Newtral,
    Enemy = 3
  }

  public enum Diplomacy2
  {
    Gaia,
    Self,
    Allied,
    Neutral,
    Enemy
  }

  /// <summary>
  /// Player start age.
  /// </summary>
  public enum StartAge
  {
    None = -1,
    Dark,
    Feudal,
    Castle,
    Imperial,
    PostImperial,
  }

  /// <summary>
  /// Scx file spec version.
  /// </summary>
  public enum ScxVersion
  {
    Version118,
    Version122,
    Version123,
    Version124,
    Version126,
    Unknown,
  }

  /// <summary>
  /// 
  /// </summary>
  public enum MapType
  {
    Arabia = 9,
    Archipelago,
    Baltic,
    BlackForest,
    Coastal,
    Continental,
    CraterLake,
    Fortress,
    GoldRush,
    Highland,
    Islands,
    Mediterranean,
    Migration,
    Rivers,
    TeamIslands,
    Scandinavia = 0x19,
    Yucatan = 0x1B,
    SaltMarsh,
    KingOfTheHill = 0x1E,
    Oasis,
    Nomad = 0x21,
  }

  /// <summary>
  /// Player colors.
  /// </summary>
  public enum PlayerColor
  {
    Blue,
    Red,
    Green,
    Yellow,
    Cyan,
    Purple,
    Gray,
    Orange,
  }



}
