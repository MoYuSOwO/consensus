using Consensus.Nodes;
using Consensus.Utils;

namespace Consensus.Models.Commands;

public class StepCommand(string fromRobotId, string toRobotId, int sendTick, float sendSingalLevel, int length) 
    : Command(fromRobotId, toRobotId, sendTick, sendSingalLevel)
{
    public int Length { get; set; } = length;

    public override void Execute(Robot robot)
    {
        robot.Step(Length);
    }
}

public class RotateCommand(string fromRobotId, string toRobotId, int sendTick, float sendSingalLevel, Direction dir)
    : Command(fromRobotId, toRobotId, sendTick, sendSingalLevel)
{
    public Direction Dir { get; set; } = dir;

    public override void Execute(Robot robot)
    {
        robot.RotateTo(Dir);
    }
}

public class RelayCommand(string fromRobotId, string toRobotId, int sendTick, float sendSingalLevel, Command cmd)
    : Command(fromRobotId, toRobotId, sendTick, sendSingalLevel)
{
    public Command Cmd { get; set; } = cmd;

    public override void Execute(Robot robot)
    {
        var timer = robot.Wait(1.0f);
        timer.Timeout += () =>
        {
            Cmd.FromRobotId = robot.RobotId;
            Cmd.SendTick = robot.Ticker.CurrentTick;
            robot.Network.SendPacket(Cmd);
        };
    }
}