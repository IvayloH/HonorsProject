/// <summary>
/// RoomExit keeps track of the state of each of the exits in a room - closed/open.
/// And also contains functions to change the state of the exits.
/// </summary>
public class RoomExit
{
    public enum ExitStatus
    {
        Open,
        ClosedDueToCollision,
        Connected,
        Closed
    };

    private ExitStatus connStat;
    private bool isOpen;
    private RoomConnector exit;
    private RoomConnector connectedTo;

    public bool FragmentSpawned { get; set; }
    public bool IsExitOpen() { return isOpen; }
    public bool IsExitConnected() { return connStat.Equals(ExitStatus.Connected); }
    public RoomConnector GetExit() { return exit; }
    public ExitStatus GetConnectionStatus() { return connStat; }
    public void SetConnectionStatus(ExitStatus status) { connStat = status; }
    public RoomConnector GetConnectedTo() { return connectedTo; }

    public RoomExit(RoomConnector exit)
    {
        isOpen = true;
        this.exit = exit;
        FragmentSpawned = false;
    }

    /// <summary>
    /// Closes the exit and sets the connection to the other exit.
    /// </summary>
    /// <param name="connectedTo">The exit to be set as a connection.</param>
    /// <param name="connStatus">Why the exit is being closed.</param>
    public void CloseExit(RoomConnector connectedTo, ExitStatus connStatus = ExitStatus.Connected)
    {
        isOpen = false;
        connStat = connStatus;
        if(connectedTo!=null)
            this.connectedTo = connectedTo;
    }

    /// <summary>
    /// Closes an exit and sets it connection status to 'Connected'.
    /// </summary>
    public void CloseExit()
    {
        isOpen = false;
        connStat = ExitStatus.Connected;
    }

    /// <summary>
    /// Opens an exit and sets it connection status to 'Open'.
    /// </summary>
    public void OpenExit()
    {
        isOpen = true;
        connStat = ExitStatus.Open;
    }
}