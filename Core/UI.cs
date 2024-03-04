namespace AVCoders.Crestron.Core;

public enum Visibility
{
    Unknown,
    Shown,
    Hidden
}

public enum ShutdownMode
{
    Unknown,
    Countdown,
    Timer
}

public delegate void VisibilityChanged(Visibility visibility);
