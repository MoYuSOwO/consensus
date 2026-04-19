using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Consensus.Models.Commands;

public record CommandParam(string Name, Type DataType, object Value);


public abstract class Command
{
    public static readonly Command Dummy = new DummyCommand();

    private sealed class DummyCommand : Command
    {
        internal DummyCommand() : base() {}
        public override void Execute(Robot robot) {}

        public override Dictionary<string, string> ExtraParamsToString() => [];
    }

    public string FromRobotId { get; set; } = "";
    public string ToRobotId { get; set; } = "";
    public int SendTick { get; set; }
    public float SendStrength { get; set; }

    public int CalculatedArrivalTick { get; set; }
    public float CalculatedArrivalStrength { get; set; }
    public float CalculatedArrivalLossRatio { get; set; }

    public abstract void Execute(Robot robot);

    public virtual void SetParam(string key, object value) {}

    public virtual List<CommandParam> GetExtraParams() => [];

    public abstract Dictionary<string, string> ExtraParamsToString();

    public override string ToString()
    {
        var d = ExtraParamsToString();

        string action = CommandFactory.GetCommandName(GetType());
        
        var paramStrings = new List<string>();
		foreach (var e in d)
		{
			paramStrings.Add($"{e.Key}: {e.Value}");
		}
		string extraParams = string.Join(" | ", paramStrings);

        return $"from: @{FromRobotId}, to: @{ToRobotId}, action: {action}, params: {extraParams}";
    }
}

public static class CommandFactory
{
    private static readonly Dictionary<string, Func<Command>> factoryRegistry = new()
    {
        { "Step", () => new StepCommand() },
        { "Rotate", () => new RotateCommand() },
        { "Relay", () => new RelayCommand() },
        { "Dummy", () => Command.Dummy }
    };
    private static readonly Dictionary<Type, string> nameRegistry = new()
    {
        { typeof(StepCommand), "Step" },
        { typeof(RotateCommand), "Rotate" },
        { typeof(RelayCommand), "Relay" },
        { Command.Dummy.GetType(), "Dummy" }
    };

    public static Func<Command> GetCommandFactory(string commandName) => factoryRegistry[commandName];
    public static string GetCommandName(Type type) => nameRegistry[type];
    public static ImmutableArray<string> Names => [.. factoryRegistry.Keys];

    public static void RegisterCommand<T>(string name, Func<T> commandFunc) where T : Command
    {
        factoryRegistry.Add(name, commandFunc);
        nameRegistry.Add(typeof(T), name);
    }
}