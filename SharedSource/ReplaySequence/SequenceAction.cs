using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;
using Kotz.ObjectPool;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.ReplaySequence;

public abstract class SequenceAction :  IMessageSerializable
{
    public enum SequenceType
    {
        PlaySound = 100,
        PostProcessingEffect =101,
        PlayAnimation =111,
        TakeDamage = 102,
        Shoot =103,
        MakeWorldObject =104,
        MoveCamera=105,
        DeleteWorldObject=106,
        Projectile = 107,
        UnitUpdate = 108,
        TileUpdate = 109,
        SpotUnit = 110,
        SpawnParticle = 112,
        
        
        ChangeUnitValues =0,
        Move=2,
        Face=3,
        Crouch=4,
        //UseItem = 5,
        //SelectItem = 6,
        Suppress = 7,
        Overwatch = 9,
        UnitStatusEffect =10,
        //	AbilityToggle = 11,
        DelayedAbilityUse =12,
        
		
		
        Undefined = -1,


      
    }

    private readonly static Dictionary<SequenceType, FluentObjectPool<SequenceAction>> ActionPools = new();

    public virtual Message? MakeTestingMessage()
    {
        return null;
    }

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
        if (t == typeof(WorldObjectManager.TakeDamage))
        {
            return SequenceType.TakeDamage;
        }
        if (t == typeof(WorldObjectManager.MakeWorldObject))
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
        if(t== typeof(Shoot))
        {
            return SequenceType.Shoot;
        }
        if(t== typeof(WorldObjectManager.DeleteWorldObject))
        {
            return SequenceType.DeleteWorldObject;
        }
        if(t== typeof(PlayAnimation))
        {
            return SequenceType.PlayAnimation;
        }
        if (t == typeof(UnitUpdate))
        {
            return SequenceType.UnitUpdate;
        }
        if(t== typeof(ProjectileAction))
        {
            return SequenceType.Projectile;
        }
        if (t == typeof(TileUpdate))
        {
            return SequenceType.TileUpdate;
        }
        if (t == typeof(SpotUnit))
        {
            return SequenceType.SpotUnit;
        }
        if (t == typeof(SpawnParticle))
        {
            return SequenceType.SpawnParticle;
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
                return typeof(WorldObjectManager.TakeDamage);
            case SequenceType.MakeWorldObject:
                return typeof(WorldObjectManager.MakeWorldObject);
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
            case SequenceType.Overwatch:
                return typeof(UnitOverWatch);
            case SequenceType.UnitStatusEffect:
                return typeof(UnitStatusEffect);
            case SequenceType.DelayedAbilityUse:
                return typeof(DelayedAbilityUse);
            case SequenceType.Shoot:
                return typeof(Shoot);
            case SequenceType.DeleteWorldObject:
                return typeof(WorldObjectManager.DeleteWorldObject);
            case SequenceType.PlayAnimation:
                return typeof(PlayAnimation);
            case SequenceType.Projectile:
                return typeof(ProjectileAction);
            case SequenceType.UnitUpdate:
                return typeof(UnitUpdate);
            case SequenceType.TileUpdate:
                return typeof(TileUpdate);
            case SequenceType.SpotUnit:
                return typeof(SpotUnit);
            case SequenceType.SpawnParticle:
                return typeof(SpawnParticle);
            default:
                throw new ArgumentOutOfRangeException(nameof(t), t, null);
        }
        				
        			
    }

    public enum BatchingMode
    {
        BlockingAlone,
        NonBlockingAlone,
        AsyncBatchSameType,
        Sequential,
        AsycnBatchAlways
    }
    public virtual BatchingMode Batching => BatchingMode.BlockingAlone;

    public abstract SequenceType GetSequenceType();
    public bool IsUnitAction => ((int) GetSequenceType()) < 100;
    

    public SequenceAction()
    {
		
    }
	
    public virtual bool ShouldDo()
    {
        return true;
    }


    private bool ran = false;
    public Task GenerateTask()
    {
        Task t = new Task(RunSynchronously);
        return t;
    }
    public void RunSynchronously(){
        if(!Active) throw new Exception("SequenceAction was returned to pool");
        if(ran) throw new Exception("SequenceAction was run twice");
        ran = true;
        Log.Message("SEQUENCE MANAGER","executing sequence action: "+this);
        RunSequenceAction();
        Return();
    }
    protected abstract void RunSequenceAction();


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
    private int randomID;
    protected internal static SequenceAction GetAction(SequenceType type, Message? msg = null)
    {
        //	Console.WriteLine("Getting action of type "+type );
        var act = ActionPools[type].Get();
        if(act == null) throw new Exception("SequenceAction pool is returned null");
        if(msg != null) act.DeserializeArgs(msg);
        act.Active = true;
        act.ran = false;
        act.randomID = Random.Shared.Next();
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

    public bool Active { private set; get; } = false;

    public virtual List<SequenceAction> SendPrerequesteInfoToPlayer(bool player1)
    {
        return new List<SequenceAction>();
    }
    
    public void Return()
    {
        Active = false;

        ActionPools[GetSequenceType()].Return(this);
    }

#if CLIENT
	public virtual void DrawConsequence(Vector2 pos, SpriteBatch batch)
	{

		batch.DrawText(GetSequenceType().ToString(), pos, Color.White);
	}

    public virtual void DrawTooltip(Vector2 pos, float scale, SpriteBatch batch)
    {
        batch.DrawText(GetSequenceType().ToString(),pos, scale,Color.White);
    }
#endif
#if SERVER
    public abstract bool ShouldSendToPlayerServerCheck(bool player1);
    public virtual void FilterForPlayer(bool player1)
    {

    }
    
#endif


    public SequenceAction Clone()
    {
        var msg = Message.Create();
        Serialize(msg);
        return GetAction(GetSequenceType(), msg);
        
    }

   
}