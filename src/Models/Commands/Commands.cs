using System.Collections.Generic;
using Consensus.Nodes;
using Consensus.Utils;

namespace Consensus.Models.Commands;

public class StepCommand : Command
{
    public const int TickPerStep = 2;

    public int Length { get; set; }

    public StepCommand() : base() {}

    public override void Execute(Robot robot)
    {
        robot.Step(Length);
    }

    public override void SetParam(string key, object value)
    {
        if (key == "Length") Length = (int)value;
    }

    public override List<CommandParam> GetExtraParams() => [
        new CommandParam("Length", typeof(int), Length)
    ];

    public override Dictionary<string, string> ExtraParamsToString()
    {
        Dictionary<string, string> d = new()
        {
            { "Length", $"{Length}" }
        };
        return d;
    }
}

public class RotateCommand : Command
{
    public const int RotateTick = 2;

    public Direction Dir { get; set; } = Direction.Down;

    public RotateCommand() : base() {}

    public override void Execute(Robot robot)
    {
        robot.RotateTo(Dir);
    }

    public override void SetParam(string key, object value)
    {
        if (key == "Direction") Dir = (Direction)value;
    }

    public override List<CommandParam> GetExtraParams() => [
        new CommandParam("Direction", typeof(Direction), Dir)
    ];

    public override Dictionary<string, string> ExtraParamsToString()
    {
        Dictionary<string, string> d = new()
        {
            { "Direction", $"{Dir}" }
        };
        return d;
    }
}

public class RelayCommand : Command
{
    public const int RelayTick = 10;

    public int TargetIndex { get; set; } = 0;

    public Command Cmd { get; set; } = Dummy;

    public override void Execute(Robot robot)
    {
        var timer = robot.Wait(TickManager.TickToSecond(RelayTick));
        timer.Timeout += () =>
        {
            Cmd.FromRobotId = robot.RobotId;
            Cmd.SendTick = robot.Ticker.CurrentTick;
            Cmd.SendStrength = SendStrength;
            robot.Network.Send(Cmd);
        };
    }

    public override void SetParam(string key, object value)
    {
        if (key == "Cmd") Cmd = (Command)value;
        if (key == "TargetCommandIndex") TargetIndex = (int)value;
    }

    public override List<CommandParam> GetExtraParams() => [
        new CommandParam("TargetCommandIndex", typeof(int), TargetIndex)
    ];

    public override Dictionary<string, string> ExtraParamsToString()
    {
        Dictionary<string, string> d = new()
        {
            { "TargetCommandIndex", $"{TargetIndex}" }
        };
        return d;
    }

}