using System;
using System.Numerics;
using Consensus.Models;
using Consensus.Models.Commands;
using Consensus.Nodes;
using Godot;

namespace Consensus.Utils;

public enum Direction
{
    Down = 0,
    Left = 1,
    Up = 2,
    Right = 3
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
            _ => throw new ArgumentException("Why other direction value???")
        };
    }

    public static int ToFrameId(this Direction direction)
    {
        return (int)direction;
    }
}

public struct NetworkResult
{
    public float Dist { get; set; }
    public ValueRange<int> DelayTicks { get; set; }
    public ValueRange<float> Strength { get; set; }
    public ValueRange<float> LossProb { get; set; }
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

    public static ValueRange<T> operator +(ValueRange<T> a, T b)
    {
        return new()
        {
            Value = a.Value + b,
            Min = a.Min + b,
            Max = a.Max + b
        };
    }

    public static ValueRange<T> operator -(ValueRange<T> a, T b)
    {
        return new()
        {
            Value = a.Value - b,
            Min = a.Min - b,
            Max = a.Max - b
        };
    }

    public static ValueRange<T> operator *(ValueRange<T> a, T b)
    {
        return new()
        {
            Value = a.Value * b,
            Min = a.Min * b,
            Max = a.Max * b
        };
    }
}

public static class AlgorithmUtil
{
    public const float GridSize = 64.0f;

    public const float MinBaseNetworkDelay = 0.3f;
    public const float MaxBaseNetworkDelay = 0.7f;

    public const float StrengthPerfect = 4.0f;
    public const float StrengthDead = 0.5f;
    public const float CurvePower = 1.5f;


    public static ValueRange<int> RandomNetworkDelay => new(
        TickManager.SecondToTick(GD.RandRange(MinBaseNetworkDelay, MaxBaseNetworkDelay)),
        TickManager.SecondToTick(MinBaseNetworkDelay),
        TickManager.SecondToTick(MaxBaseNetworkDelay)
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
            TickManager.SecondToTick(GD.RandRange(minSendDelayTime, maxSendDelayTime)),
            TickManager.SecondToTick(minSendDelayTime),
            TickManager.SecondToTick(maxSendDelayTime)
        );
    }

    public static NetworkResult CalculateInf(Robot from, Robot to, Command command)
    {
        // distance
        float dist;
        
        // delay
        ValueRange<int> delayTicks;

        // strength
        ValueRange<float> strength;

        // loss
        ValueRange<float> lossProb;

        if (to == from)
        {
            dist = 0;
            delayTicks = RandomNetworkDelay;
            strength = new(command.SendStrength);
            lossProb = new(0);
        }
        else
        {
            dist = from.GlobalPosition.DistanceTo(to.GlobalPosition) / GridSize;
            delayTicks = RandomNetworkDelay + GetRandomRobotDelay(to);
            strength = GetDecreaseRatio(dist) * command.SendStrength;
            var minLoss = GetLossProb(strength.Min);
            var maxLoss = GetLossProb(strength.Max);
            lossProb = new(GetLossProb(strength.Value).Value, Mathf.Min(minLoss.Min, maxLoss.Min), Mathf.Max(minLoss.Max, maxLoss.Max));
        }

        return new()
        {
            Dist = dist,
            DelayTicks = delayTicks,
            Strength = strength,
            LossProb = lossProb
        };
    }
}