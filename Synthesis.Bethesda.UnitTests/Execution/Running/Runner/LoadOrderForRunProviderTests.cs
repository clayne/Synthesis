﻿using FluentAssertions;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order;
using Noggog;
using NSubstitute;
using Synthesis.Bethesda.Execution.Running.Runner;
using Synthesis.Bethesda.UnitTests.AutoData;
using Xunit;

namespace Synthesis.Bethesda.UnitTests.Execution.Running.Runner;

public class LoadOrderForRunProviderTests
{
    [Theory, SynthAutoData]
    public void ReturnsListFromProvider(
        IEnumerable<ILoadOrderListingGetter> listings,
        IReadOnlySet<ModKey> blacklist,
        ModPath outputPath,
        LoadOrderForRunProvider sut)
    {
        sut.LoadOrderListingsProvider.Get(blacklist).Returns(listings);
        sut.Get(outputPath, blacklist)
            .Should().Equal(listings);
    }
        
    [Theory, SynthAutoData]
    public void TrimsPastOutputPath(
        IEnumerable<ILoadOrderListingGetter> listingsFirst,
        IEnumerable<ILoadOrderListingGetter> listingsSecond,
        IReadOnlySet<ModKey> blacklist,
        ModPath outputPath,
        LoadOrderForRunProvider sut)
    {
        var modListingGetter = new LoadOrderListing(outputPath, true);
        sut.LoadOrderListingsProvider.Get(blacklist).Returns(
            listingsFirst
                .And(modListingGetter)
                .Concat(listingsSecond));
        sut.Get(outputPath, blacklist)
            .Should().Equal(listingsFirst);
    }
}