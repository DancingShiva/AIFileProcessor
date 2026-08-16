//-----------------------------------------------------------------------
// <copyright file="WindowsUserContextTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.WinForms.Tests;

using System.IO;
using System.Linq;
using AIFileProcessor.Configuration;
using Xunit;

[Trait("Category", "WinForms")]
public class WindowsUserContextTests
{
    [Fact]
    public void UserId_ReturnsNonEmptyString()
    {
        var ctx = new WindowsUserContext();

        Assert.False(string.IsNullOrWhiteSpace(ctx.UserId));
    }

    [Fact]
    public void UserId_DoesNotContainBackslash()
    {
        var ctx = new WindowsUserContext();

        Assert.DoesNotContain('\\', ctx.UserId);
    }

    [Fact]
    public void UserId_DoesNotContainForwardSlash()
    {
        var ctx = new WindowsUserContext();

        Assert.DoesNotContain('/', ctx.UserId);
    }

    [Fact]
    public void UserId_IsStableAcrossInstances()
    {
        var ctx1 = new WindowsUserContext();
        var ctx2 = new WindowsUserContext();

        Assert.Equal(ctx1.UserId, ctx2.UserId);
    }

    [Fact]
    public void UserId_DoesNotContainInvalidPathChars()
    {
        var ctx = new WindowsUserContext();
        char[] invalid = Path.GetInvalidFileNameChars();

        Assert.DoesNotContain(invalid, ctx.UserId.Contains);
    }

    [Fact]
    public void UserId_DoesNotContainParentDirectorySequence()
    {
        var ctx = new WindowsUserContext();

        Assert.DoesNotContain("..", ctx.UserId);
    }
}