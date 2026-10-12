namespace ShareStudy.Application.Common.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Phiên đăng nhập không hợp lệ hoặc đã hết hạn.") : base(message) { }
}
