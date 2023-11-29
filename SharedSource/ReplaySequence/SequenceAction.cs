﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.ActorSequenceAction;
using DefconNull.SharedSource.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence.ActorSequenceAction;
using Microsoft.Xna.Framework.Graphics;
using Riptide;
using Kotz.ObjectPool;
using Microsoft.Xna.Framework;


namespace DefconNull.World.WorldObjects.Units.ReplaySequence;

public abstract class SequenceAction :  IMessageSerializable
{
	public enum SequenceType
	{
		PlaySound = 100,
		PostProcessingEffect =101,
		TakeDamage = 102,
		UpdateTile =103,
		MakeWorldObject =104,
		MoveCamera=105,
		
		ChangeUnitValues =0,
		//GiveItem =1,
		Move=2,
		Face=3,
		Crouch=4,
		//UseItem = 5,
		//SelectItem = 6,
		Suppress = 7,
		PlayAnimation =8,
		Overwatch = 9,
		UnitStatusEffect =10,
		//	AbilityToggle = 11,
		DelayedAbilityUse =12,
		UnitShoot =13,
		
		
		Undefined = -1,

		
	}

	private readonly static Dictionary<SequenceType, FluentObjectPool<SequenceAction>> ActionPools = new();

	public static SequenceType TypeToEnum(Type t)
	{
		if (t == typeof(PlaySound))
		{
			return SequenceType.PlaySound;
		}
		if (t == typeof(PostProcessingEffect))
		{
			return SequenceType.PostProcessingEffect;
		}
		if (t == typeof(TakeDamage))
		{
			return SequenceType.TakeDamage;
		}
		if (t == typeof(UpdateTile))
		{
			return SequenceType.UpdateTile;
		}
		if (t == typeof(MakeWorldObject))
		{
			return SequenceType.MakeWorldObject;
		}
		if (t == typeof(MoveCamera))
		{
			return SequenceType.MoveCamera;
		}
		if (t == typeof(ChangeUnitValues))
		{
			return SequenceType.ChangeUnitValues;
		}
		if (t == typeof(UnitMove))
		{
			return SequenceType.Move;
		}
		if (t == typeof(FaceUnit))
		{
			return SequenceType.Face;
		}
		if (t == typeof(CrouchUnit))
		{
			return SequenceType.Crouch;
		}
		if (t == typeof(Suppress))
		{
			return SequenceType.Suppress;
		}
		if (t == typeof(PlayAnimation))
		{
			return SequenceType.PlayAnimation;
		}
		if (t == typeof(UnitOverWatch))
		{
			return SequenceType.Overwatch;
		}
		if (t == typeof(UnitStatusEffect))
		{
			return SequenceType.UnitStatusEffect;
		}
		if (t == typeof(DelayedAbilityUse))
		{
			return SequenceType.DelayedAbilityUse;
		}
		if(t == typeof(UnitSequenceAction))
		{
			return SequenceType.Undefined;
		}
		if(t== typeof(UnitShoot))
		{
			return SequenceType.UnitShoot;
		}
		throw new ArgumentOutOfRangeException(nameof(t), t, null);
	}
	public static Type EnumToType(SequenceType t)
	{
		switch (t)
		{
			case SequenceType.PlaySound:
				return typeof(PlaySound);
			case SequenceType.PostProcessingEffect:
				return typeof(PostProcessingEffect);
			case SequenceType.TakeDamage:
				return typeof(TakeDamage);
			case SequenceType.UpdateTile:
				return typeof(UpdateTile);
			case SequenceType.MakeWorldObject:
				return typeof(MakeWorldObject);
			case SequenceType.MoveCamera:
				return typeof(MoveCamera);
			case SequenceType.ChangeUnitValues:
				return typeof(ChangeUnitValues);
			case SequenceType.Move:
				return typeof(UnitMove);
			case SequenceType.Face:
				return typeof(FaceUnit);
			case SequenceType.Crouch:
				return typeof(CrouchUnit);
			case SequenceType.Suppress:
				return typeof(Suppress);
			case SequenceType.PlayAnimation:
				return typeof(PlayAnimation);
			case SequenceType.Overwatch:
				return typeof(UnitOverWatch);
			case SequenceType.UnitStatusEffect:
				return typeof(UnitStatusEffect);
			case SequenceType.DelayedAbilityUse:
				return typeof(DelayedAbilityUse);
			case SequenceType.UnitShoot:
				return typeof(UnitShoot);
			default:
				throw new ArgumentOutOfRangeException(nameof(t), t, null);
		}
        				
        			
	}

	public virtual bool CanBatch => false;
	public abstract SequenceType GetSequenceType();
	public bool IsUnitAction => (int) GetSequenceType() < 100;

	public SequenceAction()
	{
		
	}
	
	public virtual bool ShouldDo()
	{
		return true;
	}

	public Task GenerateTask()
	{
		Task t = new Task(delegate
		{
			GenerateSpecificTask().RunSynchronously();
			Return();
		});
		return t;
	
	}
	protected abstract Task GenerateSpecificTask();


	protected abstract void SerializeArgs(Message message);	
	
	public void Serialize(Message message)
	{
		SerializeArgs(message);
	}

	public void Deserialize(Message message)
	{
		throw new Exception("cannot deserialize abstract SequenceAction");
	}
#if CLIENT


	public virtual void Preview(SpriteBatch spriteBatch)
	{
	}


#endif


	protected abstract void DeserializeArgs(Message message);

	public static readonly object poolLock = new object();
	protected internal static SequenceAction GetAction(SequenceType type, Message? msg = null)
	{
	//	Console.WriteLine("Getting action of type "+type );
		var act = ActionPools[type].Get();
		if(act == null) throw new Exception("SequenceAction pool is returned null");
		if(msg != null) act.DeserializeArgs(msg);
		act._active = true;
		return act;
		
	}
	
	public static void InitialisePools()
	{
		
		var validator_type = typeof (SequenceAction);

		var sub_validator_types = 
			validator_type
				.Assembly
				.DefinedTypes
				.Where(x => validator_type.IsAssignableFrom(x) && x != validator_type)
				.ToList();
		
		foreach (var subValidatorType in sub_validator_types)
		{
			if(TypeToEnum(subValidatorType) == SequenceType.UpdateTile) continue;//excluded from pooling
			if(TypeToEnum(subValidatorType) == SequenceType.Undefined) continue;//excluded from pooling
			//ActionPools[TypeToEnum(subValidatorType)] = new ObjectPool<SequenceAction>(() => ((SequenceAction) Activator.CreateInstance(subValidatorType)!)!,8,ObjectPoolIsFullPolicy.IncreaseSize);
			ActionPools[TypeToEnum(subValidatorType)] = new FluentObjectPool<SequenceAction>( () =>
			{
			//	Console.WriteLine("CALLING CONSTRUTOR");
				return (SequenceAction) Activator.CreateInstance(subValidatorType)!;
			});
		}
	}
	/*
	public class SequencePolicy<T> : PooledObjectPolicy<T> where T : notnull
	{
		private Func<T> creationFunc;
		public SequencePolicy (Func<T> creationFunc) {

			this.creationFunc = creationFunc;
		}

		/// <inheritdoc />
		public override T Create()
		{
				Console.WriteLine("CALLING CONSTRUTOR");
			return creationFunc();
		}

		/// <inheritdoc />
		public override bool Return(T obj)
		{
			// DefaultObjectPool<T> doesn't call 'Return' for the default policy.
			// So take care adding any logic to this method, as it might require changes elsewhere.
			return true;
		}
	}*/

	private bool _active = true;


	public void Return()
	{
		if(GetSequenceType()==SequenceType.UpdateTile)return;

		//Console.WriteLine("Returning action of type "+GetSequenceType() );
		_active = false;
		ActionPools[GetSequenceType()].Return(this);
			
	}

#if CLIENT
	public virtual void DrawDesc(Vector2 pos, SpriteBatch batch)
	{
		batch.DrawText(GetSequenceType().ToString(), pos, Color.White);
	}
#endif
	
}