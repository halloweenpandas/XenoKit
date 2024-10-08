﻿using Microsoft.Xna.Framework;
using XenoKit.Engine.Vfx.Particle;
using Xv2CoreLib.EEPK;
using Xv2CoreLib.EMP_NEW;

namespace XenoKit.Engine.Pool
{
    public class ObjectPoolManager : Entity
    {
        public readonly PoolInstance<ParticleNodeBase> ParticleNodeBasePool;
        public readonly PoolInstance<Vfx.Particle.ParticleEmitter> ParticleEmitterPool;
        public readonly PoolInstance<ParticlePlane> ParticlePlanePool;
        public readonly PoolInstance<ParticleMesh> ParticleMeshPool;

        public ObjectPoolManager(GameBase game) : base(game)
        {
            //Pool size for base node can be reduced when ShapeDraw, Cone Extrude and Mesh are added, as only Null will use the pool at that point
            ParticleNodeBasePool = new PoolInstance<ParticleNodeBase>(1000, game);
            ParticleEmitterPool = new PoolInstance<Vfx.Particle.ParticleEmitter>(500, game);
            ParticlePlanePool = new PoolInstance<ParticlePlane>(5000, game);
            ParticleMeshPool = new PoolInstance<ParticleMesh>(500, game);
        }

        public override void DelayedUpdate()
        {
            ParticleNodeBasePool.DelayedUpdate();
            ParticleEmitterPool.DelayedUpdate();
            ParticlePlanePool.DelayedUpdate();
        }


        #region Particle Methods

        public ParticleNodeBase GetParticleNodeBase(Matrix emitPoint, Vector3 velocity, ParticleSystem system, ParticleNode node, EffectPart effectPart, object effect)
        {
            ParticleNodeBase newNode = GameBase.ObjectPoolManager.ParticleNodeBasePool.GetObject();
            newNode.Initialize(emitPoint, velocity, system, node, effectPart, effect);
            newNode.Reclaim();
            return newNode;
        }

        public Vfx.Particle.ParticleEmitter GetParticleEmitter(Matrix emitPoint, Vector3 velocity, ParticleSystem system, ParticleNode node, EffectPart effectPart, object effect)
        {
            Vfx.Particle.ParticleEmitter newNode = GameBase.ObjectPoolManager.ParticleEmitterPool.GetObject();
            newNode.Initialize(emitPoint, velocity, system, node, effectPart, effect);
            newNode.Reclaim();
            return newNode;
        }

        public ParticlePlane GetParticlePlane(Matrix emitPoint, Vector3 velocity, ParticleSystem system, ParticleNode node, EffectPart effectPart, object effect)
        {
            ParticlePlane newNode = GameBase.ObjectPoolManager.ParticlePlanePool.GetObject();
            newNode.Initialize(emitPoint, velocity, system, node, effectPart, effect);
            newNode.Reclaim();
            return newNode;
        }

        public ParticleMesh GetParticleMesh(Matrix emitPoint, Vector3 velocity, ParticleSystem system, ParticleNode node, EffectPart effectPart, object effect)
        {
            ParticleMesh newNode = GameBase.ObjectPoolManager.ParticleMeshPool.GetObject();
            newNode.Initialize(emitPoint, velocity, system, node, effectPart, effect);
            newNode.Reclaim();
            return newNode;
        }
        #endregion
    }
}