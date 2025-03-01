﻿using Microsoft.Xna.Framework;

namespace XenoKit.Engine.View
{
    public interface ICameraBase
    {
        Matrix ViewMatrix { get; }
        Matrix ProjectionMatrix { get; }
        CameraState CameraState { get; }

        Vector2 ProjectToScreenPosition(Vector3 worldPos);

        float DistanceFromCamera(Vector3 worldPos);

        Vector3 TransformRelativeToCamera(Vector3 position, float distanceModifier);

        void ResetViewerAngles();
    }
}
