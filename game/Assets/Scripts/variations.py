import bpy

import numpy as np

import itertools

import random
 
def create_tile_file(combination, combination_removal, combination_scale_noise, combination_min_color, combination_height_max):

    obj.modifiers["GeometryNodes"][idt_types] = combination   
    obj.modifiers["NewHex"][removal_tiles] = combination_removal
    obj.modifiers["NewHex"][scale_noise] = combination_scale_noise
    obj.modifiers["NewHex"][min_color] = combination_min_color
    obj.modifiers["NewHex"][height_max] = combination_height_max


    bpy.ops.wm.save_mainfile(filepath=f'tile_{combination}_{combination_removal:.2f}_{combination_scale_noise:.2f}_{combination_min_color:.2f}_{combination_height_max:.2f}.blend')
 
obj = bpy.data.objects['Plane']

idt_types = obj.modifiers["GeometryNodes"].node_group.interface.items_tree["Types"].identifier

removal_tiles = obj.modifiers["NewHex"].node_group.interface.items_tree["Removal"].identifier

scale_noise = obj.modifiers["NewHex"].node_group.interface.items_tree["Scale Noise"].identifier

min_color = obj.modifiers["NewHex"].node_group.interface.items_tree["Min Color"].identifier

height_max = obj.modifiers["NewHex"].node_group.interface.items_tree["HeightMax"].identifier
 
permutations = list(itertools.product([0, 1], repeat=6))

permutations_removal = [random.uniform(0.4,0.6) for _ in range(len(permutations))]

permutations_scale_noise = [np.random.uniform(0.1,1.1) for _ in range(len(permutations))]

permutations_min_color = [np.random.uniform(0.5,1.0) for _ in range(len(permutations))]

permutations_height_max = [np.random.uniform(4.5,6.5) for _ in range(len(permutations))]

 
number_of_tiles = 4

#shouldn't that be for i in range(len(permutations)) instead? since we want to get all the combinations of 0,1?

for i in np.random.choice(len(permutations), number_of_tiles, replace=False):

    combination = 1000000

    for j, r in enumerate(permutations[i]):

        combination += int(r) * 10**j

    combination_removal = permutations_removal[i]

    combination_scale_noise = permutations_scale_noise[i]

    combination_min_color = permutations_min_color[i]

    combination_height_max = permutations_height_max[i]

    create_tile_file(combination, combination_removal, combination_scale_noise, combination_min_color, combination_height_max)
