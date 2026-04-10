# barcc
Blender Async Render Command Creator (And Runner)

# What Is This???
Well, it's a small program, that takes an input, and executes that input.

So if you want to render an animation in Blender on multiple GPUs, it's more efficient to run a Blender instance per GPU.

# How It Works
Each GPU renders ever Nth frame. So if you have 2 GPUs, GPU 1 renders frame 0, 2, 4, 6. And GPU 2 renders frame 1, 3, 5, 7, etc...

# How To Use
To run, run this in the terminal, where the `barcc` executable is placed:

```
./barcc 2 250 HIP true path/to/your/blend/file/.blend /path/to/your/render/folder /path/to/the/blender/executable/blender-5.1.0-linux-x64/blender
```
*Note: The Blender path should also include "blender" at the end. It shouldn't just be the folder*

Command options:
1. amount of GPUs
2. end frame for the animation
3. device backend (HIP, CUDA, etc...)
4. enable debug printing
5. the path to the project that we're going to render
6. output path for the images to be rendered
7. path to Blender's executable file
