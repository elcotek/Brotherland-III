using System.Collections.Generic;
using DOL.GS;
using DOL.AI;
using DOL.AI.Brain;
// <copyright file="MLBrainFactory.cs" company="Brotherland Development">Brotherland Development Team</copyright>

using System;
using Microsoft.Pex.Framework;

/// <summary>A factory for MLBrain instances</summary>
public static partial class MLBrainFactory
{
    /// <summary>A factory for MLBrain instances</summary>
    [PexFactoryMethod(typeof(global::MLBrain))]
    public static global::MLBrain Create(
        List<GamePlayer> PlayersSeen_list,
        int IntervalOut_i,
        bool value_b,
        Point3D value_point3D,
        int value_i1,
        int value_i2,
        bool value_b1,
        GameNPC value_gameNPC
    )
    {
        global::MLBrain mLBrain = new global::MLBrain();
        mLBrain.PlayersSeen = PlayersSeen_list;
        
        ((StandardMobBrain)mLBrain).ImmunityState = value_b;
        ((StandardMobBrain)mLBrain).SpawnPoint = value_point3D;
        ((StandardMobBrain)mLBrain).AggroLevel = value_i1;
        ((StandardMobBrain)mLBrain).AggroRange = value_i2;
        ((StandardMobBrain)mLBrain).AggroLOS = value_b1;
        ((ABrain)mLBrain).Body = value_gameNPC;
        return mLBrain;

        // TODO: Edit factory method of MLBrain
        // This method should be able to configure the object in all possible ways.
        // Add as many parameters as needed,
        // and assign their values to each field by using the API.
    }
}
