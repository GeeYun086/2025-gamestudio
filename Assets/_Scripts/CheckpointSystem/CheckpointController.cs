using System.Collections.Generic;
using System.Linq;
using GravityGame.Gravity;
using GravityGame.Player;
using GravityGame.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GravityGame.CheckpointSystem
{
    /// <summary>
    ///     Manages all checkpoints in the scene and the player's respawn logic.
    ///     This component should be present in each scene that requires checkpoint functionality.
    ///     How to use:
    ///     1. Populate the 'Game Object Checkpoints' list in the Inspector with GameObjects
    ///     that represent checkpoints. These GameObjects should ideally have the
    ///     <see cref="Checkpoint" /> component already, or it will be added automatically.
    ///     Each checkpoint GameObject must have a Collider component (e.g., BoxCollider, SphereCollider)
    ///     2. If no checkpoints are active when a scene loads (or on initial game start),
    ///     an initial spawn point checkpoint will be created at the player's starting position.
    ///     3. To make the player respawn, call the <see cref="RespawnPlayer" /> method.
    ///     The controller ensures that only one checkpoint is active at any time.
    ///     When a player triggers a new, unreached checkpoint, that checkpoint becomes the active one.
    /// </summary>
    public class CheckpointController : SingletonMonoBehavior<CheckpointController>
    {
      
    }
}