namespace HermesZoneTorba.Domain.HermesManagement;

/// <summary>Lifecycle states of a HermesInstance. See docs/06-hermes-integration.md for the transitions.</summary>
public enum InstanceStatus
{
    NotInstalled,
    Installing,
    Stopped,
    Starting,
    Running,
    Faulted,
    Repairing,
    Uninstalling
}
