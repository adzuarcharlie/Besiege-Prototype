This project is a Besiege-like vehicle construction prototype, including a few levels to test different mechanics
=

When you launch the game, you first have to build a vehicle using the construction editor
-

You can **move the camera around by moving the mouse while holding any mouse button down**.
When you select a piece, you can place it by using the **left-click** wherever you want on the vehicle.
You can delete a piece whenever you want by using the **right-click** on it.

There are 4 types of pieces :
* basics : those are simply blocks used to make the volume of the vehicle and put useful pieces on them.
* Movement : this is an engine. You put it on the vehicle so it can move. *Multiple engines won't change anything*.
* Weapons : those are canons. There is a **simple canon** by default and a **hold-canon** is added with the update. *This is explained below*.
* Misc : this is a booster. When you active it, it gives an impulsion in the direction according to its rotation.

For the pieces whose orientation is important (weapons and boosters), you cannot directly control it :
the orientation depends on where you put it. This is detailed in the Booster and Canon scripts.

To be able to use your vehicle, you need to **save** it.
You can also load any saved vehicle in the editor to modify it or in the level selection menu to use it.
You can also delete any saved vehicle.
*Sample vehicles are already registered in order to show what is possible*

There is an update system
-

By default, the game has version 0.
If you want to change it, you simply need to put all files from the **UpdateBackups/version1** folder to the **Updates** folder and the game will update itself.
You can also revert to version 0 by putting all files from the **UpdateBackups/version0** folder to the **Updates** folder.

The version 1 adds a piece : the **hold-canon**, and a level.

Once you have loaded a vehicle, you can select a level and launch it
-

Here are the controls :
* Z,Q,S,D : move the vehicle. *Only works if there is an __engine__*.
* R : restart the level. *There is also a **Restart** button*
* Space : activate the boosters.
* Left-click : Shoot with normal canons.
* Right-click : Holding the key will charge the *hold-canons* (with a maximum of 4 seconds). Releasing the key will then make them shoot.

You can do a level only if you have completed every level before. I let a save where they are all available.
*There is a **Restart Progress** button in the level selection menu*.

Here is the list of levels :
1. In the first level you simply have to reach the *green zone*.
2. Same, but there is a wall blocking. You need to put a *booster on the bottom of a piece* in order to get an impulse upwards.
3. You have to reach the platform in front of you, then another one will appear. Once this second platform is reached, a last one will appear with the *green zone* you need to reach to end the level. *Note that the step zones are low, so it is a bit tricky to activate them*
4. There is a target in the square in front of you. You need to shoot inside it from above. *o do so, you have to put a *canon in front of a piece* in order to have it facing downwards (or use the basketteer vehicle).
5. There are 3 targets to shoot. You need a booster to jump (like in level 2) and reach the targets' height.
6. Same, but the targets move.
7. *Added with version 1*. The targets are now on the ground. You therefore have to use the *hold-canons* with the correct force in order to shoot them.

*This project was made with Unity 2019.2.11f1, there is a build where you can launch*