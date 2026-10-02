using System;
using WeddingWitchArchipelago;
class ProgressTests {
 static int checks;
 static void Assert(bool ok,string label) { if(!ok)throw new Exception(label); checks++; }
 static void Main() {
  var p=new ProgressLedger();
  p.StartRun(0); p.Win("Normal");
  Assert(p.Wins[0]==1,"Normal win");
  Assert(p.ConfirmEnding(out var d,out var n)&&d==0&&n==1,"Normal ending");
  Assert(!p.ConfirmEnding(out _,out _),"duplicate scene callback");
  p.StartRun(0); p.Win("Normal");
  Assert(!p.ConfirmEnding(out _,out _),"same ending on a second win");
  Assert(p.Wins[0]==2,"wins still count duplicates");
  p.StartRun(1); p.Win("Beast");
  Assert(p.ConfirmEnding(out d,out n)&&d==1&&n==1,"difficulty separated");
  Assert(p.Endings[0].Count==1&&p.Endings[1].Count==1,"independent sets");
  p.StartRun(2); p.RunActive=false;
  Assert(!p.ConfirmEnding(out _,out _),"death never counts as ending");
  p.StartRun(0); p.Win("Hip"); p.Win("Hip");
  Assert(p.Wins[0]==3,"duplicate victory ignored");
  Assert(p.ConfirmEnding(out d,out n)&&n==2,"different ending adds a milestone");
  p.Flowers[0]=12; p.StartRun(0);
  Assert(p.Flowers[0]==12,"flowers persist across runs");
  p.StartRun(1); Assert(p.Flowers[1]==0,"flower difficulty split");
  p.Win("Beast"); p.StartRun(1);
  Assert(!p.ConfirmEnding(out _,out _),"stale ending removed on new run");
  Assert(SkillCatalog.Cap("AbsorptionRadiusUp")==3,"absorption max 3");
  Assert(SkillCatalog.Cap("BouquetAttackPowerUp")==6,"bouquet power max 6");
  int total=0; foreach(var skill in SkillCatalog.All) { total+=skill.MaxLevel; Assert(!skill.Class.EndsWith("Mastery"),"no mastery item"); }
  Assert(total==62,"62 progressive skill ranks");
  var oldRoot = BepInEx.Paths.BepInExRootPath;
  ApProfile.Enter("seed-a:0:1","WitchTester");
  ApProfile.Set("mod","progress","flowers=7");
  ApProfile.Flush(true);
  ApProfile.Leave();
  ApProfile.Enter("seed-b:0:1","WitchTester");
  Assert(!ApProfile.TryGet("mod","progress",out _),"different seed isolated");
  ApProfile.Leave();
  ApProfile.Enter("seed-a:0:1","WitchTester");
  Assert(ApProfile.TryGet("mod","progress",out var stored)&&stored=="flowers=7","progress survives reopening");
  ApProfile.Set("mod","progress","flowers=8"); ApProfile.Flush(true);
  Assert(System.IO.Directory.GetFiles(System.IO.Path.Combine(oldRoot,"WeddingWitchArchipelago"),"*.bak").Length==1,"atomic backup created");
  ApProfile.Leave();
  ApProfile.Enter("seed-a:1:1","WitchTester");
  Assert(!ApProfile.TryGet("mod","progress",out _),"different team isolated");
  ApProfile.Leave();
  Console.WriteLine($"PASS {checks} progress assertions");
 }
}
