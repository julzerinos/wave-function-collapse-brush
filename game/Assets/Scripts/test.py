import numpy as np
import itertools

def create_tile_file(combination, combination_removal,combination_scale_noise):
    filepath = f'tile_{combination}_{combination_removal:.2f}_{combination_scale_noise:.2f}.blend'
    return filepath

permutations = list(itertools.product([0, 1], repeat=6))

permutations_removal = [np.random.uniform(0.4,0.6) for _ in range(len(permutations))]

permutations_scale_noise = [np.random.uniform(0.1,1.1) for _ in range(len(permutations))]

number_of_tiles = 4

for i in range(len(permutations)):
    combination = 1000000
    for j, r in enumerate(permutations[i]):
        combination += int(r) * 10**j
    combination_removal = permutations_removal[i]
    combination_scale_noise = permutations_scale_noise[i]
    filepath = create_tile_file(combination, combination_removal,combination_scale_noise)
    print(f"Created file: {filepath}")
