using Xunit;
using FluentAssertions;
using TaskManager.Web.Validators;
using TaskManager.Web.DTOs;
using TaskManager.Web.Models;

namespace TaskManager.Web.Tests.Validators;

public class CreateTaskDtoValidatorTests
{
    private readonly CreateTaskDtoValidator _validator;

    public CreateTaskDtoValidatorTests()
    {
        _validator = new CreateTaskDtoValidator();
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "Valid Task",
            Description = "Valid description",
            Priority = Priority.High,
            Category = Category.Work,
            UserId = "user123",
            DueDate = DateTime.Today.AddDays(1)
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "",
            UserId = "user123",
            Priority = Priority.Medium,
            Category = Category.Work
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenTitleExceedsMaxLength()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = new string('a', 201),
            UserId = "user123",
            Priority = Priority.Medium,
            Category = Category.Work
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage.Contains("200"));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "Valid Title",
            Description = new string('a', 2001),
            UserId = "user123",
            Priority = Priority.Medium,
            Category = Category.Work
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description" && e.ErrorMessage.Contains("2000"));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenUserIdIsEmpty()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "Valid Title",
            UserId = "",
            Priority = Priority.Medium,
            Category = Category.Work
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenDueDateIsInThePast()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "Valid Title",
            UserId = "user123",
            Priority = Priority.Medium,
            Category = Category.Work,
            DueDate = DateTime.Today.AddDays(-1)
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DueDate");
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenDueDateIsNull()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "Valid Title",
            UserId = "user123",
            Priority = Priority.Medium,
            Category = Category.Work,
            DueDate = null
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenTagsExceedMaxCount()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "Valid Title",
            UserId = "user123",
            Priority = Priority.Medium,
            Category = Category.Work,
            Tags = Enumerable.Range(1, 11).Select(i => $"Tag{i}").ToList()
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Tags" && e.ErrorMessage.Contains("10"));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenTagExceedsMaxLength()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "Valid Title",
            UserId = "user123",
            Priority = Priority.Medium,
            Category = Category.Work,
            Tags = new List<string> { new string('a', 51) }
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Tags" && e.ErrorMessage.Contains("50"));
    }
}

public class UpdateTaskDtoValidatorTests
{
    private readonly UpdateTaskDtoValidator _validator;

    public UpdateTaskDtoValidatorTests()
    {
        _validator = new UpdateTaskDtoValidator();
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var dto = new UpdateTaskDto
        {
            Id = 1,
            Title = "Updated Task",
            Description = "Updated description",
            Priority = Priority.High,
            Category = Category.Work,
            Completed = false,
            DueDate = DateTime.Today.AddDays(1)
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenIdIsZero()
    {
        // Arrange
        var dto = new UpdateTaskDto
        {
            Id = 0,
            Title = "Valid Title",
            Priority = Priority.Medium,
            Category = Category.Work
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var dto = new UpdateTaskDto
        {
            Id = 1,
            Title = "",
            Priority = Priority.Medium,
            Category = Category.Work
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenTitleExceedsMaxLength()
    {
        // Arrange
        var dto = new UpdateTaskDto
        {
            Id = 1,
            Title = new string('a', 201),
            Priority = Priority.Medium,
            Category = Category.Work
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage.Contains("200"));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenDueDateIsInThePast()
    {
        // Arrange
        var dto = new UpdateTaskDto
        {
            Id = 1,
            Title = "Valid Title",
            Priority = Priority.Medium,
            Category = Category.Work,
            DueDate = DateTime.Today.AddDays(-1)
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DueDate");
    }
}
