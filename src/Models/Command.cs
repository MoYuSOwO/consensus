using Godot;

namespace Consensus.Models;

public interface ICommandFactory<T> where T : ICommandFactory<T>
{
    static abstract T CreateCommand(string fromRobotId, string toRobotId, int sendTick, int sendSingLevel);
}

public abstract class Command(string fromRobotId, string toRobotId, int sendTick, int sendSingLevel)
{
    public string FromRobotId { get; init; } = fromRobotId;
    public string ToRobotId { get; init; } = toRobotId;
    public int SendTick { get; init; } = sendTick;
    public int SendSignLevel { get; init; } = sendSingLevel;
    public int CalculatedArrivalTick { get; set; }

    public abstract void Execute(Robot robot);
}

public class ExampleCommand : Command, ICommandFactory<ExampleCommand>
{
    private ExampleCommand(string fromRobotId, string toRobotId, int sendTick, int sendSingalLevel) : base(fromRobotId, toRobotId, sendTick, sendSingalLevel) {}

    public static ExampleCommand CreateCommand(string fromRobotId, string toRobotId, int sendTick, int sendSingalLevel)
    {
        return new(fromRobotId, toRobotId, sendTick, sendSingalLevel);
    }

    public override void Execute(Robot robot)
    {
        throw new System.NotImplementedException();
    }
}