namespace Consensus.Models.Commands;

public abstract class Command(string fromRobotId, string toRobotId, int sendTick, float sendStrength)
{
    public string FromRobotId { get; set; } = fromRobotId;
    public string ToRobotId { get; set; } = toRobotId;
    public int SendTick { get; set; } = sendTick;
    public float SendStrength { get; set; } = sendStrength;

    public int CalculatedArrivalTick { get; set; }
    public float CalculatedArrivalStrength { get; set; }
    public float CalculatedArrivalLossRatio { get; set; }

    public abstract void Execute(Robot robot);
}

public class ExampleCommand : Command
{
    private ExampleCommand(string fromRobotId, string toRobotId, int sendTick, float sendSingalLevel) : base(fromRobotId, toRobotId, sendTick, sendSingalLevel) {}

    public static ExampleCommand CreateCommand(string fromRobotId, string toRobotId, int sendTick, float sendSingalLevel)
    {
        return new(fromRobotId, toRobotId, sendTick, sendSingalLevel);
    }

    public override void Execute(Robot robot)
    {
        throw new System.NotImplementedException();
    }
}