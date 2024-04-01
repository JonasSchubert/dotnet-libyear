using LibYear.Core.FileTypes;
using Xunit;

namespace LibYear.Core.Tests.FileTypes;

public class CsProjFileTests
{
	[Fact]
	public async Task CanLoadCsProjFile()
	{
		//arrange
		var filename = Path.Combine("FileTypes", "project.csproj");

		//act
		var file = new CsProjFile(filename, await File.ReadAllTextAsync(filename));

		//assert
		Assert.Equal("test1", file.Packages.First().Key);
		Assert.Equal("test2", file.Packages.Skip(1).First().Key);
		Assert.Equal("test3", file.Packages.Skip(2).First().Key);
		Assert.Equal("test4", file.Packages.Skip(3).First().Key);
		Assert.Equal("test5", file.Packages.Skip(4).First().Key);
		Assert.Equal("test6", file.Packages.Skip(5).First().Key);
		Assert.Equal("test7", file.Packages.Skip(6).First().Key);
	}

	[Fact]
	public async Task CanUpdateCsProjFile()
	{
		//arrange
		var filename = Path.Combine("FileTypes", "project.csproj");
		var file = new CsProjFile(filename, await File.ReadAllTextAsync(filename));
		var results = new[]
		{
			new Result("test1", new Release(new PackageVersion(0, 1, 0, 1), DateTime.Today), new Release(new PackageVersion(1, 2, 3), DateTime.Today)),
			new Result("test2", new Release(new PackageVersion(0, 2, 0), DateTime.Today), new Release(new PackageVersion(2, 3, 4), DateTime.Today)),
			new Result("test3", new Release(new PackageVersion(0, 3, 0), DateTime.Today), new Release(new PackageVersion(3, 4, 5), DateTime.Today)),
			new Result("test4", new Release(new PackageVersion(0, 4, 0), DateTime.Today), new Release(new PackageVersion(4, 5, 6), DateTime.Today)),
			new Result("test5", new Release(new PackageVersion(0, 5, 0), DateTime.Today), new Release(new PackageVersion(5, 6, 7), DateTime.Today)),
			new Result("test7", new Release(new PackageVersion(0, 7, 0), DateTime.Today), new Release(new PackageVersion(0, 7, 0), DateTime.Today))
		};

		//act
		var updated = file.Update(results);

		//assert
		var newFile = new CsProjFile(filename, updated);
		Assert.Equal("1.2.3", newFile.Packages["test1"]!.ToString());
		Assert.Equal("2.3.4", newFile.Packages["test2"]!.ToString());
		Assert.Equal("3.4.5", newFile.Packages["test3"]!.ToString());
		Assert.Equal("4.5.6", newFile.Packages["test4"]!.ToString());
		Assert.Equal("5.6.7", newFile.Packages["test5"]!.ToString());
		Assert.Null(newFile.Packages["test6"]);
		Assert.Equal(WildcardType.Major, newFile.Packages["test7"]!.WildcardType);
	}

	[Fact]
	public async Task InvalidVersionShowsInfo()
	{
		//arrange
		var filename = Path.Combine("FileTypes", "project.csproj");
		var contents = await File.ReadAllTextAsync(filename);
		var newContents = contents.Replace("0.2.0", "0.2.x");

		try
		{
			//act
			_ = new CsProjFile(filename, newContents);
			Assert.False(true);
		}
		catch (Exception e)
		{
			//assert
			Assert.Contains("0.2.x", e.Message);
			Assert.Contains("test2", e.Message);
			Assert.Contains("project.csproj", e.Message);
		}
	}
}