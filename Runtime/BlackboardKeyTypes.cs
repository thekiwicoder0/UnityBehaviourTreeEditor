using System;
using UnityEngine;

namespace BehaviourTreeBuilder
{
    [Serializable]
    public class BooleanKey : BlackboardKey<bool>
    {
    }

    [Serializable]
    public class IntKey : BlackboardKey<int>
    {
    }

    [Serializable]
    public class FloatKey : BlackboardKey<float>
    {
    }

    [Serializable]
    public class DoubleKey : BlackboardKey<double>
    {
    }

    [Serializable]
    public class StringKey : BlackboardKey<string>
    {
    }

    [Serializable]
    public class Vector2Key : BlackboardKey<Vector2>
    {
    }

    [Serializable]
    public class Vector3Key : BlackboardKey<Vector3>
    {
    }

    [Serializable]
    public class Vector4Key : BlackboardKey<Vector4>
    {
    }

    [Serializable]
    public class Vector2IntKey : BlackboardKey<Vector2Int>
    {
    }

    [Serializable]
    public class Vector3IntKey : BlackboardKey<Vector3Int>
    {
    }

    [Serializable]
    public class GradientKey : BlackboardKey<Gradient>
    {
    }

    [Serializable]
    public class ColorKey : BlackboardKey<Color>
    {
    }

    [Serializable]
    public class LayerKey : BlackboardKey<int>
    {
    }

    [Serializable]
    public class LayerMaskKey : BlackboardKey<LayerMask>
    {
    }

    [Serializable]
    public class TagKey : BlackboardKey<string>
    {
    }

    [Serializable]
    public class CurveKey : BlackboardKey<AnimationCurve>
    {
    }

    [Serializable]
    public class BoundsKey : BlackboardKey<Bounds>
    {
    }

    [Serializable]
    public class BoundsIntKey : BlackboardKey<BoundsInt>
    {
    }

    [Serializable]
    public class GameObjectKey : BlackboardKey<GameObject>
    {
    }

    [Serializable]
    public class MaterialKey : BlackboardKey<Material>
    {
    }

    [Serializable]
    public class RigidBodyKey : BlackboardKey<Rigidbody>
    {
    }

    [Serializable]
    public class ColliderKey : BlackboardKey<Collider>
    {
    }
}