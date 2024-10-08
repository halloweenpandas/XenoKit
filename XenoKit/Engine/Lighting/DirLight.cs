﻿using Microsoft.Xna.Framework;
using System;
using Xv2CoreLib.Resource.App;

namespace XenoKit.Engine.Lighting
{
    public class DirLight : Entity
    {
        public Vector3 SpotLightPos = new Vector3(5, 10, 10);

        public Vector4 Position { get; protected set; } = new Vector4(-1f, 0, -1f, 0f);
        public Vector4 Direction { get; protected set; } = DefaultLightDirection;

        //private static Vector4 DefaultLightPosition = new Vector4(0.5f, 0, -0.5f, 0);
        private static Vector4 DefaultLightDirection = new Vector4(0.5f, 0, -0.5f, 0);

        public DirLight(GameBase gameBase) : base(gameBase)
        {

        }

        public override void Update()
        {
            Position = new Vector4(GameBase.ActiveCameraBase.CameraState.Position.X, GameBase.ActiveCameraBase.CameraState.Position.Y, GameBase.ActiveCameraBase.CameraState.Position.Z, 0f);

            if (SettingsManager.settings.XenoKit_EnableDynamicLighting)
            {
                //Direction = Vector4.Transform(new Vector4(-0.415f, 0.1f, -0.5f, 0), GameBase.ActiveCameraBase.TestViewMatrix * GameBase.ActiveCameraBase.ProjectionMatrix);
                Direction = Vector4.Transform(new Vector4(-0.415f, 0.1f, -0.5f, 0), GameBase.ActiveCameraBase.TestViewMatrix * GameBase.ActiveCameraBase.ProjectionMatrix);
                Direction = new Vector4(MathHelper.Clamp(Direction.X * 0.6f, -0.05f, 2f), 0.0f, MathHelper.Clamp(Direction.Z, -1f, 1f), 0f);

            }
            else
            {
                Direction = DefaultLightDirection;
            }

            //Appears to fix an issue with the rim lighting (bleaching nearly the entire character)
            //Even larger multiplications minimize the rimlighting even more, but some instances seem to be unaffected (like Gokus hair)
            Position *= 2f;
        }

        public Vector4 GetLightDirection(int actorSlot)
        {
            //Temp hacky way to handle dynamic lighting. Apply Dynamic lighting to Actor 0 only, as it is broken on all other actors... i think because of their positioning. Need to look into this later.
            if (actorSlot == 0)
                return Direction;

            return DefaultLightDirection;
        }

        public Vector3 GetLightPosition()
        {
            return new Vector3(Position.X, Position.Y, Position.Z);
        }

        public Vector3 GetLightDirection()
        {
            return new Vector3(Direction.X, Direction.Y, Direction.Z);
        }

    }
}
