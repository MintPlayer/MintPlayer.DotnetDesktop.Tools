using MintPlayer.BrowserDialog.Presentation;
using MintPlayer.PlatformBrowser;

namespace MintPlayer.BrowserDialog.Tests;

public class BrowserDialogPresenterTests
{
    private static Browser BrowserWith(
        string name = "Firefox",
        string executablePath = @"C:\Program Files\Firefox\firefox.exe",
        string iconPath = @"C:\Program Files\Firefox\firefox.exe",
        int iconIndex = 0) =>
        new()
        {
            Name = name,
            ExecutablePath = executablePath,
            IconPath = iconPath,
            IconIndex = iconIndex,
        };

    private static BrowserDialogPresenter Create(
        FakePlatformBrowser platformBrowser,
        FakeIconExtractor? icons = null,
        FakeImageLoader? images = null) =>
        new(platformBrowser,
            icons ?? new FakeIconExtractor { SplitResult = [FakeIconExtractor.MakeIcon(32)] },
            images ?? new FakeImageLoader());

    [Fact]
    public async Task Builds_one_row_per_installed_browser()
    {
        var source = new FakePlatformBrowser { Browsers = [BrowserWith("Firefox"), BrowserWith("Chrome")] };

        var model = await Create(source).BuildModel();

        Assert.Equal(["Firefox", "Chrome"], model.Entries.Select(e => e.Name));
    }

    [Fact]
    public async Task Trims_the_quotes_off_the_executable_path()
    {
        var source = new FakePlatformBrowser { Browsers = [BrowserWith(executablePath: "\"C:\\firefox.exe\"")] };

        var model = await Create(source).BuildModel();

        Assert.Equal(@"C:\firefox.exe", model.Entries[0].ExecutablePath);
    }

    [Fact]
    public async Task Picks_the_largest_image_inside_the_icon()
    {
        var source = new FakePlatformBrowser { Browsers = [BrowserWith()] };
        var icons = new FakeIconExtractor
        {
            SplitResult = [FakeIconExtractor.MakeIcon(32)],
            ImageWidths = [16, 64, 32],
        };

        var model = await Create(source, icons).BuildModel();

        Assert.Equal(64, model.Entries[0].Icon?.Width);
    }

    [Fact]
    public async Task Loads_a_plain_image_file_rather_than_extracting_it()
    {
        var source = new FakePlatformBrowser { Browsers = [BrowserWith(iconPath: @"C:\browser\logo.png")] };
        var icons = new FakeIconExtractor();
        var images = new FakeImageLoader();

        var model = await Create(source, icons, images).BuildModel();

        Assert.Equal([@"C:\browser\logo.png"], images.LoadCalls);
        Assert.Empty(icons.SplitCalls);
        Assert.NotNull(model.Entries[0].Image);
        Assert.Null(model.Entries[0].Icon);
    }

    [Fact]
    public async Task A_browser_with_no_icon_path_gets_no_artwork()
    {
        var source = new FakePlatformBrowser { Browsers = [BrowserWith(iconPath: string.Empty)] };

        var model = await Create(source).BuildModel();

        Assert.False(model.Entries[0].HasArtwork);
    }

    [Fact]
    public async Task A_negative_icon_index_falls_back_to_the_first_image()
    {
        var source = new FakePlatformBrowser { Browsers = [BrowserWith(iconIndex: -1)] };
        var icons = new FakeIconExtractor { SplitResult = [FakeIconExtractor.MakeIcon(32)] };

        var model = await Create(source, icons).BuildModel();

        Assert.True(model.Entries[0].HasArtwork);
    }

    [Fact]
    public async Task An_icon_index_past_the_end_is_clamped()
    {
        var source = new FakePlatformBrowser { Browsers = [BrowserWith(iconIndex: 99)] };
        var icons = new FakeIconExtractor { SplitResult = [FakeIconExtractor.MakeIcon(32)] };

        var model = await Create(source, icons).BuildModel();

        Assert.True(model.Entries[0].HasArtwork);
    }

    [Fact]
    public async Task One_browser_with_an_unreadable_icon_does_not_cost_the_others_their_row()
    {
        // Regression: the dialog wrapped the whole load in a single try/catch, so the first
        // icon failure abandoned the rest of the list halfway.
        var source = new FakePlatformBrowser { Browsers = [BrowserWith("Broken"), BrowserWith("Chrome")] };
        var icons = new FakeIconExtractor { Failure = new InvalidOperationException("bad icon") };

        var model = await Create(source, icons).BuildModel();

        Assert.Equal(["Broken", "Chrome"], model.Entries.Select(e => e.Name));
        Assert.All(model.Entries, e => Assert.False(e.HasArtwork));
    }

    [Fact]
    public async Task A_failing_scan_yields_an_empty_model_rather_than_throwing()
    {
        var source = new FakePlatformBrowser { ScanFailure = new InvalidOperationException("registry is on fire") };

        var model = await Create(source).BuildModel();

        Assert.Empty(model.Entries);
        Assert.Equal(-1, model.DefaultIndex);
    }

    [Fact]
    public async Task Finds_the_row_matching_the_default_browser()
    {
        var chrome = BrowserWith("Chrome", @"C:\chrome.exe");
        var source = new FakePlatformBrowser
        {
            Browsers = [BrowserWith("Firefox"), chrome],
            DefaultBrowser = chrome,
        };

        var model = await Create(source).BuildModel();

        Assert.Equal(1, model.DefaultIndex);
    }

    [Fact]
    public async Task Matches_the_default_browser_by_path_not_by_reference()
    {
        var source = new FakePlatformBrowser
        {
            Browsers = [BrowserWith("Chrome", @"C:\chrome.exe")],
            // A different instance describing the same install.
            DefaultBrowser = BrowserWith("Chrome", @"C:\chrome.exe"),
        };

        var model = await Create(source).BuildModel();

        Assert.Equal(0, model.DefaultIndex);
    }

    [Fact]
    public async Task Reports_no_default_when_none_is_registered()
    {
        var source = new FakePlatformBrowser { Browsers = [BrowserWith()], DefaultBrowser = null };

        Assert.Equal(-1, (await Create(source).BuildModel()).DefaultIndex);
    }

    [Fact]
    public async Task Reports_no_default_when_looking_it_up_throws()
    {
        var source = new FakePlatformBrowser
        {
            Browsers = [BrowserWith()],
            DefaultFailure = new InvalidOperationException("no url association"),
        };

        var model = await Create(source).BuildModel();

        // The list still renders; only the pre-selection is lost.
        Assert.Single(model.Entries);
        Assert.Equal(-1, model.DefaultIndex);
    }

    [Fact]
    public async Task An_unreadable_image_file_leaves_the_row_without_artwork()
    {
        var source = new FakePlatformBrowser { Browsers = [BrowserWith(iconPath: @"C:\browser\logo.png")] };
        var images = new FakeImageLoader { Result = null };

        var model = await Create(source, images: images).BuildModel();

        Assert.False(model.Entries[0].HasArtwork);
    }
}
