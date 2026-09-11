namespace QuinntyneBrownStudio.Application.Blog.Services;

public interface IReadingTimeCalculator
{
    int Calculate(string markdownBody);
}
