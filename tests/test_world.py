"""Contract/budget tests plus generation tests using real Archipelago classes."""
import importlib, json, pathlib, random, re, sys, unittest
ROOT = pathlib.Path(__file__).resolve().parents[1]
from worlds.wedding_witch import WeddingWitchWorld, allocate_flowers
from worlds.wedding_witch.constants import *
from worlds.wedding_witch.items import *
from worlds.wedding_witch.locations import *
from worlds.wedding_witch.achievements import ACHIEVEMENTS, achievement_location
from worlds.wedding_witch.options import WeddingWitchOptions
from BaseClasses import MultiWorld, CollectionState

class RulesTests(unittest.TestCase):
    def test_full_budget_all_goals(self):
        for goal in range(1,8):
            flowers=allocate_flowers([-1,-1,-1],goal)
            self.assertEqual(80,18+6+goal+len(ACHIEVEMENTS)+sum(flowers))
            self.assertEqual(62,80-goal-sum(flowers))
            self.assertEqual(80,len(item_pool_names('Beast')))
    def test_custom_flowers(self):
        self.assertEqual([4,5,6],allocate_flowers([4,5,6],3))
    def test_invalid_budget(self):
        for config in ([0,0,0],[100,100,100],[500,500,-1]):
            with self.assertRaises(Exception): allocate_flowers(config,3)
    def test_skills_removed_from_ap_pool(self):
        from collections import Counter
        pool=Counter(item_pool_names('Beast'))
        self.assertFalse(any(name.startswith('Skill Unlock:') for name in pool))
        self.assertFalse(any(name.startswith('Skill Unlock:') for name in ITEM_NAME_TO_ID))
        self.assertNotIn('Skills',ITEM_NAME_GROUPS)
        self.assertEqual(73,sum(pool[name] for _,name,_ in UPGRADES))
    def test_ids_unique(self):
        for table in (ITEM_NAME_TO_ID,LOCATION_NAME_TO_ID): self.assertEqual(len(table),len(set(table.values())))
    def test_native_skill_slot_contract(self):
        _,w=self.make_world(3,0,25)
        slot=w.fill_slot_data()
        self.assertEqual('level_up',slot['skill_mode'])
        self.assertEqual(80,slot['pool_size'])
        self.assertNotIn('skill_caps',slot)
        self.assertNotIn('skill_classes',slot)
    def test_upgrade_contract(self):
        text=(ROOT/'src'/'UpgradeCatalog.cs').read_text()
        entries=[(key,name,int(cap)) for key,name,cap in re.findall(r'new Upgrade\("([^"]+)", "([^"]+)", (\d+)\)',text)]
        self.assertEqual(list(UPGRADES),entries)
        self.assertEqual(73,sum(x[2] for x in entries))
    def test_achievement_contract(self):
        text=(ROOT/'src'/'AchievementCatalog.cs').read_text()
        entries=re.findall(r'\("([^"]+)", "([^"]+)"\)',text)
        self.assertEqual([(key,label) for key,label,_,_ in ACHIEVEMENTS],entries)
        self.assertEqual(38,len(entries))
        self.assertEqual(38,len(set(key for key,_ in entries)))
    def make_world(self,goal,difficulty,seed):
        m=MultiWorld(1); m.game[1]='Wedding Witch'; m.player_name={1:'Test'}; m.set_seed(seed)
        w=WeddingWitchWorld(m,1); m.worlds[1]=w
        # AP options use constructors via from_any, as the real generator does.
        from typing import get_type_hints
        annotations=get_type_hints(WeddingWitchOptions)
        defaults={key:typ.from_any(getattr(typ,'default',0)) for key,typ in annotations.items()}
        defaults['transformEnd']=annotations['transformEnd'].from_any(goal)
        defaults['difficulty']=annotations['difficulty'].from_any(difficulty)
        w.options=WeddingWitchOptions(**defaults)
        m.state=CollectionState(m)
        w.generate_early(); w.create_regions(); w.create_items(); w.set_rules()
        return m,w
    def test_all_goals_difficulties(self):
        for goal in range(1,8):
            for difficulty in range(3):
                m,w=self.make_world(goal,difficulty,100+goal+difficulty)
                self.assertEqual(80,len(m.itempool))
                self.assertEqual(80,len([x for x in m.get_locations(1) if x.address]))
                self.assertEqual(1,len(m.precollected_items[1]))
                state=CollectionState(m)
                for item in m.itempool: state.collect(item,True)
                state.sweep_for_advancements()
                self.assertTrue(m.completion_condition[1](state))
                self.assertTrue(all(x.can_reach(state) for x in m.get_locations(1)))
                self.assertEqual(4,w.fill_slot_data()['schema_version'])
                self.assertEqual([key for key,_,_,_ in ACHIEVEMENTS],w.fill_slot_data()['achievement_checks'])
                self.assertEqual(38,len(m.get_region('Achievements',1).locations))
    def test_early_normal_has_checks(self):
        m,w=self.make_world(7,2,1)
        state=CollectionState(m)
        self.assertTrue(m.get_location('Normal Map 1',1).can_reach(state))
        self.assertFalse(m.get_location('Hard Map 1',1).can_reach(state))
        self.assertFalse(m.get_location('Nightmare Map 1',1).can_reach(state))
    def test_normal_ending_two_requires_mix(self):
        m,w=self.make_world(2,0,1); state=CollectionState(m)
        self.assertFalse(m.get_location('Transform Ending 2',1).can_reach(state))
        other=next(e for e in EXP_TYPES if e!=w.starting_exp)
        state.collect(w.create_item('EXP Unlock: '+other),True)
        self.assertTrue(m.get_location('Transform Ending 2',1).can_reach(state))
    def test_achievement_requirements(self):
        m,w=self.make_world(3,0,19); state=CollectionState(m)
        self.assertTrue(m.get_location('Achievement: Defeat 1000 Enemies',1).can_reach(state))
        for difficulty in ('Hard','Nightmare'):
            location=m.get_location('Achievement: Clear '+difficulty,1)
            self.assertFalse(location.can_reach(state))
            state.collect(w.create_item(difficulty+' Wedding'),True)
            self.assertTrue(location.can_reach(state))
        for _,label,_,exp in ACHIEVEMENTS:
            if not exp or exp==w.starting_exp: continue
            self.assertFalse(m.get_location(achievement_location(label),1).can_reach(state))
        for exp in EXP_TYPES: state.collect(w.create_item('EXP Unlock: '+exp),True)
        self.assertTrue(all(m.get_location(achievement_location(label),1).can_reach(state) for _,label,_,_ in ACHIEVEMENTS))
if __name__=='__main__': unittest.main()
