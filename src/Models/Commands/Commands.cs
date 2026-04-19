using Consensus.Nodes;
using Consensus.Utils;

namespace Consensus.Models.Commands;

public class StepCommand(string fromRobotId, string toRobotId, int sendTick, float sendSingalLevel, int length) 
    : Command(fromRobotId, toRobotId, sendTick, sendSingalLevel)
{
    public const int TickPerStep = 2;

    public int Length { get; set; } = length;

    public override void Execute(Robot robot)
    {
        robot.Step(Length);
    }
}

public class RotateCommand(string fromRobotId, string toRobotId, int sendTick, float sendSingalLevel, Direction dir)
    : Command(fromRobotId, toRobotId, sendTick, sendSingalLevel)
{
    public const int RotateTick = 2;

    public Direction Dir { get; set; } = dir;

    public override void Execute(Robot robot)
    {
        robot.RotateTo(Dir);
    }
}

public class RelayCommand(string fromRobotId, string toRobotId, int sendTick, float sendSingalLevel, Command cmd)
    : Command(fromRobotId, toRobotId, sendTick, sendSingalLevel)
{
    public const int RelayTick = 10;

    public Command Cmd { get; set; } = cmd;

    public override void Execute(Robot robot)
    {
        var timer = robot.Wait(TickManager.TickToSecond(RelayTick));
        timer.Timeout += () =>
        {
            Cmd.FromRobotId = robot.RobotId;
            Cmd.SendTick = robot.Ticker.CurrentTick;
            robot.Network.Send(Cmd);
        };
    }
}