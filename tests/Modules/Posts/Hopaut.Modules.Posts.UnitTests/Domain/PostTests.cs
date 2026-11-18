using AwesomeAssertions.Execution;
using Hopaut.Modules.Posts.Domain;
using Hopaut.Modules.Posts.Domain.Events;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.UnitTests.Domain;

public sealed class PostTests
{
    private static readonly DateTimeOffset Now = PostBuilder.Now;

    [Fact]
    public void Create_WithValidInput_IsActiveOwnsItsChildrenAndRaisesCreated()
    {
        // Act
        var post = new PostBuilder().WithPictures("https://cdn.hopaut.app/a.webp").Build();

        // Assert
        using (new AssertionScope())
        {
            post.Status.Should().Be(PostStatus.Active);
            post.PostTime.Should().Be(Now);
            post.Event.PostId.Should().Be(post.Id);
            post.Event.Title.Value.Should().Be("Garden party");
            post.Location.PostId.Should().Be(post.Id);
            post.Location.Location.SRID.Should().Be(EventLocation.Wgs84);
            post.Location.Location.X.Should().Be(13.405);
            post.Pictures.Should().ContainSingle(p => p.State == PictureState.Pending && p.PostId == post.Id);
            post.DomainEvents.Should().ContainSingle(e => e is PostCreatedDomainEvent);
        }
    }

    [Theory]
    [InlineData(null, "posts.post.end_required")]
    [InlineData(14, "posts.post.too_short")]
    [InlineData(12 * 60, "posts.post.too_long")]
    public void Create_WithAnInvalidDuration_FailsWithTheScheduleRule(int? minutes, string code)
    {
        // Arrange
        var start = Now.AddHours(1);
        DateTimeOffset? end = minutes is { } m ? start.AddMinutes(m) : null;

        // Act
        var result = new PostBuilder().From(start, end).TryBuild();

        // Assert
        using (new AssertionScope())
        {
            result.Error.Code.Should().Be(code);
            result.Error.Type.Should().Be(ErrorType.Rule);
        }
    }

    [Theory]
    [InlineData(15)]
    [InlineData(12 * 60 - 1)]
    public void Create_WithADurationAtTheLimits_Succeeds(int minutes)
    {
        // Act
        var result = new PostBuilder().From(Now.AddHours(1), Now.AddHours(1).AddMinutes(minutes)).TryBuild();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(30, true)]
    [InlineData(31, false)]
    public void Create_StartingInThePast_IsAllowedForThirtyMinutes(int minutesAgo, bool allowed)
    {
        // Arrange
        var start = Now.AddMinutes(-minutesAgo);

        // Act
        var result = new PostBuilder().From(start, start.AddHours(2)).TryBuild();

        // Assert
        (result.IsSuccess ? null : result.Error.Code).Should().Be(allowed ? null : "posts.post.starts_in_the_past");
    }

    [Fact]
    public void Create_WithRequirementsLongerThan500Characters_FailsValidation()
    {
        // Act
        var result = new PostBuilder().WithRequirements(new string('r', PostErrors.MaxRequirementsLength + 1)).TryBuild();

        // Assert
        result.Error.Code.Should().Be("posts.event.requirements_too_long");
    }

    [Fact]
    public void Create_WithRepeatedTags_LinksEachTagOnce()
    {
        // Act
        var post = new PostBuilder().WithId(9).WithTags(3, 5, 3).Build();

        // Assert
        post.Tags.Select(link => (link.PostId.Value, link.TagId.Value)).Should().Equal((9, 3), (9, 5));
    }

    [Fact]
    public void Create_WithMoreThanTwentyTags_FailsWithTooManyTags()
    {
        // Act
        var result = new PostBuilder().WithTags([.. Enumerable.Range(1, Post.MaxTags + 1)]).TryBuild();

        // Assert
        result.Error.Code.Should().Be("posts.post.too_many_tags");
    }

    [Fact]
    public void Retag_KeepsSharedTagsUnlinksDroppedOnesAndLinksNewOnes()
    {
        // Arrange
        var post = new PostBuilder().WithTags(1, 2).Build();
        var kept = post.Tags[1];

        // Act
        var result = post.Retag([TagId.From(2), TagId.From(3), TagId.From(3)], Now);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            post.Tags.Select(link => link.TagId.Value).Should().Equal(2, 3);
            post.Tags[0].Should().BeSameAs(kept);
        }
    }

    [Fact]
    public void Retag_WithMoreThanTwentyTags_FailsAndKeepsTheTags()
    {
        // Arrange
        var post = new PostBuilder().WithTags(1).Build();

        // Act
        var result = post.Retag(Enumerable.Range(1, Post.MaxTags + 1).Select(TagId.From), Now);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Code.Should().Be("posts.post.too_many_tags");
            post.Tags.Select(link => link.TagId.Value).Should().Equal(1);
        }
    }

    [Fact]
    public void Reschedule_WithAValidSchedule_ReplacesIt()
    {
        // Arrange
        var post = new PostBuilder().Build();
        var schedule = TimeRange.Create(Now.AddDays(2), Now.AddDays(2).AddHours(1)).Value;

        // Act
        var result = post.Reschedule(schedule, Now);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            post.Schedule.Should().Be(schedule);
        }
    }

    [Fact]
    public void Reschedule_BreakingAScheduleRule_KeepsTheOldSchedule()
    {
        // Arrange
        var post = new PostBuilder().Build();
        var original = post.Schedule;

        // Act
        var result = post.Reschedule(TimeRange.Create(Now.AddDays(2), Now.AddDays(2).AddMinutes(5)).Value, Now);

        // Assert
        using (new AssertionScope())
        {
            result.Error.Code.Should().Be("posts.post.too_short");
            post.Schedule.Should().Be(original);
        }
    }

    [Fact]
    public void Disable_WhenActive_DisablesAndRefusesASecondTime()
    {
        // Arrange
        var post = new PostBuilder().Build();

        // Act
        var first = post.Disable();
        var second = post.Disable();

        // Assert
        using (new AssertionScope())
        {
            first.IsSuccess.Should().BeTrue();
            post.Status.Should().Be(PostStatus.Disabled);
            second.Error.Code.Should().Be("posts.post.already_disabled");
        }
    }

    [Fact]
    public void Delete_RaisesDeletedWithThePostId()
    {
        // Arrange
        var post = new PostBuilder().WithId(42).Build();

        // Act
        post.Delete(Now);

        // Assert
        post.DomainEvents.OfType<PostDeletedDomainEvent>().Should().ContainSingle(e => e.PostId == PostId.From(42));
    }

    [Fact]
    public void MarkPictureReady_WhenPending_StoresTheFinalUrlAndRaisesStateChanged()
    {
        // Arrange
        var post = new PostBuilder().Build();
        var picture = post.AttachPicture("https://cdn.hopaut.app/tmp.webp");
        PostBuilder.AssignId(picture, 3);

        // Act
        var result = post.MarkPictureReady(PictureId.From(3), "https://cdn.hopaut.app/final.webp", Now);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            picture.State.Should().Be(PictureState.Ready);
            picture.Url.Should().Be("https://cdn.hopaut.app/final.webp");
            post.DomainEvents.OfType<PictureStateChangedDomainEvent>().Should().ContainSingle(e => e.NewState == PictureState.Ready);
        }
    }

    [Fact]
    public void MarkPictureFailed_AfterItWasProcessed_IsAConflict()
    {
        // Arrange
        var post = new PostBuilder().Build();
        var picture = post.AttachPicture("https://cdn.hopaut.app/tmp.webp");
        PostBuilder.AssignId(picture, 3);
        post.MarkPictureFailed(PictureId.From(3), Now);

        // Act
        var result = post.MarkPictureReady(PictureId.From(3), "https://cdn.hopaut.app/final.webp", Now);

        // Assert
        using (new AssertionScope())
        {
            picture.State.Should().Be(PictureState.Failed);
            result.Error.Code.Should().Be("posts.picture.not_pending");
        }
    }

    [Fact]
    public void MarkPictureReady_ForAnotherPostsPicture_IsNotFound()
    {
        // Arrange
        var post = new PostBuilder().Build();

        // Act
        var result = post.MarkPictureReady(PictureId.From(99), "https://cdn.hopaut.app/final.webp", Now);

        // Assert
        result.Error.Code.Should().Be("posts.picture.not_found");
    }
}
