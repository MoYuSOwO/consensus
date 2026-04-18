using System;
using System.Numerics;
using Consensus.Models;
using Consensus.Nodes;
using Godot;

namespace Consensus.Utils;

public enum Direction
{
    Down = 0,
    Left = 90,
    Up = 180,
    Right = 270
}

public static class DirectionExtensions
{
    public static Vector2I ToVector2I(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => new(0, -1),
            Direction.Down => new(0, 1),
            Direction.Left => new(-1, 0),
            Direction.Right => new(1, 0),
            _ => throw new ArgumentException("Why other direction value???"),
        };
    }

    public static int ToDegree(this Direction direction)
    {
        return (int)direction;
    }

    public static float ToRadians(this Direction direction)
    {
        return Mathf.DegToRad((float)direction);
    }
}

public readonly struct ValueRange<T> where T : INumber<T>
{ 
    public T Value { get; init; }
    public T Min { get; init; }
    public T Max { get; init; }

    public ValueRange()
    {
        Value = T.CreateChecked(0);
        Min = T.CreateChecked(0);
        Max = T.CreateChecked(0);
    }

    public ValueRange(T value)
    {
        Value = value;
        Min = value;
        Max = value;
    }

    public ValueRange(T value, T min, T max)
    {
        Value = value;
        Min = min;
        Max = max;
    }

    public bool IsValid => !T.IsNaN(Value) && !T.IsInfinity(Value) && !T.IsNaN(Min) && !T.IsInfinity(Min) && !T.IsNaN(Max) && !T.IsInfinity(Max) && Min <= Value && Value <= Max;
    public bool IsNumber => IsValid && T.Abs(Max - Min) < T.CreateChecked(1e-9);
    public bool IsZero => IsNumber && T.Abs(Value) == T.CreateChecked(0);

    public static ValueRange<T> operator +(ValueRange<T> a, ValueRange<T> b)
    {
        return new()
        {
            Value = a.Value + b.Value,
            Min = a.Min + b.Min,
            Max = a.Max + b.Max
        };
    }

    public static ValueRange<T> operator -(ValueRange<T> a, ValueRange<T> b)
    {
        return new()
        {
            Value = a.Value - b.Value,
            Min = a.Min - b.Min,
            Max = a.Max - b.Max
        };
    }

    public static ValueRange<T> operator *(ValueRange<T> a, ValueRange<T> b)
    {
        return new()
        {
            Value = a.Value * b.Value,
            Min = a.Min * b.Min,
            Max = a.Max * b.Max
        };
    }
}

public static class AlgorithmUtil
{
    public const float MinBaseNetworkDelay = 0.3f;
    public const float MaxBaseNetworkDelay = 0.7f;

    public const float StrengthPerfect = 4.0f;
    public const float StrengthDead = 0.1f;
    public const float CurvePower = 1.5f;

    public static int ToTick(float second)
    {
        return (int)(second * TickManager.TickPerSecond);
    }

    public static int ToTick(double second)
    {
        return (int)(second * TickManager.TickPerSecond);
    }


    public static ValueRange<int> RandomNetworkDelay => new(
        ToTick(GD.RandRange(MinBaseNetworkDelay, MaxBaseNetworkDelay)),
        ToTick(MinBaseNetworkDelay),
        ToTick(MaxBaseNetworkDelay)
    );

    public static ValueRange<float> GetLossProb(float strength)
    {
        if (strength >= StrengthPerfect) return new();
        else if (strength <= StrengthDead) return new(1);
        var t = (StrengthPerfect - strength) / (StrengthPerfect - StrengthDead);
        var p = Mathf.Pow(t, CurvePower);
        return new(p * (float)GD.RandRange(0.9, 1.1), p * 0.9f, p * 1.1f);
    }

    public static ValueRange<float> GetDecreaseRatio(float dist)
    {
        var r = 1.0f / (1.0f + 0.3f * dist);
        return new(r * (float)GD.RandRange(0.9, 1.1), r * 0.9f, r * 1.1f);
    }

    public static ValueRange<int> GetRandomRobotDelay(Robot robot)
    {
        return GetRandomRobotDelay(robot.MinSendDelayTime, robot.MaxSendDelayTime);
    }

    public static ValueRange<int> GetRandomRobotDelay(float minSendDelayTime, float maxSendDelayTime)
    {
        return new(
            ToTick(GD.RandRange(minSendDelayTime, maxSendDelayTime)),
            ToTick(minSendDelayTime),
            ToTick(maxSendDelayTime)
        );
    }
}