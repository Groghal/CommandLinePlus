namespace CommandLinePlus.GUI.Example.Models.Models;

public enum LogDriver
{
    None,
    Json,
    Syslog,
    Journald,
    Gelf,
    Fluentd,
    Awslogs
}