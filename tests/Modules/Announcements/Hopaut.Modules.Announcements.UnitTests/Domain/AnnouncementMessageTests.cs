using Hopaut.Modules.Announcements.Domain;

namespace Hopaut.Modules.Announcements.UnitTests.Domain;

public sealed class AnnouncementMessageTests
{
    [Theory]
    [InlineData("  Doors open at eight  ", "Doors open at eight")]
    [InlineData("x", "x")]
    [InlineData("\t\u00A0Bring snacks\n", "Bring snacks")]
    public void Create_TrimsTheMessage(string input, string expected) =>
        AnnouncementMessage.Create(input).Value.Value.Should().Be(expected);

    [Fact]
    public void Create_OfTheMaximumLength_IsAccepted() =>
        AnnouncementMessage.Create(new string('x', AnnouncementMessage.MaxLength)).IsSuccess.Should().BeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n\u3000")]
    public void Create_WhenBlank_IsEmpty(string? input) =>
        AnnouncementMessage.Create(input).Error.Should().Be(AnnouncementErrors.MessageEmpty);

    [Fact]
    public void Create_LongerThanTheMaximum_IsTooLong() =>
        AnnouncementMessage.Create(new string('x', AnnouncementMessage.MaxLength + 1)).Error.Should().Be(AnnouncementErrors.MessageTooLong);

    [Theory]
    [InlineData(250, true)]
    [InlineData(251, false)]
    public void Create_CountsUtf16CodeUnits_SoEachEmojiCountsTwice(int emoji, bool accepted) =>
        AnnouncementMessage.Create(string.Concat(Enumerable.Repeat("🎉", emoji))).IsSuccess.Should().Be(accepted);

    [Fact]
    public void ToString_IsTheMessage() =>
        AnnouncementMessage.Create(" Doors open ").Value.ToString().Should().Be("Doors open");
}
