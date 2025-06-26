namespace CommandLinePlus.GUI.Example.Models.Models;

public enum SecurityOption
{
    NoNewPrivileges,
    AppArmorUnconfined,
    SeccompUnconfined,
    LabelDisable,
    LabelUser,
    LabelRole,
    LabelType,
    LabelLevel
}