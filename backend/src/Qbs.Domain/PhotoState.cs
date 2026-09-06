namespace Qbs.Domain;

public enum PhotoState
{
    Uploading,
    Processing,
    Ready,
    Rejected,
    Failed,
    DeletionPending,
    Deleted,
}
