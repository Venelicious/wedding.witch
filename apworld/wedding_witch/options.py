from dataclasses import dataclass
from Options import Choice, Range, PerGameCommonOptions, StartInventoryPool, DeathLink
class TransformEnd(Range):
    """Number of DIFFERENT endings to win on the selected difficulty. Normal (untransformed) is one of seven. Each new ending up to this target sends one milestone check."""
    display_name = "Different Transform Endings"
    range_start = 1
    range_end = 7
    default = 1
class GoalDifficulty(Choice):
    """Difficulty on which the distinct ending goal must be completed. All difficulties still have checks."""
    option_normal = 0
    option_hard = 1
    option_nightmare = 2
    default = 0
class StartingExpType(Choice):
    """One potion type starts unlocked; the other five are AP items. Auto chooses the starting type randomly."""
    option_auto = 0
    option_bigbreast = 1
    option_smallbreast = 2
    option_corruption = 3
    option_beast = 4
    option_muscle = 5
    option_hip = 6
    default = 0
class FlowerChecks(Range):
    """Cumulative natural flower pickups on this difficulty across runs. -1 distributes the remaining item budget automatically. Explicit values must leave exactly enough checks for the item pool."""
    range_start = -1
    range_end = 500
    default = -1
@dataclass
class WeddingWitchOptions(PerGameCommonOptions):
    start_inventory_from_pool: StartInventoryPool
    death_link: DeathLink
    transformEnd: TransformEnd
    difficulty: GoalDifficulty
    starting_exp_type: StartingExpType
    flower_checks_normal: FlowerChecks
    flower_checks_hard: FlowerChecks
    flower_checks_nightmare: FlowerChecks
