"""Contract/budget tests plus generation tests using real Archipelago classes."""
import importlib, json, pathlib, random, re, sys, unittest
ROOT = pathlib.Path(__file__).resolve().parents[1]
from worlds.wedding_witch import WeddingWitchWorld, allocate_flowers
from worlds.wedding_witch.constants import *
from worlds.wedding_witch.items import *
from worlds.wedding_witch.locations import *
from worlds.wedding_witch.options import WeddingWitchOptions
from BaseClasses import MultiWorld, CollectionState

class RulesTests(unittest.TestCase):
    def test_full_budget_all_goals(self):
        for goal in range(1,8):
            flowers=allocate_flowers([-1,-1,-1],goal)
            self.assertEqual(142,18+6+goal+sum(flowers))
            self.assertEqual(142,len(item_pool_names('Beast')))
    def test_custom_flowers(self):
        self.assertEqual([10,20,85],allocate_flowers([10,20,85],3))
    def test_invalid_budget(self):
        for config in ([0,0,0],[100,100,100],[500,500,-1]):
            with self.assertRaises(Exception): allocate_flowers(config,3)
    def test_progressive_skill_ranks(self):
        from collections import Counter
        pool=Counter(item_pool_names('Beast'))
        self.assertEqual(62,sum(pool[f"Skill Unlock: {name}"] for _,name,_ in SKILLS))
        self.assertFalse(any(key.endswith('Mastery') for key,_,_ in SKILLS))
        for _,name,cap in SKILLS: self.assertEqual(cap,pool[f"Skill Unlock: {name}"])
    def test_ids_unique(self):
        for table in (ITEM_NAME_TO_ID,LOCATION_NAME_TO_ID): self.assertEqual(len(table),len(set(table.values())))
    def test_skill_class_contract(self):
        text=(ROOT/'src'/'SkillCatalog.cs').read_text()
        entries=[(key,name,int(cap)) for key,name,cap in re.findall(r'\("(\w+)", "([^"]+)", (\d+)\)',text)]
        self.assertEqual(list(SKILLS),entries)
    def test_upgrade_contract(self):
        text=(ROOT/'src'/'UpgradeCatalog.cs').read_text()
        entries=[(key,name,int(cap)) for key,name,cap in re.findall(r'new Upgrade\("([^"]+)", "([^"]+)", (\d+)\)',text)]
        self.assertEqual(list(UPGRADES),entries)
        self.assertEqual(73,sum(x[2] for x in entries))
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
                self.assertEqual(142,len(m.itempool))
                self.assertEqual(142,len([x for x in m.get_locations(1) if x.address]))
                self.assertEqual(1,len(m.precollected_items[1]))
                state=CollectionState(m)
                for item in m.itempool: state.collect(item,True)
                state.sweep_for_advancements()
                self.assertTrue(m.completion_condition[1](state))
                self.assertTrue(all(x.can_reach(state) for x in m.get_locations(1)))
                self.assertEqual(2,w.fill_slot_data()['schema_version'])
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
if __name__=='__main__': unittest.main()
