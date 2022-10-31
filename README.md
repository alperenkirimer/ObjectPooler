
# Object Pooler for Unity

This project contains the Unity C# scripts to build an [Object Pooler](https://learn.unity.com/tutorial/introduction-to-object-pooling).

## Features

- Classical object pooling
- Automatic pool size increment available
- Particle System supported

## Installation

  Just download the scripts and add to your Unity project.

## Glossary

#### Object Pool Manager

The main component that handles the core logic.

Note that all prefabs DO NOT need to be pooled. Only the ones that are created and removed very frequently, like the bullets in a shooter game.


#### Object Pool

An object pool keeps the required parameters and the lists of intances of the pooled object.

#### In Reserve / In Use

The intances that are waiting in the pool are in reserve. The ones that are actively used in the scene are in use. In Reserve / In Use counts of each pool can be seen in Pool Summary (after the manager is initialized).

#### Give / Recycle

Instead of instantiating a new object, we get an already instantiated object from the pool by  `Give()`.
Instead of destroying an object, we send it back to the pool by `Recycle()`.

## How to Use

- Create an object with `Object Pool Manager` component.
- Define your `Object Pools` in the Inspector.
- To get an instance from the pool use `ObjectPoolManager.Give();`
- To send an instance back to the pool `ObjectPoolManager.Recycle();`

## Object Pool Manager Inspector

#### Init Mode

Sets when the manager is going to be initialized. `AWAKE` is recommended. However, you can also choose `START` or `MANUAL`. In order to initialize manually, you have to call `ObjectPoolManager.InitializeObjectPoolManager();` at the desired point.

#### Allow Logs

If you would like to see what is happening in the background you can set it to `True`. Useful for debugging purposes.

#### Add New Object Pool Button

Click this button to add a new Object Pool.

#### Expand/Collapse All Buttons

Click these buttons to expand/collapse the Object Pools.

#### Remove Button

Click this button to remove a certain Object Pool.

#### Pool Summary

Displays the status of the manager.
It additionally displays the number of objects in reserve and  in use for each Object Pool.

## Object Pool Settings

#### Prefab

The prefab `GameObject` you would like to pool.

You can also use a non-prefab GameObject. However you will need to make sure that it is not deleted.

#### Count

Size of the pool. If Auto Increase is allowed this number will change dynamically.

#### Is Particle

If the Prefab has a `Particle System`, and you would like it to automatically recycle after the particle animation is completed, set it to `True`.

It uses the built-in `OnParticleSystemStopped()` event. Make sure that the Prefab does not have another auto-destruct logic other than this.

#### Allow Auto Increase

Sometimes the need for instances can exceed the initial pool size. In order to increase your pool size automatically set it to `True`.

The manager adds more instances to the pool in the background slowly. Feel free to modify `AutoIncreaseWatcher()` Coroutine to make it more aggressive.

#### Auto Increase Threshold Percentage

Auto increasing is triggered if there are less instances than this percentage of the `Count`.

#### Auto Increase Coefficient

When auto increasing is triggered, the manager adds `AutoIncreaseCoefficient * Count` new instances to the pool.

#### Max Instances

Auto increase does not exceed this number, in order to prevent unwanted performance issues.
## FAQ

#### Which Auto Increase Parameters should I use?

This part is quite experimental but the default values should do fine.

Choosing a very high `Threshold Percentage` is likely to cause auto increase logic to be aggressive.

#### Can I add a new Object Pool to the manager after initializing it?

Not at this version. But you can extend the code to meet your needs.
