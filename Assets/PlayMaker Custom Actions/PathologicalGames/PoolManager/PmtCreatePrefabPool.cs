// (c) Copyright HutongGames, LLC 2010-2012. All rights reserved.


using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Path-o-logical/Pool Manager 2")]
	[Tooltip("Creates a new PrefabPool in this Pool and instances the requested number of instances (set by preloadAmount)")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W842")]
	public class PmtCreatePrefabPool : FsmStateAction
	{
		[ActionSection("Pool")]
		
		[RequiredField]
		[Tooltip("Pool Name. Creates a new PrefabPool in this Pool ")]
		public FsmString poolName;

		[ActionSection("Set up")]
		
		[RequiredField]
		[Tooltip("Prefab to preload")]
		public FsmGameObject prefab;
		
		[Tooltip("The number of instances to preload")]
		public FsmInt preloadAmount;
		
		[Tooltip("Limits the number of instances allowed in the game. Turning this ON " +
			"means when 'Limit Amount' is hit, no more instances will be created." +
			"CALLS TO SpawnPool.Spawn() WILL BE IGNORED, and return null!")]
		public FsmBool limitInstances;
		
		[Tooltip("This is the max number of instances allowed if 'limitInstances' is ON.")]
		public FsmInt limitAmount;
		
		[ActionSection("Culling. ADVANCED")]
		[Tooltip("Turn this ON to activate the culling feature for this Pool. " +
			"Use this feature to remove despawned (inactive) instances from the pool" +
			"if the size of the pool grows too large. " +
			"DO NOT USE THIS UNLESS YOU NEED TO MANAGE MEMORY ISSUES!")]
		public FsmBool cullDespawned;
		
		[Tooltip("The number of TOTAL (spawned + despawned) instances to keep.")]
		public FsmInt cullAbove;
		
		[Tooltip("The amount of time, in seconds, to wait before culling. This is timed " +
			"from the moment when the Queue's TOTAL count (spawned + despawned) " +
			 "becomes greater than 'Cull Above'. Once triggered, the timer is repeated " +
			  "until the count falls below 'Cull Above'.")]
		public FsmInt cullDelay;
		
		[ActionSection("result")]
		
		
		[Tooltip("Creation successful")]
		public FsmEvent successEvent;
		
		[Tooltip("For completness, trigger this event is the pool is already set with the 'prefab'.")]
		public FsmEvent alreadyExistsEvent;
		
		[Tooltip("Something went wrong")]
		public FsmEvent failureEvent;

		[Tooltip("the actual preloaded amount after creation")]
		public FsmInt preloadedAmount;
		
		private SpawnPool _pool;
		
		public override void Reset ()
		{
			poolName = null;
			prefab = null;
			preloadAmount = 5;      // This is the default so may be omitted
			
			limitInstances = false; // This is the default so may be omitted
			limitAmount = 20;       // This is the default so may be omitted
			
			cullDespawned = false;
			cullAbove = 10;
			cullDelay = 1;
			
			preloadedAmount = 0;
        	
			successEvent = null;
			failureEvent = null;
			alreadyExistsEvent = null;

		}

		public override void  OnEnter ()
		{
			CreatePrefabPool ();
			
			Finish ();
		}

		void CreatePrefabPool ()
		{
			if (poolName.Value == "") {
				Debug.LogError ("Pool name not set");
				if (failureEvent !=null)
				{
					Fsm.Event(failureEvent);
				}
				return;
			}

			/*
			foreach(KeyValuePair<string,SpawnPool> item in PoolManager.Pools)
			{
				Debug.Log(item.Key);
			}
			*/

			//check if the pool doesn't exists
			if (!PoolManager.Pools.TryGetValue( poolName.Value, out _pool)) {
				LogError("Pool does not exists");
				
				if (failureEvent !=null)
				{
					Fsm.Event(failureEvent);
				}
				return;
			}
			
			
				
			
			if (prefab == null) {
				LogWarning("Prefab not set");
				if (failureEvent !=null)
				{
					Fsm.Event(failureEvent);
				}
				return;
				
			}
			
			PrefabPool _prefabPool = new PrefabPool (prefab.Value.transform);
			
			bool isAlreadyPool = _pool.GetPrefab (_prefabPool.prefab) == null ? false : true;
			
			if (alreadyExistsEvent !=null  && isAlreadyPool) {
				Fsm.Event (alreadyExistsEvent);
				return;
			}
			
			_prefabPool.preloadAmount = preloadAmount.Value;
			
			_prefabPool.cullDespawned = cullDespawned.Value;
			_prefabPool.cullAbove = cullAbove.Value;
			_prefabPool.cullDelay = cullDelay.Value;
			_prefabPool.limitInstances = limitInstances.Value;
			_prefabPool.limitAmount = limitAmount.Value;
			
			
			try
			{
				_pool.CreatePrefabPool (_prefabPool);
			}catch(Exception e)
			{
				Debug.LogWarning(e.Message);
				if (failureEvent !=null)
				{
					Fsm.Event(failureEvent);
				}
				return;
			}
			
			preloadedAmount.Value = _pool.Count;
			
			if (successEvent !=null)
			{
				Fsm.Event(successEvent);
				return;
			}
		}
	}
}