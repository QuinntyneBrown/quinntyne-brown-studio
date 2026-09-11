namespace QuinntyneBrownStudio.Application.Blog.Services;

public interface ISearchHighlighter
{
    string Highlight(string text, string query);
}
