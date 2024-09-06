using Ardalis.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;
using MiniAssetManagement.Core.DriveAggregate.Events;
using MiniAssetManagement.Core.DriveAggregate.Handlers;
using MiniAssetManagement.Core.FolderAggregate;
using MiniAssetManagement.Core.FolderAggregate.Events;
using MiniAssetManagement.Core.FolderAggregate.Specifications;
using MiniAssetManagement.UnitTests.Fixtures;
using NSubstitute;

namespace MiniAssetManagement.UnitTests.Core.Handlers;

public class DriveDeletedNotificationHandlerHandle
{
    private readonly ILogger<DriveDeletedEvent> _logger = Substitute.For<
        ILogger<DriveDeletedEvent>
    >();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IRepository<Folder> _folderRepository = Substitute.For<IRepository<Folder>>();
    private DriveDeletedNotificationHandler _handler;

    public DriveDeletedNotificationHandlerHandle() =>
        _handler = new(_logger, _mediator, _folderRepository);

    [Test]
    public async Task DeleteFoldersMatchOwnerId()
    {
        // Given
        var folder1 = Folder.CreateFromDrive("a", 1);
        var folder2 = Folder.CreateFromDrive("b", 1);
        List<Folder> folders = new() { folder1, folder2 };
        _folderRepository
            .ListAsync(Arg.Any<FoldersByDriveIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(folders);

        // When
        await _handler.Handle(new(UserFixture.IdToDelete), CancellationToken.None);

        // Then
        await _folderRepository
            .Received(2)
            .UpdateAsync(Arg.Any<Folder>(), Arg.Any<CancellationToken>());
        await _mediator
            .Received(2)
            .Publish(Arg.Any<FolderDeletedEvent>(), Arg.Any<CancellationToken>());
        Assert.Multiple(() =>
        {
            Assert.That(folder1.Status, Is.EqualTo(FolderStatus.Deleted), nameof(folder1.Status));
            Assert.That(folder2.Status, Is.EqualTo(FolderStatus.Deleted), nameof(folder2.Status));
        });
    }
}
