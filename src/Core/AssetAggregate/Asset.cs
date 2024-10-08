using Ardalis.GuardClauses;
using Ardalis.SharedKernel;
using MiniAssetManagement.Core.DriveAggregate;

namespace MiniAssetManagement.Core.AssetAggregate;

public class Asset : EntityBase, IAggregateRoot
{
    public int? DriveId { get; private set; }
    public int? ParentId { get; private set; }
    public string Name { get; private set; }
    public FileType? FileType { get; private set; }
    public AssetStatus Status { get; private set; } = AssetStatus.Available;

    public Drive? Drive { get; set; }
    public Asset? Parent { get; set; }
    public IEnumerable<Asset> Children => _children.AsReadOnly();
    public IEnumerable<Permission> Permissions => _permissions.AsReadOnly();

    public readonly List<Asset> _children = new();
    private readonly List<Permission> _permissions = new();

    private Asset(string name) => Name = Guard.Against.NullOrEmpty(name, nameof(name));

    public static Asset CreateFolderFromDrive(string name, int driveId) =>
        new(name) { DriveId = driveId, FileType = null };

    public static Asset CreateFolderFromAsset(string name, int assetId) =>
        new(name) { ParentId = assetId, FileType = null };

    public static Asset CreateFileFromDrive(string name, int driveId, FileType type) =>
        new(name) { DriveId = driveId, FileType = type };

    public static Asset CreateFileFromAsset(string name, int assetId, FileType type) =>
        new(name) { ParentId = assetId, FileType = type };

    public void UpdateName(string newName) =>
        Name = Guard.Against.NullOrEmpty(newName, nameof(newName));

    public void UpdateStatus(AssetStatus newStatus) => Status = newStatus;

    public void AddOrUpdatePermission(int userId, PermissionType type)
    {
        var permission = _permissions.Find(p => p.UserId == userId);
        if (permission is null)
            _permissions.Add(new(Id, userId, type));
        else
            permission.SetType(type);
    }

    public void RemovePermissionByUserId(int userId) =>
        _permissions.RemoveAll(p => p.UserId == userId);
}
