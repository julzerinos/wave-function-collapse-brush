import numpy as np
import itertools
import os

def create_tile_file(combination, combination_removal,combination_scale_noise,combination_min_color,combination_height_max):
    
    filepath = os.path.join(directory, f'tile_{combination}_{combination_removal:.2f}_{combination_scale_noise:.2f}_{combination_min_color:.2f}_{combination_height_max:.2f}.blend')
    return filepath


  
 

directory = os.path.expanduser("~/Documents/DTU")

permutations = list(itertools.product([0, 1], repeat=6))

permutations_removal = [np.random.uniform(0.4,0.6) for _ in range(len(permutations))]

permutations_scale_noise = [np.random.uniform(0.1,1.1) for _ in range(len(permutations))]

permutations_min_color = [np.random.uniform(0.5,1.0) for _ in range(len(permutations))]

permutations_height_max = [np.random.uniform(4.5,6.5) for _ in range(len(permutations))]

number_of_tiles = 4

file_paths_set = set()

for i in np.random.choice(len(permutations), number_of_tiles, replace=False):
    combination = 1000000
    for j, r in enumerate(permutations[i]):
        combination += int(r) * 10**j
    combination_removal = np.random.uniform(0.4,0.6)
    combination_scale_noise = np.random.uniform(0.1,1.1)
    combination_min_color = np.random.uniform(0.5,1.0)
    combination_height_max = np.random.uniform(4.5,6.5)

    filepath = create_tile_file(combination, combination_removal,combination_scale_noise,combination_min_color, combination_height_max)
    file_paths_set.add(filepath);
    print(f"Created file: {filepath}")

print(len(file_paths_set))