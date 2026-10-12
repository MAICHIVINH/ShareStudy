namespace ShareStudy.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string name, object key) : base($"Dữ liệu {name} với khóa ({key}) không tồn tại.") { }
}
