namespace ShareStudy.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }

    public ValidationException(IEnumerable<string> errors) : base("Dữ liệu đầu vào không hợp lệ.")
    {
        Errors = errors.ToList();
    }
}
