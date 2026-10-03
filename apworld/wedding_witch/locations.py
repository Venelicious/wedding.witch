from .constants import BASE_ID, DIFFICULTIES, FORMS, FLOWER_MAX
from .achievements import ACHIEVEMENTS, achievement_location
LOCATION_NAME_TO_ID = {f"{name} Map {n}": BASE_ID + d * 100 + n for name, d, count in DIFFICULTIES for n in range(1, count + 1)}
LOCATION_NAME_TO_ID.update({f"Full Transformation {name}": BASE_ID + 400 + body for name, body in FORMS})
LOCATION_NAME_TO_ID.update({f"{name} Flower {n}": BASE_ID + 5000 + d * 1000 + n for name, d, _ in DIFFICULTIES for n in range(1, FLOWER_MAX + 1)})
LOCATION_NAME_TO_ID.update({f"Transform Ending {n}": BASE_ID + 8000 + n for n in range(1, 8)})
LOCATION_NAME_TO_ID.update({achievement_location(label): BASE_ID + 9000 + i for i, (_, label, _, _) in enumerate(ACHIEVEMENTS, 1)})
def locations_by_region(flowers, endings, difficulty):
    result = {name: [f"{name} Map {n}" for n in range(1, count + 1)] + [f"{name} Flower {n}" for n in range(1, flowers[d] + 1)] for name, d, count in DIFFICULTIES}
    result["Transformations"] = [f"Full Transformation {name}" for name, _ in FORMS]
    result["Achievements"] = [achievement_location(label) for _, label, _, _ in ACHIEVEMENTS]
    result[difficulty] += [f"Transform Ending {n}" for n in range(1, endings + 1)]
    return result
