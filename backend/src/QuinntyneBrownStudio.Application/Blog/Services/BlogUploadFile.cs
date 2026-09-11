namespace QuinntyneBrownStudio.Application.Blog.Services;

public sealed record BlogUploadFile(long Length, string FileName, Func<Stream> OpenReadStream);
